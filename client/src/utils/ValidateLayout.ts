import type { Layout } from "../features/layout/types";

export function validateLayout(data: any): Layout {
  if (!data || typeof data !== "object") {
    throw new Error("Invalid data");
  }

  if (typeof data.type !== "string") {
    throw new Error("Missing type");
  }

  if (typeof data.version !== "number") {
    throw new Error("Missing version");
  }

  if (!Array.isArray(data.positive)) {
    throw new Error("Invalid positive points");
  }

  if (!Array.isArray(data.negatives)) {
    throw new Error("Invalid negatives");
  }

  const isPoint = (p: any): p is [number, number] =>
    Array.isArray(p) &&
    p.length === 2 &&
    typeof p[0] === "number" &&
    typeof p[1] === "number";

  const positive = data.positive.map((p: any) => {
    if (!isPoint(p)) throw new Error("Invalid positive point");
    return p;
  });

  const negatives = data.negatives.map((poly: any) => {
    if (!Array.isArray(poly)) throw new Error("Invalid negative polygon");
    return poly.map((p: any) => {
      if (!isPoint(p)) throw new Error("Invalid negative point");
      return p;
    });
  });

  const exits = data.exits.map((p: any) => {
    if (!isPoint(p)) throw new Error("Invalid exit point");
    return p;
  });

  const name =
    data.name === undefined
      ? "Custom"
      : typeof data.name === "string"
        ? data.name
        : (() => {
            throw new Error("Invalid name");
          })();

  return {
    type: data.type,
    version: data.version,
    positive,
    negatives,
    exits,
    name,
  };
}
