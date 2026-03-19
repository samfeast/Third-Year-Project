import { useEffect, useRef, useState } from "react";
import Slider from "../../components/Slider/Slider";
import { snapshotStore } from "../../features/simulation/snapshotStore";
import { useSnapshot } from "../../features/simulation/useSnapshot";
import { useStore } from "../../store/StoreProvider";
import { getSnapshots } from "../../websocket/simulationCommands";
import { startPlaybackLoop } from "../../features/simulation/playbackLoop";
import {
  startSnapshotPolling,
  stopSnapshotPolling,
} from "../../features/simulation/bufferPolling";

export default function TimelineScrubber() {
  const { state } = useStore();
  // Needed to trigger state updates to progress slider
  const snapshot = useSnapshot();

  const dragMaxStepIndexRef = useRef<number | null>(null);
  const [seekPosition, setSeekPosition] = useState<number | null>(null);

  function getMaxStepIndex() {
    return (
      dragMaxStepIndexRef.current ?? snapshotStore.getMaxStepIndexDisplayed()
    );
  }

  const storePosition = snapshotStore.getCurrentStep() / getMaxStepIndex();
  useEffect(() => {
    if (
      seekPosition !== null &&
      Math.abs(storePosition - seekPosition) < 0.01
    ) {
      setSeekPosition(null);
    }
  }, [snapshot]);

  const scrubberPosition = seekPosition ?? storePosition;

  // Fix scrubber range at the start of interaction
  function handleDragStart() {
    dragMaxStepIndexRef.current = snapshotStore.getMaxStepIndexDisplayed();
  }

  // Handle rewind/fast-forward
  function handleScrubberChange(newPosition: number) {
    setSeekPosition(newPosition);
    console.log(`Set to ${newPosition}`);
    const newStep = positionToStepIndex(newPosition);

    dragMaxStepIndexRef.current = null;

    // Order important here - isPlaybackFinished() is dependent on the state of the buffer and hasReceivedFinalStep
    const playbackFinished = snapshotStore.isPlaybackFinished();
    snapshotStore.clearSnapshotBuffer();
    snapshotStore.setReceivedFinalStep(false);
    // If playback had finished, restart the playback loop
    if (playbackFinished) {
      startPlaybackLoop();
    }

    // Restart buffer polling, this waits one interval before calling getSnapshots() so call manually once below
    stopSnapshotPolling();
    startSnapshotPolling(state.clientId);

    getSnapshots(state.clientId, {
      lastDisplayedStep: newStep - 1,
      lastBufferedStep: newStep - 1,
      playbackSpeed: snapshotStore.getPlaybackSpeed(),
    });
  }

  function positionToStepIndex(position: number) {
    return Math.round(position * getMaxStepIndex());
  }

  function formatScrubber(position: number) {
    return positionToStepIndex(position).toString();
  }

  return (
    <Slider
      position={scrubberPosition}
      width={18}
      length={1400}
      orientation={"horizontal"}
      onDragStart={handleDragStart}
      onChangeCommitted={handleScrubberChange}
      formatPosition={formatScrubber}
      styles={{
        trackBorderRadius: 12,
        trackColour: "#464C5A",
        fillColour: "#dedede",
        handleWidth: 28,
        handleLength: 28,
        handleBorderRadius: 20,
        handleColour: "#fff",
        tooltipOffset: 20,
        tooltipFontSize: 24,
        tooltipBackgroundColour: "#222",
        tooltipTextColour: "#dedede",
        tooltipBorderRadius: 6,
        invertTooltip: true,
      }}
    />
  );
}
