import { Outlet } from "react-router-dom";
import Navbar from "./components/Navbar";
import { useStore } from "./store/StoreProvider";
import { useEffect } from "react";
import { websocketClient } from "./websocket/WebSocketClient";
import "./App.css";

export default function AppLayout() {
  const { dispatch } = useStore();

  useEffect(() => {
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
