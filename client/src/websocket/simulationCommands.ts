// Commands to be sent over the web socket
import type { PlaybackInfo } from "../features/simulation/types";
import type { Config } from "../types/types";

import { websocketClient } from "./WebSocketClient";

export function create(clientId: string, config: Config) {
  websocketClient.send({
    clientId: clientId,
    command: "create",
    payload: config,
  });
}

export function getSnapshots(clientId: string, playbackInfo: PlaybackInfo) {
  websocketClient.send({
    clientId: clientId,
    command: "get-snapshots",
    payload: playbackInfo,
  });
}
