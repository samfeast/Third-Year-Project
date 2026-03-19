import type { Snapshot } from "./types";

type PlaybackState = "playing" | "paused";

class SnapshotStore {
  private listeners = new Set<() => void>();
  private snapshots: Snapshot[] = [];

  private playbackState: PlaybackState = "paused";
  private playbackSpeed: number = 1;
  private receivedFinalStep: boolean = false;
  private maxStepIndexDisplayed: number = 0;

  subscribe = (listener: () => void) => {
    this.listeners.add(listener);
    return () => this.listeners.delete(listener);
  };

  private emit() {
    this.listeners.forEach((l) => l());
  }

  setPlaybackState(playbackState: PlaybackState) {
    this.playbackState = playbackState;
  }

  getPlaybackState() {
    return this.playbackState;
  }

  isPaused() {
    return this.playbackState === "paused";
  }

  setPlaybackSpeed(playbackSpeed: number) {
    this.playbackSpeed = playbackSpeed;
  }

  getPlaybackSpeed() {
    return this.playbackSpeed;
  }

  setReceivedFinalStep(state: boolean) {
    this.receivedFinalStep = state;
  }

  hasReceivedFinalStep() {
    return this.receivedFinalStep;
  }

  isPlaybackFinished() {
    return this.receivedFinalStep && this.snapshots.length <= 1;
  }

  getLatestSnapshot(): Snapshot | null {
    return this.snapshots[0] ?? null;
  }

  getCurrentStep(): number {
    return this.snapshots[0]?.step ?? -1;
  }

  getLastBufferedStep(): number {
    return this.snapshots[this.snapshots.length - 1]?.step ?? -1;
  }

  getBufferLength(): number {
    return this.snapshots.length;
  }

  clearSnapshotBuffer() {
    this.snapshots = [];
  }

  getMaxStepIndexDisplayed(): number {
    return this.maxStepIndexDisplayed;
  }

  addSnapshots(newSnapshots: Snapshot[]) {
    const last = this.getLastBufferedStep();

    for (const snapshot of newSnapshots) {
      if (last === -1 || snapshot.step > last) {
        this.snapshots.push(snapshot);
        if (snapshot.final) {
          this.receivedFinalStep = true;
        }
      }
    }
    this.emit();
  }

  advanceStep() {
    if (this.snapshots.length > 1) {
      const next = this.snapshots[1];
      if (next.step > this.maxStepIndexDisplayed) {
        this.maxStepIndexDisplayed = next.step;
      }
      this.snapshots.shift();
      this.emit();
    }
  }
}

export const snapshotStore = new SnapshotStore();
