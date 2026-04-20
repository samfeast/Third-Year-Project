import { Graphics } from "pixi.js";
import { extend } from "@pixi/react";

import type { HeatMap } from "../../features/analysis/types";

extend({ Graphics });

export default function DrawHeatMap({ heatMap }: { heatMap: HeatMap }) {
  return (
    <pixiGraphics
      draw={(g) => {
        g.clear();

        heatMap.triangles.forEach((triangle) => {
          if (triangle.value === 0) return;
          const a = triangle.a;
          const b = triangle.b;
          const c = triangle.c;
          g.moveTo(a[0], a[1]);
          g.lineTo(b[0], b[1]);
          g.lineTo(c[0], c[1]);
          g.lineTo(a[0], a[1]);
          g.closePath();
          const colour = GetColour(triangle.value);
          g.fill({ color: colour });
          g.stroke({ color: 0x000000, width: 120 });
        });
      }}
    />
  );
}

const viridisColors = [
  0x440154, 0x481567, 0x482677, 0x453781, 0x404788, 0x39568c, 0x33638d,
  0x2a788e, 0x21918c, 0x22a884, 0x44c36c, 0x7ad151, 0xbddf26, 0xfde725,
];

function GetColour(value: number) {
  const idx = Math.floor(value * (viridisColors.length - 1));
  return viridisColors[idx];
}
