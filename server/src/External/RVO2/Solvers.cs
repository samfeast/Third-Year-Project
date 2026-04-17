namespace RVO2;

public static class Solvers
{
    /// <summary>Solves a one-dimensional linear program on a specified line
    /// subject to linear constraints defined by lines and a circular
    /// constraint.</summary>
    ///
    /// <returns>True if successful.</returns>
    ///
    /// <param name="lines">Lines defining the linear constraints.</param>
    /// <param name="lineNo">The specified line constraint.</param>
    /// <param name="radius">The radius of the circular constraint.</param>
    /// <param name="optVelocity">The optimization velocity.</param>
    /// <param name="directionOpt">True if the direction should be optimized.
    /// </param>
    /// <param name="result">A reference to the result of the linear program.
    /// </param>
    private static bool LinearProgram1(IList<Line> lines, int lineNo, float radius, Vector2 optVelocity, bool directionOpt,
        ref Vector2 result)
    {
        float dotProduct = lines[lineNo].Point * lines[lineNo].Direction;
        float discriminant = dotProduct * dotProduct + radius * radius - RVOMath.AbsSq(lines[lineNo].Point);

        if (discriminant < 0.0f)
        {
            /* Max speed circle fully invalidates line lineNo. */
            return false;
        }

        float sqrtDiscriminant = MathF.Sqrt(discriminant);
        float tLeft = -dotProduct - sqrtDiscriminant;
        float tRight = -dotProduct + sqrtDiscriminant;

        for (int i = 0; i < lineNo; ++i)
        {
            float denominator = RVOMath.Det(lines[lineNo].Direction, lines[i].Direction);
            float numerator = RVOMath.Det(lines[i].Direction, lines[lineNo].Point - lines[i].Point);

            if (MathF.Abs(denominator) <= RVOMath.RVO_EPSILON)
            {
                /* Lines lineNo and i are (almost) parallel. */
                if (numerator < 0.0f)
                {
                    return false;
                }

                continue;
            }

            float t = numerator / denominator;

            if (denominator >= 0.0f)
            {
                /* Line i bounds line lineNo on the right. */
                tRight = Math.Min(tRight, t);
            }
            else
            {
                /* Line i bounds line lineNo on the left. */
                tLeft = Math.Max(tLeft, t);
            }

            if (tLeft > tRight)
            {
                return false;
            }
        }

        if (directionOpt)
        {
            /* Optimize direction. */
            if (optVelocity * lines[lineNo].Direction > 0.0f)
            {
                /* Take right extreme. */
                result = lines[lineNo].Point + tRight * lines[lineNo].Direction;
            }
            else
            {
                /* Take left extreme. */
                result = lines[lineNo].Point + tLeft * lines[lineNo].Direction;
            }
        }
        else
        {
            /* Optimize closest point. */
            float t = lines[lineNo].Direction * (optVelocity - lines[lineNo].Point);

            if (t < tLeft)
            {
                result = lines[lineNo].Point + tLeft * lines[lineNo].Direction;
            }
            else if (t > tRight)
            {
                result = lines[lineNo].Point + tRight * lines[lineNo].Direction;
            }
            else
            {
                result = lines[lineNo].Point + t * lines[lineNo].Direction;
            }
        }

        return true;
    }

    /// <summary>Solves a two-dimensional linear program subject to linear
    /// constraints defined by lines and a circular constraint.</summary>
    ///
    /// <returns>The number of the line it fails on, and the number of lines
    /// if successful.</returns>
    ///
    /// <param name="lines">Lines defining the linear constraints.</param>
    /// <param name="radius">The radius of the circular constraint.</param>
    /// <param name="optVelocity">The optimization velocity.</param>
    /// <param name="directionOpt">True if the direction should be optimized.
    /// </param>
    /// <param name="result">A reference to the result of the linear program.
    /// </param>
    public static int LinearProgram2(IList<Line> lines, float radius, Vector2 optVelocity, bool directionOpt,
        out Vector2 result)
    {
        if (directionOpt)
        {
            /*
             * Optimize direction. Note that the optimization velocity is of
             * unit length in this case.
             */
            result = optVelocity * radius;
        }
        else if (RVOMath.AbsSq(optVelocity) > radius * radius)
        {
            /* Optimize closest point and outside circle. */
            result = RVOMath.Normalize(optVelocity) * radius;
        }
        else
        {
            /* Optimize closest point and inside circle. */
            result = optVelocity;
        }

        for (int i = 0; i < lines.Count; ++i)
        {
            if (RVOMath.Det(lines[i].Direction, lines[i].Point - result) > 0.0f)
            {
                /* Result does not satisfy constraint i. Compute new optimal result. */
                Vector2 tempResult = result;
                if (!LinearProgram1(lines, i, radius, optVelocity, directionOpt, ref result))
                {
                    result = tempResult;

                    return i;
                }
            }
        }

        return lines.Count;
    }

    /// <summary>Solves a two-dimensional linear program subject to linear
    /// constraints defined by lines and a circular constraint.</summary>
    ///
    /// <param name="lines">Lines defining the linear constraints.</param>
    /// <param name="numObstLines">Count of obstacle lines.</param>
    /// <param name="beginLine">The line on which the 2-d linear program
    /// failed.</param>
    /// <param name="radius">The radius of the circular constraint.</param>
    /// <param name="result">A reference to the result of the linear program.
    /// </param>
    public static void LinearProgram3(IList<Line> lines, int numObstLines, int beginLine, float radius, ref Vector2 result)
    {
        float distance = 0.0f;

        for (int i = beginLine; i < lines.Count; ++i)
        {
            if (RVOMath.Det(lines[i].Direction, lines[i].Point - result) > distance)
            {
                /* Result does not satisfy constraint of line i. */
                IList<Line> projLines = new List<Line>();
                for (int ii = 0; ii < numObstLines; ++ii)
                {
                    projLines.Add(lines[ii]);
                }

                for (int j = numObstLines; j < i; ++j)
                {
                    Line line;

                    float determinant = RVOMath.Det(lines[i].Direction, lines[j].Direction);

                    if (MathF.Abs(determinant) <= RVOMath.RVO_EPSILON)
                    {
                        /* Line i and line j are parallel. */
                        if (lines[i].Direction * lines[j].Direction > 0.0f)
                        {
                            /* Line i and line j point in the same direction. */
                            continue;
                        }
                        else
                        {
                            /* Line i and line j point in opposite direction. */
                            line.Point = 0.5f * (lines[i].Point + lines[j].Point);
                        }
                    }
                    else
                    {
                        line.Point = lines[i].Point +
                                     (RVOMath.Det(lines[j].Direction, lines[i].Point - lines[j].Point) / determinant) *
                                     lines[i].Direction;
                    }

                    line.Direction = RVOMath.Normalize(lines[j].Direction - lines[i].Direction);
                    projLines.Add(line);
                }

                Vector2 tempResult = result;
                if (LinearProgram2(projLines, radius, new Vector2(-lines[i].Direction.Y, lines[i].Direction.X), true,
                        out result) < projLines.Count)
                {
                    /*
                     * This should in principle not happen. The result is by
                     * definition already in the feasible region of this
                     * linear program. If it fails, it is due to small
                     * floating point error, and the current result is kept.
                     */
                    result = tempResult;
                }

                distance = RVOMath.Det(lines[i].Direction, lines[i].Point - result);
            }
        }
    }
}