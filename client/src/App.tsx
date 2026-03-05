import { Routes, Route, Navigate } from "react-router-dom";
import { StoreProvider } from "./store/StoreProvider";
import GuidePage from "./pages/GuidePage";
import TestPage from "./pages/TestPage";
import CreatePage from "./pages/CreatePage";
import ConfigurePage from "./pages/ConfigurePage";
import SimulatePage from "./pages/SimulatePage";
import AnalysePage from "./pages/AnalysePage";
import AppLayout from "./AppLayout";

function App() {
  return (
    <StoreProvider>
      <Routes>
        <Route path="/" element={<AppLayout />}>
          <Route index element={<Navigate to="/guide" />} />
          <Route path="guide" element={<GuidePage />} />
          <Route path="test" element={<TestPage />} />
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
