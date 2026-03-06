import { Application, extend } from "@pixi/react";
import type { Layout, Point, Snapshot } from "../store/types";
import DrawSnapshot from "./DrawSnapshot";
import DrawLayout from "./DrawLayout";
import { Container, Graphics } from "pixi.js";

extend({ Graphics, Container });

export default function SimulationCanvas({
  layout,
  snapshot,
}: {
  layout: Layout;
  snapshot?: Snapshot;
}) {
  const canvasWidth = 1200;
  const canvasHeight = 600;

  let minX = Infinity;
  let minY = Infinity;
  let maxX = -Infinity;
  let maxY = -Infinity;

  const processPoint = ([x, y]: Point) => {
    if (x < minX) minX = x;
    if (y < minY) minY = y;
    if (x > maxX) maxX = x;
    if (y > maxY) maxY = y;
  };

  layout.positive.forEach(processPoint);

  const worldWidth = maxX - minX;
  const worldHeight = maxY - minY;

  const scaleX = (0.9 * canvasWidth) / worldWidth;
  const scaleY = (0.9 * canvasHeight) / worldHeight;
  const scale = Math.min(scaleX, scaleY);

  const offsetX = -minX * scale + (canvasWidth - scale * worldWidth) / 2;
  const offsetY = -minY * scale + (canvasHeight - scale * worldHeight) / 2;

  return (
    <Application
      width={canvasWidth}
      height={canvasHeight}
      background={0xaaaaaa}
    >
      <container scale={scale} x={offsetX} y={offsetY}>
        <DrawLayout layout={layout} />

        {snapshot && !snapshot.final && (
          <DrawSnapshot snapshot={snapshot} agentRadius={22.5} />
        )}
      </container>
    </Application>
  );
}
