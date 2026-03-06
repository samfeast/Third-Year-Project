import type { Layout, Point } from "../store/types";

const positive: Point[] = [
  [0, 0],
  [6000, 0],
  [6000, 4000],
  [0, 4000],
];

const negative1: Point[] = [
  [800, 400],
  [800, 1800],
  [1200, 1800],
  [1200, 400],
];

const negative2: Point[] = [
  [800, 2200],
  [800, 3600],
  [1200, 3600],
  [1200, 2200],
];

const negative3: Point[] = [
  [2000, 400],
  [2000, 1800],
  [2400, 1800],
  [2400, 400],
];

const negative4: Point[] = [
  [2000, 2200],
  [2000, 3600],
  [2400, 3600],
  [2400, 2200],
];

const negative5: Point[] = [
  [4800, 400],
  [4800, 1800],
  [5200, 1800],
  [5200, 400],
];

const negative6: Point[] = [
  [4800, 2200],
  [4800, 3600],
  [5200, 3600],
  [5200, 2200],
];

const negative7: Point[] = [
  [3600, 400],
  [3600, 1800],
  [4000, 1800],
  [4000, 400],
];

const negative8: Point[] = [
  [3600, 2200],
  [3600, 3600],
  [4000, 3600],
  [4000, 2200],
];

const negative9: Point[] = [
  [2700, 2100],
  [2700, 2800],
  [3300, 2800],
  [3300, 2100],
];

const negative10: Point[] = [
  [2700, 1200],
  [2700, 1900],
  [3300, 1900],
  [3300, 1200],
];

export const preset4: Layout = {
  type: "geometry",
  version: 1,
  positive: positive,
  negatives: [
    negative1,
    negative2,
    negative3,
    negative4,
    negative5,
    negative6,
    negative7,
    negative8,
    negative9,
    negative10,
  ],
};
