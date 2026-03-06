import "./styles/ToggleSwitch.css";

type ToggleSwitchProps = {
  leftLabel?: string;
  rightLabel?: string;
  checked: boolean;
  onChange: (checked: boolean) => void;
};

export default function ToggleSwitch({
  leftLabel,
  rightLabel,
  checked,
  onChange,
}: ToggleSwitchProps) {
  return (
    <div className="toggle-wrapper">
      {leftLabel && <span className="toggle-label">{leftLabel}</span>}
      <label className="switch">
        <input
          type="checkbox"
          checked={checked}
          onChange={(e) => onChange(e.target.checked)}
        />
        <span className="slider round"></span>
      </label>
      {rightLabel && <span className="toggle-label">{rightLabel}</span>}
    </div>
  );
}
