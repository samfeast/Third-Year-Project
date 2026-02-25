// useSimulationWebSocket.ts
import { useState, useEffect, useRef } from "react";
import type { Agent } from "../components/DrawAgents";
import type { Snapshot } from "../App";

// viridis colors sampled as 256 values
const viridisColors = [
  0x440154, 0x481567, 0x482677, 0x453781, 0x404788, 0x39568c,
  0x33638d, 0x2a788e, 0x21918c, 0x22a884, 0x44c36c, 0x7ad151,
  0xbddf26, 0xfde725
  // for demo, you can expand to full 256 color map later
];

function speedToColour(speed: number) {
    const MIN_SPEED = 90
    const MAX_SPEED = 150
    const t = Math.min(Math.max((speed - MIN_SPEED) / (MAX_SPEED - MIN_SPEED), 0), 1);
    const idx = Math.floor(t * (viridisColors.length - 1));
    return viridisColors[idx];
}

export function useSimulationWebSocket(serverUrl: string) {
  const [currentSnapshot, setCurrentSnapshot] = useState<Snapshot>({step: 0, agents: []});
  const ws = useRef<WebSocket | null>(null);
  const snapshotBuffer = useRef<Snapshot[]>([]); // queue of snapshots

  useEffect(() => {
    ws.current = new WebSocket(serverUrl);

    ws.current.onopen = () => console.log("WebSocket connected");
    ws.current.onclose = () => console.log("WebSocket disconnected");
    ws.current.onerror = (err) => console.error("WebSocket error", err);

    ws.current.onmessage = (event) => {
        try {
        const data = JSON.parse(event.data);
        if (data.snapshot && data.snapshot.agents && typeof data.snapshot.step === "number") {
            const agents: Agent[] = data.snapshot.agents.map((a: any) => ({
                id: a.id,
                x: a.position[0],
                y: a.position[1],
                colour: speedToColour(a.speed),
            }));

            const snapshot: Snapshot = {
                agents,
                step: data.snapshot.step
            }

            console.log(snapshot)

            snapshotBuffer.current.push(snapshot);
        }
      } catch (err) {
        console.error("Failed to parse message", err);
      }
    };

    return () => ws.current?.close();
  }, [serverUrl]);

  // Animation loop to play snapshots at fixed rate
  useEffect(() => {
    const fps = 10;
    const interval = 1000 / fps;

    const timer = setInterval(() => {
      if (snapshotBuffer.current.length > 0) {
        const nextSnapshot = snapshotBuffer.current.shift();
        if (nextSnapshot) setCurrentSnapshot(nextSnapshot);
      }
    }, interval);

    return () => clearInterval(timer);
  }, []);

  const sendCommand = (command: string, preset?: number) => {
    if (!ws.current || ws.current.readyState !== WebSocket.OPEN) return;
    const message: any = { command };
    if (preset !== undefined) message.preset = preset;
    ws.current.send(JSON.stringify(message));
  };

  return { snapshot: currentSnapshot, sendCommand };
}