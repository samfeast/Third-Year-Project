import { snapshotStore } from "./snapshotStore";

const FRAME_INTERVAL = 100; // 10 FPS by default

let intervalId: number | null = null;

export function startPlaybackLoop() {
  if (intervalId !== null) return;

  intervalId = window.setInterval(() => {
    if (snapshotStore.isPaused()) return;
    snapshotStore.advanceStep();
  }, FRAME_INTERVAL);
}

export function stopPlaybackLoop() {
  if (intervalId !== null) {
    clearInterval(intervalId);
    intervalId = null;
  }
}
