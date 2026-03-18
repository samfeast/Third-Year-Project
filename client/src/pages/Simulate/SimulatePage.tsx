import { useState } from "react";
import { useStore } from "../../store/StoreProvider";

import { snapshotStore } from "../../features/simulation/snapshotStore";
import { startSimulation } from "../../features/simulation/startSimulation";

import SimulationCanvas from "./SimulationCanvas";
import Slider from "../../components/Slider/Slider";
import PlaybackSpeedIcon from "../../components/PlaybackSpeedIcon/PlaybackSpeedIcon";

import "./SimulatePage.css";

export default function SimulatePage() {
  const { state } = useStore();

  const [playbackSpeedPosition, setPlaybackSpeedPosition] = useState(0.5);

  function handlePlaybackSpeedChange(newPosition: number) {
    if (playbackSpeedPosition === newPosition) return;
    setPlaybackSpeedPosition(newPosition);
    const playbackSpeed = parseFloat(
      positionToPlaybackSpeed(newPosition).toPrecision(2),
    );
    console.log("Playback speed set to", playbackSpeed);
    snapshotStore.setPlaybackSpeed(playbackSpeed);
  }

  function positionToPlaybackSpeed(position: number) {
    return 0.1 * Math.pow(10 / 0.1, position);
  }

  function formatPlaybackSpeed(position: number) {
    return positionToPlaybackSpeed(position).toPrecision(2) + "x";
  }

  return (
    <div className="simulation-page">
      <div className="button-row">
        <button
          className="button"
          onClick={() => startSimulation(state.clientId, state.config)}
        >
          Start Simulation
        </button>
        <button
          className="button"
          onClick={() => snapshotStore.setPlaybackState("playing")}
        >
          Play
        </button>
        <button
          className="button"
          onClick={() => snapshotStore.setPlaybackState("paused")}
        >
          Pause
        </button>
      </div>

      {/* Canvas + slider */}
      <div className="canvas-area">
        <SimulationCanvas />
        <div className="slider-wrapper">
          <Slider
            position={playbackSpeedPosition}
            width={24}
            length={650}
            orientation={"vertical"}
            onChangeCommitted={handlePlaybackSpeedChange}
            formatPosition={formatPlaybackSpeed}
            icon={{
              element: <PlaybackSpeedIcon size={40} strokeWidth={2.2} />,
              position: "min",
              gap: 10,
            }}
            styles={{
              trackBorderRadius: 12,
              trackColour: "#464C5A",
              fillColour: "#dedede",
              handleWidth: 40,
              handleLength: 20,
              handleBorderRadius: 20,
              handleColour: "#fff",
              tooltipOffset: 16,
              tooltipFontSize: 20,
              tooltipBackgroundColour: "#222",
              tooltipTextColour: "#dedede",
              tooltipBorderRadius: 6,
            }}
          />
        </div>
      </div>
    </div>
  );
}
