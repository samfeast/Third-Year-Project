import type { Point } from "../../types/types";

export type Layout = {
  type: string;
  version: number;
  positive: Point[];
  negatives: Point[][];
};
