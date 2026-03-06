import { type Snapshot } from "./types";

class SnapshotStore {
  private listeners = new Set<() => void>();

  private bufferSize = 30;
  private buffer: Snapshot[] = [];
  private latest: Snapshot | null = null;

  subscribe = (listener: () => void) => {
    this.listeners.add(listener);
    return () => this.listeners.delete(listener);
  };

  getSnapshot = () => this.latest;

  getBuffer = () => this.buffer;

  setSnapshot = (snapshot: Snapshot) => {
    this.latest = snapshot;

    const next = [...this.buffer, snapshot];

    if (next.length > this.bufferSize) {
      next.shift();
    }

    this.buffer = next;

    this.emit();
  };

  private emit() {
    this.listeners.forEach((l) => l());
  }
}

export const snapshotStore = new SnapshotStore();
