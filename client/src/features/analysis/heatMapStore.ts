import type { HeatMap } from "./types";

class HeatMapStore {
  private listeners = new Set<() => void>();
  private heatMap: HeatMap | null = null;

  subscribe = (listener: () => void) => {
    this.listeners.add(listener);
    return () => this.listeners.delete(listener);
  };

  private emit() {
    this.listeners.forEach((l) => l());
  }

  setHeatMap(heatMap: HeatMap | null) {
    this.heatMap = heatMap;
    this.emit();
  }

  getHeatMap(): HeatMap | null {
    return this.heatMap;
  }
}

export const heatMapStore = new HeatMapStore();
