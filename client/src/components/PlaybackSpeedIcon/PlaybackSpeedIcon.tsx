type PlaybackSpeedIconProps = {
  size: number;
  strokeWidth: number;
};

export default function PlaybackSpeedIcon({
  size,
  strokeWidth,
}: PlaybackSpeedIconProps) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth={strokeWidth}
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <path d="M14.6 1.8 a 10.5 10.5 0 1 0 7 6" />
      <line x1="12" y1="12" x2="18" y2="5" />
      <circle cx="12" cy="12" r="1.3" fill="currentColor" stroke="none" />
    </svg>
  );
}
