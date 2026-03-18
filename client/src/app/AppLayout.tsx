import { Outlet } from "react-router-dom";
import { useEffect } from "react";

import { useStore } from "../store/StoreProvider";
import { websocketClient } from "../websocket/WebSocketClient";
import Navbar from "../components/Navbar/Navbar";

import styles from "../styles/App.module.css";

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

      <main className={styles["main-content"]}>
        <Outlet />
      </main>
    </div>
  );
}
