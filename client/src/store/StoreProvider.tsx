import React, { createContext, useReducer, useContext } from "react";
import { reducer, initialState, type Action } from "./reducer";
import { type AppState } from "./types";

type StoreContextType = {
  state: AppState;
  dispatch: React.Dispatch<Action>;
};

const StoreContext = createContext<StoreContextType | undefined>(undefined);

export function StoreProvider({ children }: { children: React.ReactNode }) {
  const [state, dispatch] = useReducer(reducer, initialState);

  return (
    <StoreContext.Provider value={{ state, dispatch }}>
      {children}
    </StoreContext.Provider>
  );
}

export function useStore() {
  const context = useContext(StoreContext);
  if (!context) throw new Error("useStore must be used inside StoreProvider");
  return context;
}
