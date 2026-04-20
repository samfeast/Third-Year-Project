import type { Point } from "../../types/types";

type HeatMapTriangle = {
  a: Point;
  b: Point;
  c: Point;
  value: number;
};

export type HeatMap = {
  triangles: HeatMapTriangle[];
};
