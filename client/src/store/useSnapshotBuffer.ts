import { useSyncExternalStore } from "react";
import { snapshotStore } from "./snapshotStore";

export function useSnapshotBuffer() {
  return useSyncExternalStore(snapshotStore.subscribe, snapshotStore.getBuffer);
}
