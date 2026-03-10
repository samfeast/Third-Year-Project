import { StoreProvider } from "../store/StoreProvider";
import { Routes, Route, Navigate } from "react-router-dom";

import AppLayout from "./AppLayout";
import GuidePage from "../pages/Guide/GuidePage";
import CreatePage from "../pages/Create/CreatePage";
import ConfigurePage from "../pages/Configure/ConfigurePage";
import SimulatePage from "../pages/Simulate/SimulatePage";
import AnalysePage from "../pages/Analyse/AnalysePage";

function App() {
  return (
    <StoreProvider>
      <Routes>
        <Route path="/" element={<AppLayout />}>
          <Route index element={<Navigate to="/guide" />} />
          <Route path="guide" element={<GuidePage />} />
          <Route path="create" element={<CreatePage />} />
          <Route path="configure" element={<ConfigurePage />} />
          <Route path="simulate" element={<SimulatePage />} />
          <Route path="analyse" element={<AnalysePage />} />
        </Route>
      </Routes>
    </StoreProvider>
  );
}

export default App;
