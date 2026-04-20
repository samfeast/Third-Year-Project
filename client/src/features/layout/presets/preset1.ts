import type { Point } from "../../../types/types";
import type { Layout } from "../types";

const positive: Point[] = [
  [0, 0],
  [25000, 0],
  [25000, 25000],
  [0, 25000],
];

export const preset1: Layout = {
  type: "geometry",
  version: 1,
  positive: positive,
  negatives: [],
  exits: [[12500, 12500]],
};
