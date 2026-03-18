import { snapshotStore } from "./snapshotStore";

const BASE_FRAME_INTERVAL = 100; // 10 FPS by default
let timeoutId: number | null = null;

function playbackStep() {
  if (snapshotStore.isPlaybackFinished()) {
    stopPlaybackLoop();
    return;
  }

  if (!snapshotStore.isPaused()) {
    snapshotStore.advanceStep();
  }

  // Calculate next frame interval based on playback speed
  const speed = snapshotStore.getPlaybackSpeed(); // 0.1 to 10
  const interval = BASE_FRAME_INTERVAL / speed;

  timeoutId = window.setTimeout(playbackStep, interval);
}

export function startPlaybackLoop() {
  if (timeoutId !== null) return;
  playbackStep();
}

export function stopPlaybackLoop() {
  if (timeoutId !== null) {
    clearTimeout(timeoutId);
    timeoutId = null;
  }
}
