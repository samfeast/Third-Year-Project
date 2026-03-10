import type { Point } from "../../../types/types";
import type { Layout } from "../types";

const positive: Point[] = [
  [0, 0],
  [1000, 0],
  [1000, -500],
  [3000, -500],
  [3000, 0],
  [3500, 0],
  [3500, -1000],
  [4000, -1000],
  [4000, 500],
  [3000, 500],
  [3000, 1500],
  [2500, 1500],
  [2500, 2500],
  [4000, 2500],
  [4000, 3000],
  [0, 3000],
  [0, 2000],
  [2000, 2000],
  [2000, 1500],
  [1000, 1500],
  [1000, 1000],
  [0, 1000],
];

const negative1: Point[] = [
  [2000, 1000],
  [2500, 500],
  [2000, 0],
  [1500, 500],
];

const negative2: Point[] = [
  [250, 2600],
  [2000, 2600],
  [2000, 2400],
  [250, 2400],
];

export const preset2: Layout = {
  type: "geometry",
  version: 1,
  positive: positive,
  negatives: [negative1, negative2],
};
