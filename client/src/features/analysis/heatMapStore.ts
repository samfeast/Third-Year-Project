import type { Heatmap } from "./types";

class HeatmapStore {
  private listeners = new Set<() => void>();
  private heatmap: Heatmap | null = null;

  subscribe = (listener: () => void) => {
    this.listeners.add(listener);
    return () => this.listeners.delete(listener);
  };

  private emit() {
    this.listeners.forEach((l) => l());
  }

  setHeatmap(heatmap: Heatmap | null) {
    this.heatmap = heatmap;
    this.emit();
  }

  getHeatmap(): Heatmap | null {
    return this.heatmap;
  }
}

export const heatmapStore = new HeatmapStore();
