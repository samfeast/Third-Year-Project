import {
  Mesh,
  MeshGeometry,
  Shader,
  GlProgram,
  Application,
  TextureSource,
  Texture,
  ImageSource,
} from "pixi.js";
import { createHeatmapTexture } from "./createHeatmapTexture";
import type { Heatmap } from "./types";

export function buildHeatmapMesh(heatmap: Heatmap) {
  const { width, height, heatmap: values } = heatmap;

  const canvas = new OffscreenCanvas(width, height);
  const ctx = canvas.getContext("2d")!;
  const imageData = ctx.createImageData(width, height);

  for (let y = 0; y < height; y++) {
    for (let x = 0; x < width; x++) {
      const i = y * width + x;

      const flippedY = height - 1 - y;

      const srcIndex = x * height + flippedY;

      const value = values[srcIndex];
      const invalid = value === 0;

      imageData.data[i * 4] = invalid ? 0 : Math.round(value * 255);
      imageData.data[i * 4 + 1] = invalid ? 0 : 255;
      imageData.data[i * 4 + 2] = 0;
      imageData.data[i * 4 + 3] = 255;
    }
  }

  ctx.putImageData(imageData, 0, 0);

  const source = new ImageSource({ resource: canvas });
  const texture = new Texture({ source });

  const geometry = new MeshGeometry({
    positions: new Float32Array([0, 0, width, 0, width, height, 0, height]),
    uvs: new Float32Array([0, 0, 1, 0, 1, 1, 0, 1]),
    indices: new Uint32Array([0, 1, 2, 0, 2, 3]),
  });

  const vertexSrc = `#version 300 es
in vec2 aPosition;
in vec2 aUV;
out vec2 vUV;

uniform mat3 uProjectionMatrix;
uniform mat3 uWorldTransformMatrix;
uniform mat3 uTransformMatrix;

void main() {
  mat3 mvp = uProjectionMatrix * uWorldTransformMatrix * uTransformMatrix;
  gl_Position = vec4((mvp * vec3(aPosition, 1.0)).xy, 0.0, 1.0);
  vUV = aUV;
}`;

  const fragmentSrc = `#version 300 es
precision mediump float;
in vec2 vUV;
out vec4 fragColor;
uniform sampler2D uTexture;

vec4 heatColour(float t) {
  vec3 viridis[14];
  viridis[0]  = vec3(0.267, 0.005, 0.329);
  viridis[1]  = vec3(0.282, 0.086, 0.404);
  viridis[2]  = vec3(0.282, 0.153, 0.467);
  viridis[3]  = vec3(0.271, 0.216, 0.498);
  viridis[4]  = vec3(0.251, 0.278, 0.533);
  viridis[5]  = vec3(0.224, 0.337, 0.549);
  viridis[6]  = vec3(0.200, 0.392, 0.553);
  viridis[7]  = vec3(0.165, 0.471, 0.557);
  viridis[8]  = vec3(0.129, 0.569, 0.549);
  viridis[9]  = vec3(0.133, 0.659, 0.518);
  viridis[10] = vec3(0.267, 0.765, 0.424);
  viridis[11] = vec3(0.478, 0.820, 0.318);
  viridis[12] = vec3(0.741, 0.875, 0.149);
  viridis[13] = vec3(0.992, 0.906, 0.145);

  vec3 background = vec3(1.0, 1.0, 1.0); // 0x1a1a1a

  if (t < 0.1) {
    // Interpolate from background to viridis[0] as t goes 0 -> 0.4
    float frac = t / 0.1;
    return vec4(mix(background, viridis[0], frac), 1.0);
  }

  // Remap 0.4-1.0 to 0.0-1.0 for the full viridis range
  float scaled = ((t - 0.1) / 0.9) * 13.0;
  int idx = int(scaled);
  int next = min(idx + 1, 13);
  float frac = scaled - float(idx);

  return vec4(mix(viridis[idx], viridis[next], frac), 1.0);
}

void main() {
  vec4 sampleT = texture(uTexture, vUV);

  if (sampleT.g < 0.5) {
    fragColor = vec4(0.0, 0.0, 0.0, 0.0); // transparent
    return;
  }

  fragColor = heatColour(sampleT.r);
}`;

  const shader = new Shader({
    glProgram: new GlProgram({ vertex: vertexSrc, fragment: fragmentSrc }),
    resources: {
      uTexture: texture.source,
    },
  });

  return new Mesh({ geometry, shader });
}
