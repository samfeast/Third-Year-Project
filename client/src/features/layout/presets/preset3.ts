import type { Point } from "../../../types/types";
import type { Layout } from "../types";

const positive: Point[] = [
  [0, 0],
  [495, 0],
  [495, 1500],
  [505, 1500],
  [505, 0],
  [5000, 0],
  [5000, 5000],
  [0, 5000],
];

const negative1: Point[] = [
  [500, 4500],
  [1600, 4500],
  [1600, 3000],
  [3400, 3000],
  [3400, 4500],
  [4500, 4500],
  [4500, 2000],
  [1950, 2000],
  [1950, 2010],
  [4200, 2010],
  [4200, 2500],
  [1650, 2500],
  [1650, 2000],
  [500, 2000],
];

const negative2: Point[] = [
  [1800, 4500],
  [2400, 4500],
  [2400, 4490],
  [1800, 4490],
];

const negative3: Point[] = [
  [2600, 4500],
  [3200, 4500],
  [3200, 4490],
  [2600, 4490],
];

const negative4: Point[] = [
  [1000, 1500],
  [4500, 1500],
  [4500, 500],
  [2700, 500],
  [2700, 510],
  [4490, 510],
  [4490, 1000],
  [2000, 1000],
  [2000, 500],
  [1000, 500],
];

export const preset3: Layout = {
  type: "geometry",
  version: 1,
  positive: positive,
  negatives: [negative1, negative2, negative3, negative4],
};
