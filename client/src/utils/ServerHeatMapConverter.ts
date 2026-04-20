import type { HeatMap } from "../features/analysis/types";
import type { Point } from "../types/types";

type HeatMapCell = {
  triangle: Point[];
  value: number;
};

type ServerHeatMapPayload = {
  type: "heatmap";
  version: number;
  cells: HeatMapCell[];
};

export function convertHeatMap(data: ServerHeatMapPayload): HeatMap {
  if (data.type !== "heatmap") {
    throw new Error("Invalid heatmap file");
  }

  const triangles = data.cells.map((cell) => {
    if (cell.triangle.length !== 3) {
      throw new Error("Invalid triangle data");
    }

    const [a, b, c] = cell.triangle;

    return {
      a,
      b,
      c,
      value: cell.value,
    };
  });

  return { triangles };
}
