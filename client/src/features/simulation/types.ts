import type { Point } from "../../types/types";

export type Snapshot = {
  step: number;
  final: boolean;
  positions: Point[];
  speeds: number[];
};

export type PlaybackInfo = {
  lastDisplayedStep: number;
  lastBufferedStep: number;
  playbackSpeed: number;
};
