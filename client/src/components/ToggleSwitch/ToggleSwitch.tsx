import styles from "./ToggleSwitch.module.css";

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
    <div className={styles["toggle-wrapper"]}>
      {leftLabel && <span className={styles["toggle-label"]}>{leftLabel}</span>}
      <label className={styles["switch"]}>
        <input
          type="checkbox"
          checked={checked}
          onChange={(e) => onChange(e.target.checked)}
        />
        <span className={`${styles.slider} ${styles.round}`}></span>
      </label>
      {rightLabel && (
        <span className={styles["toggle-label"]}>{rightLabel}</span>
      )}
    </div>
  );
}
