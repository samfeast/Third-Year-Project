import { type Action } from "../store/reducer";
import { convertSnapshot } from "../utils/ServerSnapshotConverter";

class WebSocketClient {
  private socket?: WebSocket;
  private dispatch?: React.Dispatch<Action>;
  private connected = false;

  connect(dispatch: React.Dispatch<Action>) {
    if (this.connected) return;

    this.connected = true;
    this.dispatch = dispatch;

    this.socket = new WebSocket("ws://localhost:5158/ws");

    dispatch({ type: "SET_CONNECTION_STATUS", payload: "connecting" });

    this.socket.onopen = () => {
      dispatch({ type: "SET_CONNECTION_STATUS", payload: "connected" });
    };

    this.socket.onclose = () => {
      this.connected = false;
      dispatch({ type: "SET_CONNECTION_STATUS", payload: "disconnected" });
    };

    this.socket.onerror = () => {
      this.connected = false;
      dispatch({ type: "SET_CONNECTION_STATUS", payload: "disconnected" });
    };

    this.socket.onmessage = (event) => {
      const msg = JSON.parse(event.data);

      const snapshot = convertSnapshot(msg);

      this.dispatch?.({
        type: "ADD_SNAPSHOT",
        payload: snapshot,
      });
    };
  }

  send(message: any) {
    this.socket?.send(JSON.stringify(message));
  }
}

export const websocketClient = new WebSocketClient();
