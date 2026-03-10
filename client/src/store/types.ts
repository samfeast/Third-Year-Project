import type { Config } from "../types/types";

export type AppState = {
  connectionStatus: "disconnected" | "connecting" | "connected";
  clientId: string;
  config: Config;
};
