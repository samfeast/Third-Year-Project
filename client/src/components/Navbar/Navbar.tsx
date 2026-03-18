import { Link } from "react-router-dom";

import styles from "./Navbar.module.css";

export default function Navbar() {
  return (
    <nav className={styles["navbar"]}>
      <Link to="/guide">Guide</Link>
      <Link to="/create">Create</Link>
      <Link to="/configure">Configure</Link>
      <Link to="/simulate">Simulate</Link>
      <Link to="/analyse">Analyse</Link>
    </nav>
  );
}
