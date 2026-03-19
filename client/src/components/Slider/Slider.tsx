import React, { useRef, useState } from "react";

import sliderStyles from "./Slider.module.css";

type SliderProps = {
  position: number; // Position in [0,1], intentionally normalised to allow non-linear mappings via formatPosition
  width: number; // Pixel width of track (side to side)
  length: number; // Pixel length of track (end to end)
  orientation: "vertical" | "horizontal";
  disabled?: boolean;
  onDragStart?: () => void;
  onChangeCommitted?: (position: number) => void;
  formatPosition?: (position: number) => string; // Function to convert position in range [0,1] to display value
  styles?: SliderStyles;
  icon?: {
    element: React.ReactNode;
    position: "min" | "max"; // Determine which end of the slider to place the icon
    gap: number; // Pixel gap betwen icon and track
  };
};

// Colours should be in hex format with hashtag, all other values in pixels
type SliderStyles = {
  trackColour?: string;
  fillColour?: string;
  handleColour?: string;
  tooltipBackgroundColour?: string;
  tooltipTextColour?: string;
  trackBorderRadius?: number;
  handleBorderRadius?: number;
  tooltipBorderRadius?: number;
  handleWidth?: number;
  handleLength?: number;
  tooltipFontSize?: number;
  tooltipOffset?: number;
  invertTooltip?: boolean; // By default tooltip is below track for horizontal and right of track for vertical
};

export default function Slider({
  position,
  width,
  length,
  orientation,
  disabled,
  onDragStart,
  onChangeCommitted,
  formatPosition,
  styles,
  icon,
}: SliderProps) {
  const trackRef = useRef<HTMLDivElement>(null);

  const [dragPosition, setDragPosition] = useState<number | null>(null);
  const [dragging, setDragging] = useState(false);

  // Use drag position or use position prop clamped to range [0,1]
  const currentPosition = dragPosition ?? Math.max(0, Math.min(1, position));
  const isVertical = orientation === "vertical";

  const axisStart = isVertical ? "bottom" : "left";
  const lengthAxis = isVertical ? "height" : "width";
  const widthAxis = isVertical ? "width" : "height";

  const containerStyle: React.CSSProperties = icon
    ? {
        flexDirection: isVertical ? "column" : "row",
        gap: icon.gap,
      }
    : {};

  const trackStyle: React.CSSProperties = {
    [lengthAxis]: length,
    [widthAxis]: width,
    backgroundColor: styles?.trackColour ?? "#000",
    borderRadius: styles?.trackBorderRadius ?? 0,
  };

  const radius = styles?.trackBorderRadius ?? 0;

  const fillBorderRadius: React.CSSProperties = isVertical
    ? {
        borderBottomLeftRadius: radius,
        borderBottomRightRadius: radius,
        borderTopLeftRadius: 0,
        borderTopRightRadius: 0,
      }
    : {
        borderTopLeftRadius: radius,
        borderBottomLeftRadius: radius,
        borderTopRightRadius: 0,
        borderBottomRightRadius: 0,
      };

  const fillStyle: React.CSSProperties = {
    [lengthAxis]: `${currentPosition * length}px`,
    [widthAxis]: width,
    [axisStart]: `${currentPosition * 50}%`,
    backgroundColor: styles?.fillColour ?? "#fff",
    transform: isVertical ? `translate(0%, 50%)` : `translate(-50%, 0%)`,
    ...fillBorderRadius,
  };

  const handleStyle: React.CSSProperties = {
    [lengthAxis]: styles?.handleLength ?? 0,
    [widthAxis]: styles?.handleWidth ?? 0,
    [axisStart]: `${currentPosition * 100}%`,
    cursor: disabled ? "default" : "pointer",
    borderRadius: styles?.handleBorderRadius ?? 0,
    backgroundColor: styles?.handleColour ?? "#fff",
    transform: isVertical
      ? `translate(${(width - (styles?.handleWidth ?? width)) / 2}px, 50%)`
      : `translate(-50%, ${(width - (styles?.handleWidth ?? width)) / 2}px)`,
  };

  let translateX: string;
  let translateY: string;

  const tooltipOffset = styles?.tooltipOffset ?? 0;

  if (isVertical) {
    translateY = "50%";

    translateX = styles?.invertTooltip
      ? `calc(-100% - ${tooltipOffset}px)`
      : `${width + tooltipOffset}px`;
  } else {
    translateX = "-50%";

    translateY = styles?.invertTooltip
      ? `calc(-100% - ${tooltipOffset}px)`
      : `${width + tooltipOffset}px`;
  }

  const tooltipStyle: React.CSSProperties = {
    [axisStart]: `${currentPosition * 100}%`,
    backgroundColor: styles?.tooltipBackgroundColour ?? "#000",
    color: styles?.tooltipTextColour ?? "#fff",
    borderRadius: styles?.tooltipBorderRadius ?? 0,
    fontSize: styles?.tooltipFontSize ?? 12,
    transform: `translate(${translateX}, ${translateY})`,
  };

  const iconStyle: React.CSSProperties = {
    display: "flex",
    alignItems: "center",
  };

  const iconBefore =
    icon &&
    ((icon.position === "max" && isVertical) ||
      (icon.position === "min" && !isVertical))
      ? icon
      : null;

  const iconAfter =
    icon &&
    ((icon.position === "max" && !isVertical) ||
      (icon.position === "min" && isVertical))
      ? icon
      : null;

  function getPositionFromPointer(e: React.PointerEvent) {
    const rect = trackRef.current!.getBoundingClientRect();

    const raw = isVertical
      ? (e.clientY - rect.top) / rect.height
      : (e.clientX - rect.left) / rect.width;

    const p = isVertical ? 1 - raw : raw;

    // Clamp position to range [0,1]
    return Math.max(0, Math.min(1, p));
  }

  // Pointer down handler
  function handlePointerDown(e: React.PointerEvent) {
    if (disabled) return;

    e.currentTarget.setPointerCapture(e.pointerId);
    setDragging(true);
    onDragStart?.();

    const p = getPositionFromPointer(e);
    setDragPosition(p);
  }

  // Pointer move handler
  function handlePointerMove(e: React.PointerEvent) {
    if (!dragging || disabled) return;

    const p = getPositionFromPointer(e);
    setDragPosition(p);
  }

  // Pointer release handler
  function handlePointerUp() {
    if (!dragging || disabled) return;
    if (dragPosition !== null) onChangeCommitted?.(dragPosition);

    setDragPosition(null);
    setDragging(false);
  }

  return (
    <div className={sliderStyles["container"]} style={containerStyle}>
      {iconBefore && (
        <div className={sliderStyles["icon"]} style={iconStyle}>
          {iconBefore.element}
        </div>
      )}
      <div
        ref={trackRef}
        onPointerDown={handlePointerDown}
        onPointerMove={handlePointerMove}
        onPointerUp={handlePointerUp}
        className={sliderStyles["track"]}
        style={trackStyle}
      >
        <div className={sliderStyles["fill"]} style={fillStyle} />
        <div className={sliderStyles["handle"]} style={handleStyle} />
        {dragging && (
          <div className={sliderStyles["tooltip"]} style={tooltipStyle}>
            {formatPosition
              ? formatPosition(currentPosition)
              : currentPosition.toString()}
          </div>
        )}
      </div>
      {iconAfter && (
        <div className={sliderStyles["icon"]} style={iconStyle}>
          {iconAfter.element}
        </div>
      )}
    </div>
  );
}
