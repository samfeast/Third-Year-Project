import { useSyncExternalStore } from "react";
import { snapshotStore } from "./snapshotStore";

export function useSnapshot() {
  return useSyncExternalStore(snapshotStore.subscribe, () =>
    snapshotStore.getLatestSnapshot(),
  );
}
