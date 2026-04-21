import { Application, extend } from "@pixi/react";
import { Container, Graphics } from "pixi.js";
import { useEffect, useMemo, useRef } from "react";
import { buildHeatmapMesh } from "../../features/analysis/buildHeatmapMesh";
import type { Heatmap } from "../../features/analysis/types";

extend({ Graphics, Container });

type HeatmapLayerProps = {
  heatmap: Heatmap;
  scale: number;
  offsetX: number;
  offsetY: number;
};

export function HeatmapLayer({
  heatmap,
  scale,
  offsetX,
  offsetY,
}: HeatmapLayerProps) {
  const containerRef = useRef<Container>(null);

  useEffect(() => {
    const container = containerRef.current;
    if (!container) return;

    const mesh = buildHeatmapMesh(heatmap);
    container.addChild(mesh);

    return () => {
      container.removeChild(mesh);
      mesh.destroy();
    };
  }, [heatmap]);

  return (
    <container
      ref={containerRef}
      scale={{ x: scale, y: scale }}
      x={offsetX}
      y={offsetY}
    />
  );
}
