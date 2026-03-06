// Commands to be sent over the web socket
import type { Config } from "../store/types";
import { websocketClient } from "./WebSocketClient";

export function create(config: Config) {
  websocketClient.send({
    command: "create",
    payload: config,
  });
}
