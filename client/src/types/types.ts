import type { Layout } from "../features/layout/types";
import { emptyLayout } from "../features/layout/defaults";

export type Config = {
  agentDensity: number;
  agentStartPositions: Point[];
  seed: number;
  agentRadius: number;
  speedShape: number;
  speedScale: number;
  layout: Layout;
};

export type Point = [number, number];

export const defaultConfig: Config = {
  agentDensity: 0.1,
  agentStartPositions: [],
  seed: 42,
  agentRadius: 225,
  speedShape: 10.14,
  speedScale: 1.41,
  layout: emptyLayout,
};
