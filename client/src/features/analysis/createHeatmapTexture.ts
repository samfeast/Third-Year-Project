import { Application, ImageSource, Texture, TextureSource } from "pixi.js";
import type { Heatmap } from "./types";

export function createHeatmapTexture(heatmap: Heatmap): Texture {
  const { width, height, heatmap: values } = heatmap;

  const canvas = new OffscreenCanvas(width, height);
  const ctx = canvas.getContext("2d")!;
  const imageData = ctx.createImageData(width, height);

  for (let i = 0; i < values.length; i++) {
    const byte = Math.round(values[i] * 255);
    imageData.data[i * 4] = byte; // R
    imageData.data[i * 4 + 1] = 0; // G
    imageData.data[i * 4 + 2] = 0; // B
    imageData.data[i * 4 + 3] = 255; // A
  }

  ctx.putImageData(imageData, 0, 0);

  const source = new ImageSource({
    resource: canvas,
    width,
    height,
    format: "rgba8unorm",
    scaleMode: "nearest",
  });

  return new Texture({ source });
}
