import React, { useRef, useState } from "react";

import PlaybackSpeedIcon from "../PlaybackSpeedIcon/PlaybackSpeedIcon";

import "./LogSliderVertical.css";

type LogSliderVerticalProps = {
  min: number;
  max: number;
  value: number;
  height: number;
  onChange?: (value: number) => void;
};

function valueToPosition(value: number, min: number, max: number) {
  return Math.log(value / min) / Math.log(max / min);
}

function positionToValue(p: number, min: number, max: number) {
  return min * Math.pow(max / min, p);
}

export default function LogSliderVertical({
  min,
  max,
  value,
  height,
  onChange,
}: LogSliderVerticalProps) {
  const trackRef = useRef<HTMLDivElement>(null);

  const [dragPosition, setDragPosition] = useState<number | null>(null);
  const [dragging, setDragging] = useState(false);

  const committedPosition = valueToPosition(value, min, max);
  const position = dragPosition ?? committedPosition;

  const displayValue = positionToValue(position, min, max);

  function clamp(v: number) {
    return Math.max(0, Math.min(1, v));
  }

  function getPositionFromPointer(clientY: number) {
    const rect = trackRef.current!.getBoundingClientRect();

    const p = 1 - (clientY - rect.top) / rect.height;

    return clamp(p);
  }

  function handlePointerDown(e: React.PointerEvent) {
    e.currentTarget.setPointerCapture(e.pointerId);

    setDragging(true);

    const p = getPositionFromPointer(e.clientY);
    setDragPosition(p);
  }

  function handlePointerMove(e: React.PointerEvent) {
    if (!dragging) return;

    const p = getPositionFromPointer(e.clientY);
    setDragPosition(p);
  }

  function handlePointerUp(e: React.PointerEvent) {
    if (!dragging) return;

    const p = getPositionFromPointer(e.clientY);

    const newValue = parseFloat(positionToValue(p, min, max).toPrecision(2));

    onChange?.(newValue);

    setDragPosition(null);
    setDragging(false);

    e.currentTarget.releasePointerCapture(e.pointerId);
  }

  return (
    <div className="slider-container">
      <div
        ref={trackRef}
        onPointerDown={handlePointerDown}
        onPointerMove={handlePointerMove}
        onPointerUp={handlePointerUp}
        className="slider-box"
        style={{
          height,
        }}
      >
        <div
          className="fill"
          style={{
            bottom: `${position * 50}%`,
            height: `${position * height}px`,
          }}
        />
        <div
          className="handle"
          style={{
            bottom: `${position * 100}%`,
          }}
        />

        {dragging && (
          <div
            className="tooltip"
            style={{
              bottom: `${position * 100}%`,
            }}
          >
            {displayValue.toPrecision(2)}x
          </div>
        )}
      </div>
      <PlaybackSpeedIcon size={22} strokeWidth={2} />
    </div>
  );
}
