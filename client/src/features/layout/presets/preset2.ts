import type { Point } from "../../../types/types";
import type { Layout } from "../types";

const positive: Point[] = [
  [0, 0],
  [10000, 0],
  [10000, -5000],
  [30000, -5000],
  [30000, 0],
  [35000, 0],
  [35000, -10000],
  [40000, -10000],
  [40000, 5000],
  [30000, 5000],
  [30000, 15000],
  [25000, 15000],
  [25000, 25000],
  [40000, 25000],
  [40000, 30000],
  [0, 30000],
  [0, 20000],
  [20000, 20000],
  [20000, 15000],
  [10000, 15000],
  [10000, 10000],
  [0, 10000],
];

const negative1: Point[] = [
  [20000, 10000],
  [25000, 5000],
  [20000, 0],
  [15000, 5000],
];

const negative2: Point[] = [
  [2500, 26000],
  [20000, 26000],
  [20000, 24000],
  [2500, 24000],
];

export const preset2: Layout = {
  type: "geometry",
  version: 1,
  positive: positive,
  negatives: [negative1, negative2],
  exits: [[5000, 5000]],
  name: "Preset 2",
};
