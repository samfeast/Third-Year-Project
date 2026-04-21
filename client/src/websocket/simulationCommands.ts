// Commands to be sent over the web socket
import type { HeatmapInfo } from "../features/analysis/types";
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

export function getHeatmap(clientId: string, heatmapInfo: HeatmapInfo) {
  websocketClient.send({
    clientId: clientId,
    command: "get-heatmap",
    payload: heatmapInfo,
  });
}
