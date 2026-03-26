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

    public struct HalfPlane
    {
        public Vector2 Point;
        public Vector2 Normal;
    }

    public static VelocityObstacle GetVelocityObstacle(Vector2Int posA, int radA, Vector2Int posB, int radB, double tau)
    {
        var relPos = (posB - posA).ToVector2();
        var combinedRadius = radA + radB;
        
        var vo = new VelocityObstacle
        {
            TruncationCentre = relPos / tau,
            TruncationRadius = combinedRadius / tau,
        };
        
        // Distance between pA and pB
        if ((posB - posA).GetSquaredLength() < combinedRadius * combinedRadius)
        {
            // Agents already overlapping, handle separately later
            vo.IsOverlapping = true;
            return vo;
        }
        
        var dist = relPos.GetLength();
        
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
        if (vo.IsOverlapping)
        {
            throw new ArgumentException("Should not call GetNearestPointOnBoundary with overlapping obstacle");
        }
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
    
    public static Vector2? LinearProgram2(List<HalfPlane> lines, Vector2 vPref)
    {
        var result = vPref;

        for (int i = 0; i < lines.Count; ++i)
        {
            // Continue if the current result doesn't violate HalfPlane i
            if (Vector2.Dot(result - lines[i].Point, lines[i].Normal) >= 0) continue;
            
            // Find the new optimal point on the line of the current constraint
            var newResult = LinearProgram1(lines, i, lines[i], vPref);
                
            if (newResult == null)
            {
                return null; // Infeasible, need 3D fallback
            }
                
            result = newResult.Value;
        }

        return result;
    }
    
    private static Vector2? LinearProgram1(List<HalfPlane> lines, int count, HalfPlane lineI, Vector2 vPref)
    {
        // Direction of the line (perpendicular to the normal)
        var direction = new Vector2(lineI.Normal.Y, -lineI.Normal.X);
        
        // Represent the point on the line as: lineI.Point + t * direction
        var tLeft = double.NegativeInfinity;
        var tRight = double.PositiveInfinity;

        for (int j = 0; j < count; ++j)
        {
            var denominator = Vector2.Dot(direction, lines[j].Normal);
            var numerator = Vector2.Dot(lines[j].Point - lineI.Point, lines[j].Normal);

            if (Math.Abs(denominator) < EPSILON)
            {
                // Lines are parallel. If the point on lineI is outside lines[j], it's infeasible.
                if (numerator > 0) return null;
                continue;
            }

            var t = numerator / denominator;

            if (denominator > 0)
            {
                // Constraint bounds the line from the right
                tRight = Math.Min(tRight, t);
            }
            else
            {
                // Constraint bounds the line from the left
                tLeft = Math.Max(tLeft, t);
            }

            if (tLeft > tRight)
            {
                return null; // Infeasible
            }
        }

        // Project vPref onto the line to find the ideal 't'
        var tPref = Vector2.Dot(direction, vPref - lineI.Point);

        // Clamp the preferred t to our valid range [tLeft, tRight]
        var tResult = Math.Clamp(tPref, tLeft, tRight);

        return lineI.Point + direction * tResult;
    }
}