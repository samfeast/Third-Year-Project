import { Graphics } from "pixi.js";
import { extend } from "@pixi/react";

import type { Layout } from "../../features/layout/types";

extend({ Graphics });

export default function DrawLayout({ layout }: { layout: Layout }) {
  return (
    <pixiGraphics
      draw={(g) => {
        g.clear();

        const drawPolygon = (
          points: [number, number][],
          fillColor: number,
          strokeWidth: number,
        ) => {
          if (points.length === 0) return;

          const [firstX, firstY] = points[0];
          g.moveTo(firstX, firstY);
          points.slice(1).forEach(([x, y]) => g.lineTo(x, y));
          g.closePath();

          g.fill({ color: fillColor });
          g.stroke({ color: 0x000000, width: strokeWidth, alignment: 0.5 });
        };

        // Outer polygon
        drawPolygon(layout.positive, 0xffffff, 120);

        // Holes
        layout.negatives.forEach((poly) => {
          drawPolygon(poly, 0x333333, 80);
        });

        // Exits
        layout.exits.forEach(([x, y]) => {
          g.circle(x, y, 675);
          g.fill({ color: "#43b581" });
        });
      }}
    />
  );
}
