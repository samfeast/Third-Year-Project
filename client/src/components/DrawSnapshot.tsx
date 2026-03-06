import { extend } from "@pixi/react";
import { Graphics } from "pixi.js";
import type { Snapshot } from "../store/types";

extend({ Graphics });

const viridisColors = [
  0x440154, 0x481567, 0x482677, 0x453781, 0x404788, 0x39568c, 0x33638d,
  0x2a788e, 0x21918c, 0x22a884, 0x44c36c, 0x7ad151, 0xbddf26, 0xfde725,
];

function speedToColour(speed: number) {
  const MIN_SPEED = 90;
  const MAX_SPEED = 150;
  const t = Math.min(
    Math.max((speed - MIN_SPEED) / (MAX_SPEED - MIN_SPEED), 0),
    1,
  );
  const idx = Math.floor(t * (viridisColors.length - 1));
  return viridisColors[idx];
}

export default function DrawSnapshot({
  snapshot,
  agentRadius,
}: {
  snapshot: Snapshot;
  agentRadius: number;
}) {
  return (
    <pixiGraphics
      draw={(g) => {
        if (snapshot.positions.length === 0) return;
        g.clear();
        snapshot.positions.forEach(([x, y], i) => {
          const speed = snapshot.speeds[i];
          const colour = speedToColour(speed);
          g.circle(x, y, agentRadius);
          g.fill({ color: colour });
        });
      }}
    />
  );
}
