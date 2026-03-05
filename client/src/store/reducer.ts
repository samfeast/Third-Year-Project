import { type AppState } from "./types";

// Update this and reducer() when adding new actions
export type Action =
  | { type: "SET_CONNECTION_STATUS"; payload: AppState["connectionStatus"] }
  | { type: "SET_SIMULATION"; payload: AppState["simulation"] };

// Default app state
export const initialState: AppState = {
  connectionStatus: "disconnected",
  simulation: {
    id: "",
    status: "idle",
  },
};

export function reducer(state: AppState, action: Action): AppState {
  switch (action.type) {
    case "SET_CONNECTION_STATUS":
      return { ...state, connectionStatus: action.payload };

    case "SET_SIMULATION":
      return { ...state, simulation: action.payload };

    default:
      return state;
  }
}
