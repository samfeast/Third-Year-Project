import type { Point } from "../../types/types";

export type HeatmapInfo = {
  startStep: number;
  endStep: number;
};

export type Heatmap = {
  origin: Point;
  cellSize: number;
  width: number;
  height: number;
  heatmap: number[];
};
