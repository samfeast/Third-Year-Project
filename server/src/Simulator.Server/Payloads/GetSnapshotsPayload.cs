namespace Simulator.Server.Payloads;

public class GetSnapshotsPayload
{
    public int lastDisplayedStep { get; set; }
    public int lastBufferedStep { get; set; }
    public double playbackSpeed { get; set; }
}