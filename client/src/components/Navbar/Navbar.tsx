import { Link } from "react-router-dom";

import "./Navbar.css";

export default function Navbar() {
  return (
    <nav className="navbar">
      <Link to="/guide">Guide</Link>
      <Link to="/create">Create</Link>
      <Link to="/configure">Configure</Link>
      <Link to="/simulate">Simulate</Link>
      <Link to="/analyse">Analyse</Link>
    </nav>
  );
}
