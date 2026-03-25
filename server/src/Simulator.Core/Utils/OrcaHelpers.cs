using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core.Utils;

public static class OrcaHelpers
{
    
    private const double EPSILON = 0.00001;
    public struct VelocityObstacle
    {
        // Centre and radius of the truncation arc (the "bottom" of the cone)
        public Vector2 TruncationCentre; // (pB - pA) / tau
        public double TruncationRadius; // (rA + rB) / tau

        // Unit vectors from the origin along each straight leg of the cone
        public Vector2 LeftLegIntersection;
        public Vector2 RightLegIntersection;

        // True if agents are already overlapping - the cone geometry degenerates
        public bool IsOverlapping;
    }

    public static VelocityObstacle GetVelocityObstacle(Vector2Int posA, int radA, Vector2Int posB, int radB, double tau)
    {
        var relPos = (posB - posA).ToVector2();
        var combinedRadius = radA + radB;

        var truncationCenter = relPos / tau;
        var vo = new VelocityObstacle
        {
            TruncationCentre = relPos / tau,
            TruncationRadius = combinedRadius / tau,
        };
        
        // Distance between pA and pB
        var dist = relPos.GetLength();
        if (dist < combinedRadius)
        {
            // Agents already overlapping, handle separately later
            vo.IsOverlapping = true;
            return vo;
        }
        
        var distToIntersection = Math.Sqrt(relPos.GetSquaredLength() - combinedRadius * combinedRadius) / tau;

        // Normalised vector from origin to pB - pA
        var dHat = relPos / dist;

        // Theta is the angle between origin and relPos, and origin to each leg
        var sinTheta = combinedRadius / dist;
        var cosTheta = Math.Sqrt(1f - sinTheta * sinTheta);

        // Rotate dHat counterclockwise by theta for left leg
        vo.LeftLegIntersection = new Vector2(
            (dHat.X * cosTheta - dHat.Y * sinTheta) * distToIntersection,
            (dHat.X * sinTheta + dHat.Y * cosTheta) * distToIntersection
        );

        // Rotate dHat clockwise by theta for right leg
        vo.RightLegIntersection = new Vector2(
            (dHat.X * cosTheta + dHat.Y * sinTheta) * distToIntersection,
            (-dHat.X * sinTheta + dHat.Y * cosTheta) * distToIntersection
        );

        return vo;
    }

    public static (Vector2 closestPoint, Vector2 outwardNormal) GetNearestPointOnBoundary(VelocityObstacle vo,
        Vector2 relativeVelocity)
    {
        var nearestPointLeftLeg = GetNearestPointOnRay(relativeVelocity, vo.LeftLegIntersection);
        var distanceSqToLeftLeg = (nearestPointLeftLeg - relativeVelocity).GetSquaredLength();
        
        var nearestPointRightLeg = GetNearestPointOnRay(relativeVelocity, vo.RightLegIntersection);
        var distanceSqToRightLeg = (nearestPointRightLeg - relativeVelocity).GetSquaredLength();

        var nearestPointArc = new Vector2(double.PositiveInfinity, double.PositiveInfinity);
        var distanceSqToArc = double.PositiveInfinity;
        
        // Case where relative velocity is in the center, so there are infinitely many points that are nearest
        if ((relativeVelocity - vo.TruncationCentre).GetSquaredLength() < EPSILON)
        {
            nearestPointArc = vo.LeftLegIntersection;
            distanceSqToArc = (vo.LeftLegIntersection - relativeVelocity).GetSquaredLength();
        }
        else
        {
            var pointToCircleCenter = relativeVelocity - vo.TruncationCentre;
            var nearestPointCircle = vo.TruncationCentre +
                                     pointToCircleCenter / pointToCircleCenter.GetLength() * vo.TruncationRadius;
            
            // The circle intersection will be on the arc is it's to the right of the vector left intersection to
            // right intersection
            var arcDir = vo.RightLegIntersection - vo.LeftLegIntersection;
            var testDir = nearestPointCircle - vo.LeftLegIntersection;
            
            var cross = arcDir.X * testDir.Y - arcDir.Y * testDir.X;

            if (cross < 0)
            {   
                nearestPointArc = nearestPointCircle;
                distanceSqToArc = (nearestPointArc - relativeVelocity).GetSquaredLength();
            }
        }

        Vector2 closestPoint, outwardNormal;
        if (distanceSqToLeftLeg <= distanceSqToRightLeg && distanceSqToLeftLeg <= distanceSqToArc)
        {
            closestPoint = nearestPointLeftLeg;
            var leftDir = vo.LeftLegIntersection.GetNormalized();
            outwardNormal = new Vector2(-leftDir.Y, leftDir.X);
        }
        else if (distanceSqToRightLeg <= distanceSqToArc)
        {
            closestPoint = nearestPointRightLeg;
            var rightDir = vo.RightLegIntersection.GetNormalized();
            outwardNormal = new Vector2(rightDir.Y, -rightDir.X);
        }
        else
        {
            closestPoint = nearestPointArc;
            outwardNormal = (nearestPointArc - vo.TruncationCentre) / vo.TruncationRadius;
        }

        return (closestPoint, outwardNormal);
    }
    
    // Get the point on a ray from an origin in the specified direction which is nearest to point p
    private static Vector2 GetNearestPointOnRay(Vector2 p, Vector2 start, Vector2 direction)
    {
        if (direction is { X: 0, Y: 0 })
            return start;
        
        var ap = p - start;

        var t = ap.Dot(direction) / direction.GetSquaredLength();

        // Clamp to greater than 0
        t = Math.Max(0, t);

        return new Vector2(start.X + t * direction.X, start.Y + t * direction.Y);
    }
    
    // Override - use in cases where the direction of the ray is from (0,0) to start
    private static Vector2 GetNearestPointOnRay(Vector2 p, Vector2 start) => GetNearestPointOnRay(p, start, start);
}