using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core;

public struct SimulationSnapshot(int n, int step)
{
    public int Step = step;
    public int StoredAgents = 0;
    public int[] Ids = new int[n];
    public Vector2[] Positions = new Vector2[n];
    public bool AllComplete;

    public void AddAgent(int id, Vector2 position)
    {
        Ids[StoredAgents] = id;
        Positions[StoredAgents] = position;
        StoredAgents++;
    }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine($"SimulationSnapshot");
        sb.AppendLine($"  Step: {Step}");
        sb.AppendLine($"  StoredAgents: {StoredAgents}");
        sb.AppendLine($"  AllComplete: {AllComplete}");

        int previewCount = Math.Min(StoredAgents, 5);

        if (previewCount > 0)
        {
            sb.AppendLine($"  Agents (showing {previewCount} of {StoredAgents}):");

            for (int i = 0; i < previewCount; i++)
            {
                sb.AppendLine($"    [{i}] Id={Ids[i]}, Pos={Positions[i]}");
            }

            if (StoredAgents > previewCount)
                sb.AppendLine("    ...");
        }

        return sb.ToString();
    }
}