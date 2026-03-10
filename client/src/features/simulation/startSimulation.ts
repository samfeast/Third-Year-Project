import { startPlaybackLoop } from "../simulation/playbackLoop";
import { startSnapshotPolling } from "../simulation/bufferPolling";
import type { Config } from "../../types/types";
import { create } from "../../websocket/simulationCommands";
import { snapshotStore } from "./snapshotStore";

export function startSimulation(clientId: string, config: Config) {
  create(clientId, config);
  startPlaybackLoop();
  startSnapshotPolling(clientId);
  snapshotStore.setPlaybackState("playing");
  snapshotStore.clearSnapshotBuffer();
  snapshotStore.setReceivedFinalStep(false);
}
