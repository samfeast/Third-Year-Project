import { defaultConfig, type AppState, type Snapshot } from "./types";

// Update this and reducer() when adding new actions
export type Action =
  | { type: "SET_CONNECTION_STATUS"; payload: AppState["connectionStatus"] }
  | { type: "SET_CONFIG"; payload: AppState["config"] }
  | { type: "ADD_SNAPSHOT"; payload: Snapshot };

// Default (initial) app state
export const initialState: AppState = {
  connectionStatus: "disconnected",
  config: defaultConfig,
  snapshots: [],
};

export function reducer(state: AppState, action: Action): AppState {
  switch (action.type) {
    case "SET_CONNECTION_STATUS":
      return { ...state, connectionStatus: action.payload };

    case "SET_CONFIG":
      return { ...state, config: action.payload };

    case "ADD_SNAPSHOT":
      return {
        ...state,
        snapshots: [...state.snapshots, action.payload],
      };

    default:
      return state;
  }
}
