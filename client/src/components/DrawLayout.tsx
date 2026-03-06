import { Graphics } from "pixi.js";
import type { Layout } from "../store/types";
import { extend } from "@pixi/react";

extend({ Graphics });

export default function DrawLayout({ layout }: { layout: Layout }) {
  return (
    <pixiGraphics
      draw={(g) => {
        g.clear();

        // Outer polygon
        if (layout.positive.length > 0) {
          const [firstX, firstY] = layout.positive[0];
          g.moveTo(firstX, firstY);
          layout.positive.slice(1).forEach(([x, y]) => g.lineTo(x, y));
          g.closePath();
          g.fill({ color: 0xffffff });
          g.stroke({ color: 0x000000, width: 12 });
        }

        // Inner polygons
        layout.negatives.forEach((poly) => {
          if (poly.length === 0) return;
          const [hx, hy] = poly[0];
          g.moveTo(hx, hy);
          poly.slice(1).forEach(([x, y]) => g.lineTo(x, y));
          g.closePath();
          g.fill({ color: 0xcccccc });
          g.stroke({ color: 0x000000, width: 12 });
        });
      }}
    />
  );
}
