import { type Snapshot } from "./types";

type PlaybackStatus = "playing" | "paused";

class SnapshotStore {
  private listeners = new Set<() => void>();
  private snapshots: Snapshot[] = [];

  private playbackStatus: PlaybackStatus = "paused";

  subscribe = (listener: () => void) => {
    this.listeners.add(listener);
    return () => this.listeners.delete(listener);
  };

  private emit() {
    this.listeners.forEach((l) => l());
  }

  setPlaybackStatus(playbackStatus: PlaybackStatus) {
    this.playbackStatus = playbackStatus;
    this.emit();
  }

  getPlaybackStatus() {
    return this.playbackStatus;
  }

  isPaused() {
    return this.playbackStatus === "paused";
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

  addSnapshots(newSnapshots: Snapshot[]) {
    const last = this.getLastBufferedStep();

    for (const snapshot of newSnapshots) {
      if (last === -1 || snapshot.step > last) {
        this.snapshots.push(snapshot);
      }
    }
  }

  advanceStep() {
    if (this.snapshots.length > 1) {
      this.snapshots.shift();
      this.emit();
    }
  }
}

export const snapshotStore = new SnapshotStore();
