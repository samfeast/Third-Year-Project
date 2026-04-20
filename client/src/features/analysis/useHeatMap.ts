import { useSyncExternalStore } from "react";
import { heatMapStore } from "./heatMapStore";

export function useHeatMap() {
  return useSyncExternalStore(heatMapStore.subscribe, () =>
    heatMapStore.getHeatMap(),
  );
}
