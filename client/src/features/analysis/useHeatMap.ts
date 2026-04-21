import { useSyncExternalStore } from "react";
import { heatmapStore } from "./heatMapStore";

export function useHeatmap() {
  return useSyncExternalStore(heatmapStore.subscribe, () =>
    heatmapStore.getHeatmap(),
  );
}
