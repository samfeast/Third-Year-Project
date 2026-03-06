// Data store
export type AppState = {
  connectionStatus: "disconnected" | "connecting" | "connected";
  config: Config;
};

export type Config = {
  agentDensity: number;
  layout: Layout;
};

type Layout = {
  type: string;
  version: number;
  positive: Point[];
  negatives: Point[][];
};

type Point = [number, number];

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
