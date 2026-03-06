import { useState } from "react";
import ToggleSwitch from "../components/ToggleSwitch";
import PresetCard from "../components/PresetCard";
import "./styles/CreatePage.css";
import { useStore } from "../store/StoreProvider";
import { emptyLayout, type Layout } from "../store/types";
import { preset1 } from "../presets/preset1";
import { preset2 } from "../presets/preset2";
import { preset3 } from "../presets/preset3";
import { preset4 } from "../presets/preset4";

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
        <div className="preset-grid">
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
