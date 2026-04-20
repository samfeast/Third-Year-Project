import type { Point } from "../../../types/types";
import type { Layout } from "../types";

const positive: Point[] = [
  [0, 0],
  [60000, 0],
  [60000, 40000],
  [0, 40000],
];

const negative1: Point[] = [
  [8000, 4000],
  [8000, 18000],
  [12000, 18000],
  [12000, 4000],
];

const negative2: Point[] = [
  [8000, 22000],
  [8000, 36000],
  [12000, 36000],
  [12000, 22000],
];

const negative3: Point[] = [
  [20000, 4000],
  [20000, 18000],
  [24000, 18000],
  [24000, 4000],
];

const negative4: Point[] = [
  [20000, 22000],
  [20000, 36000],
  [24000, 36000],
  [24000, 22000],
];

const negative5: Point[] = [
  [48000, 4000],
  [48000, 18000],
  [52000, 18000],
  [52000, 4000],
];

const negative6: Point[] = [
  [48000, 22000],
  [48000, 36000],
  [52000, 36000],
  [52000, 22000],
];

const negative7: Point[] = [
  [36000, 4000],
  [36000, 18000],
  [40000, 18000],
  [40000, 4000],
];

const negative8: Point[] = [
  [36000, 22000],
  [36000, 36000],
  [40000, 36000],
  [40000, 22000],
];

const negative9: Point[] = [
  [27000, 21000],
  [27000, 28000],
  [33000, 28000],
  [33000, 21000],
];

const negative10: Point[] = [
  [27000, 12000],
  [27000, 19000],
  [33000, 19000],
  [33000, 12000],
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
  exits: [
    [44000, 20000],
    [16000, 20000],
  ],
  name: "Preset 4",
};
