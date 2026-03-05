// Commands to be sent over the web socket
import { websocketClient } from "./WebSocketClient";

export function sendStatus(status: "idle" | "running" | "finished") {
  websocketClient.send({
    command: "send_status",
    payload: status,
  });
}
