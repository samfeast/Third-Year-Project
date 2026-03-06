import { useState } from "react";
import ToggleSwitch from "../components/ToggleSwitch";
import PresetCard from "../components/PresetCard";
import "./styles/CreatePage.css";

export default function CreatePage() {
  const [showPresets, setShowPresets] = useState(false);

  const [selectedPreset, setSelectedPreset] = useState<string | null>(null);

  const exampleImageUrl =
    "https://media.istockphoto.com/id/1316134499/photo/a-concept-image-of-a-magnifying-glass-on-blue-background-with-a-word-example-zoom-inside-the.jpg?s=612x612&w=0&k=20&c=sZM5HlZvHFYnzjrhaStRpex43URlxg6wwJXff3BE9VA=";

  function handlePresetSelect(name: string) {
    setSelectedPreset(name);
  }

  const presets = [
    { name: "Preset 1", description: "Large Area." },
    { name: "Preset 2", description: "Medium Area." },
    { name: "Preset 3", description: "Small Area." },
    { name: "Preset 4", description: "Custom Area." },
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
