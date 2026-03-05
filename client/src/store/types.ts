// Data store
export type AppState = {
  connectionStatus: "disconnected" | "connecting" | "connected";
  simulation: {
    id: string;
    status: "idle" | "running" | "finished";
  };
};
