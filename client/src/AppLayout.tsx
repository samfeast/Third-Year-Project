import { Outlet } from "react-router-dom";
import Navbar from "./components/Navbar";
import { useStore } from "./store/StoreProvider";
import { useEffect } from "react";
import { websocketClient } from "./websocket/WebSocketClient";
import "./App.css";

export default function AppLayout() {
  const { dispatch } = useStore();

  useEffect(() => {
    let clientId = localStorage.getItem("clientId");

    if (!clientId) {
      clientId = crypto.randomUUID();
      localStorage.setItem("clientId", clientId);
    }

    dispatch({
      type: "SET_CLIENT_ID",
      payload: clientId,
    });

    websocketClient.connect(dispatch);
  }, []);

  return (
    <div>
      <Navbar />

      <main className="main-content">
        <Outlet />
      </main>
    </div>
  );
}
