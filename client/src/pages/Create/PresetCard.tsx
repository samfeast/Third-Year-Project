import "./PresetCard.css";

type PresetCardProps = {
  name: string;
  imageUrl: string;
  description: string;
  onClick?: (name: string) => void;
  selected?: boolean;
};

export default function PresetCard({
  name,
  imageUrl,
  description,
  onClick,
  selected = false,
}: PresetCardProps) {
  return (
    <div
      className={`card ${selected ? "selected" : ""}`}
      onClick={() => onClick && onClick(name)}
      style={{ cursor: "pointer" }}
    >
      <h2 className="name">{name}</h2>
      <img src={imageUrl} alt={name} className="image" />
      <p className="description">{description}</p>
    </div>
  );
}
