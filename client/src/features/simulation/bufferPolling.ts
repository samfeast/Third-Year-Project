import { getSnapshots } from "../../websocket/simulationCommands";
import { snapshotStore } from "./snapshotStore";

let intervalId: number | null = null;

export function startSnapshotPolling(clientId: string) {
  if (intervalId !== null) return;

  intervalId = window.setInterval(() => {
    if (snapshotStore.hasReceivedFinalStep()) {
      stopSnapshotPolling();
      return;
    }

    if (snapshotStore.isPaused()) return;

    getSnapshots(clientId, {
      lastDisplayedStep: snapshotStore.getCurrentStep(),
      lastBufferedStep: snapshotStore.getLastBufferedStep(),
      playbackSpeed: snapshotStore.getPlaybackSpeed(),
    });
  }, 500);
}

export function stopSnapshotPolling() {
  if (intervalId !== null) {
    clearInterval(intervalId);
    intervalId = null;
  }
}
