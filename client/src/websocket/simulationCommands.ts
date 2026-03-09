// Commands to be sent over the web socket
import type { Config, PlaybackStatus } from "../store/types";
import { websocketClient } from "./WebSocketClient";

export function create(clientId: string, config: Config) {
  websocketClient.send({
    clientId: clientId,
    command: "create",
    payload: config,
  });
}

export function getSnapshots(clientId: string, playbackStatus: PlaybackStatus) {
  websocketClient.send({
    clientId: clientId,
    command: "get-snapshots",
    payload: playbackStatus,
  });
}
