import type { Layout } from "../features/layout/types";
import { emptyLayout } from "../features/layout/defaults";

export type Config = {
  agentDensity: number;
  seed: number;
  layout: Layout;
};

export type Point = [number, number];

export const defaultConfig: Config = {
  agentDensity: 0.1,
  seed: 42,
  layout: emptyLayout,
};
