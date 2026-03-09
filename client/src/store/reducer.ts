import { defaultConfig, type AppState } from "./types";

// Update this and reducer() when adding new actions
export type Action =
  | { type: "SET_CONNECTION_STATUS"; payload: AppState["connectionStatus"] }
  | { type: "SET_CLIENT_ID"; payload: AppState["clientId"] }
  | { type: "SET_CONFIG"; payload: AppState["config"] };

// Default (initial) app state
export const initialState: AppState = {
  connectionStatus: "disconnected",
  clientId: "",
  config: defaultConfig,
};

export function reducer(state: AppState, action: Action): AppState {
  switch (action.type) {
    case "SET_CONNECTION_STATUS":
      return { ...state, connectionStatus: action.payload };

    case "SET_CLIENT_ID":
      return { ...state, clientId: action.payload };

    case "SET_CONFIG":
      return { ...state, config: action.payload };

    default:
      return state;
  }
}
