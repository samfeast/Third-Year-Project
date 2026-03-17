import React, { useRef, useState } from "react";

import "./TimelineSlider.css";
import { snapshotStore } from "../../features/simulation/snapshotStore";

type TimelineSliderProps = {
  value: number;
  width: number;
  onChange?: (value: number) => void;
};

function getValueFromPosition(position: number) {
  return Math.round(position * snapshotStore.getCurrentStep());
}

export default function TimelineSlider({
  value,
  width,
  onChange,
}: TimelineSliderProps) {
  const trackRef = useRef<HTMLDivElement>(null);

  const [dragPosition, setDragPosition] = useState<number | null>(null);
  const [dragging, setDragging] = useState(false);

  const committedPosition = value;
  const position = dragPosition ?? committedPosition;

  const displayValue = getValueFromPosition(position);

  function clamp(v: number) {
    return Math.max(0, Math.min(1, v));
  }

  function getPositionFromPointer(clientX: number) {
    const rect = trackRef.current!.getBoundingClientRect();

    const p = (clientX - rect.left) / rect.width;

    return clamp(p);
  }

  function handlePointerDown(e: React.PointerEvent) {
    e.currentTarget.setPointerCapture(e.pointerId);

    setDragging(true);

    const p = getPositionFromPointer(e.clientX);
    setDragPosition(p);
  }

  function handlePointerMove(e: React.PointerEvent) {
    if (!dragging) return;

    const p = getPositionFromPointer(e.clientX);
    setDragPosition(p);
  }

  function handlePointerUp(e: React.PointerEvent) {
    if (!dragging) return;

    const p = getPositionFromPointer(e.clientX);

    const newValue = parseFloat(p.toPrecision(2));

    onChange?.(newValue);

    setDragPosition(null);
    setDragging(false);

    e.currentTarget.releasePointerCapture(e.pointerId);
  }

  return (
    <div
      ref={trackRef}
      onPointerDown={handlePointerDown}
      onPointerMove={handlePointerMove}
      onPointerUp={handlePointerUp}
      className="timeline-container"
      style={{
        width,
      }}
    >
      <div
        className="timeline-fill"
        style={{
          left: `${position * 50}%`,
          width: `${position * width}px`,
        }}
      />
      <div
        className="timeline-handle"
        style={{
          left: `${position * 100}%`,
        }}
      />

      {dragging && (
        <div
          className="timeline-tooltip"
          style={{
            left: `${position * 100}%`,
          }}
        >
          {displayValue.toPrecision(2)}
        </div>
      )}
    </div>
  );
}
