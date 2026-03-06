// Data store
export type AppState = {
  connectionStatus: "disconnected" | "connecting" | "connected";
  config: Config;
  snapshots: Snapshot[];
};

export type Config = {
  agentDensity: number;
  layout: Layout;
};

export type Layout = {
  type: string;
  version: number;
  positive: Point[];
  negatives: Point[][];
};

export type Point = [number, number];

export type Snapshot = {
  step: number;
  final: boolean;
  positions: Point[];
  speeds: number[];
};

export const emptyLayout: Layout = {
  type: "geometry",
  version: 1,
  positive: [],
  negatives: [],
};

export const defaultConfig: Config = {
  agentDensity: 0.1,
  layout: emptyLayout,
};
