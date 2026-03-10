import type { Point } from "../../../types/types";
import type { Layout } from "../types";

const positive: Point[] = [
  [0, 0],
  [2500, 0],
  [2500, 2500],
  [0, 2500],
];

export const preset1: Layout = {
  type: "geometry",
  version: 1,
  positive: positive,
  negatives: [],
};
