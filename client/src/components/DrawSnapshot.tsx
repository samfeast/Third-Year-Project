import { extend } from "@pixi/react";
import { Graphics } from "pixi.js";
import type { Snapshot } from "../store/types";

extend({ Graphics });

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
        snapshot.positions.forEach(([x, y]) => {
          g.circle(x, y, agentRadius);
          g.fill("red");
        });
      }}
    />
  );
}
