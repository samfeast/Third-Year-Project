import { getHeatMap } from "../../websocket/simulationCommands";

export function startSimulation(clientId: string) {
  getHeatMap(clientId);
}
