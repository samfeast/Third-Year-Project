import type { Point } from "../types/types";
import type { Snapshot } from "../features/simulation/types";

type ServerAgent = {
  id: number;
  position: Point;
  speed: number;
};

type ServerSnapshot = {
  step: number;
  final: boolean;
  agents: ServerAgent[];
};

type ServerSnapshotsPayload = {
  type: "snapshots";
  version: number;
  numSnapshots: number;
  snapshots: ServerSnapshot[];
};

export function convertSnapshots(data: ServerSnapshotsPayload): Snapshot[] {
  if (data.type !== "snapshots") {
    throw new Error("Invalid snapshot file");
  }

  return data.snapshots.map((snapshot) => ({
    step: snapshot.step,
    final: snapshot.final,
    positions: snapshot.agents.map((a) => a.position),
    speeds: snapshot.agents.map((a) => a.speed),
  }));
}
