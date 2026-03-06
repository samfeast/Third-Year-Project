import type { Point, Snapshot } from "../store/types";

type ServerAgent = {
  id: number;
  position: Point;
  speed: number;
};

type ServerSnapshotMessage = {
  version: number;
  snapshot: {
    step: number;
    final: boolean;
    agents: ServerAgent[];
  };
};

export function convertSnapshot(msg: ServerSnapshotMessage): Snapshot {
  const agents = msg.snapshot.agents;

  return {
    step: msg.snapshot.step,
    final: msg.snapshot.final,
    positions: agents.map((a) => a.position),
    speeds: agents.map((a) => a.speed),
  };
}
