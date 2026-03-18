import { useState } from "react";
import { useStore } from "../../store/StoreProvider";
import type { Layout } from "../../features/layout/types";

import ToggleSwitch from "../../components/ToggleSwitch/ToggleSwitch";
import PresetCard from "./PresetCard";

import { preset1 } from "../../features/layout/presets/preset1";
import { preset2 } from "../../features/layout/presets/preset2";
import { preset3 } from "../../features/layout/presets/preset3";
import { preset4 } from "../../features/layout/presets/preset4";
import { emptyLayout } from "../../features/layout/defaults";

import styles from "./CreatePage.module.css";

export default function CreatePage() {
  const { state, dispatch } = useStore();

  const [showPresets, setShowPresets] = useState(false);
  const [selectedPreset, setSelectedPreset] = useState<string | null>(null);

  const exampleImageUrl =
    "https://media.istockphoto.com/id/1316134499/photo/a-concept-image-of-a-magnifying-glass-on-blue-background-with-a-word-example-zoom-inside-the.jpg?s=612x612&w=0&k=20&c=sZM5HlZvHFYnzjrhaStRpex43URlxg6wwJXff3BE9VA=";

  function handlePresetSelect(name: string) {
    setSelectedPreset(name);
    const agentDensity = state.config.agentDensity;
    let layout: Layout;
    switch (name) {
      case "Preset 1":
        layout = preset1;
        break;
      case "Preset 2":
        layout = preset2;
        break;

      case "Preset 3":
        layout = preset3;
        break;
      case "Preset 4":
        layout = preset4;
        break;
      default:
        layout = emptyLayout;
    }
    dispatch({
      type: "SET_CONFIG",
      payload: {
        agentDensity: agentDensity,
        layout: layout,
      },
    });
  }

  const presets = [
    { name: "Preset 1", description: "Walkable Area: 625 m2" },
    { name: "Preset 2", description: "Walkable Area: 865 m2" },
    { name: "Preset 3", description: "Walkable Area: ?" },
    { name: "Preset 4", description: "Walkable Area: ?" },
  ];

  return (
    <div>
      <ToggleSwitch
        leftLabel={""}
        rightLabel={"Choose from presets"}
        checked={showPresets}
        onChange={setShowPresets}
      />

      {showPresets ? (
        <div className={styles["preset-grid"]}>
          {presets.map((preset) => (
            <PresetCard
              key={preset.name}
              name={preset.name}
              imageUrl={exampleImageUrl}
              description={preset.description}
              onClick={handlePresetSelect}
              selected={selectedPreset === preset.name} // highlight if selected
            />
          ))}
        </div>
      ) : (
        <h1>Coming soon</h1>
      )}
    </div>
  );
}
