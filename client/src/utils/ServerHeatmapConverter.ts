import type { Heatmap } from "../features/analysis/types";
import type { Point } from "../types/types";

type ServerHeatmapPayload = {
  type: "heatmap";
  version: number;
  origin: Point;
  cellSize: number;
  width: number;
  height: number;
  heatmap: number[];
};

export function convertHeatmap(data: ServerHeatmapPayload): Heatmap {
  if (data.type !== "heatmap") {
    throw new Error("Invalid heatmap file");
  }

  return {
    origin: data.origin,
    cellSize: data.cellSize,
    width: data.width,
    height: data.height,
    heatmap: data.heatmap,
  };
}
