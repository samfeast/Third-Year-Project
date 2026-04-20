import "./PresetCard.css";

type PresetCardProps = {
  name: string;
  // This now accepts both a URL string or a local image import
  imageSrc: string;
  description: string;
  onSelect?: (name: string) => void;
  upload?: boolean;
};

export default function PresetCard({
  name,
  imageSrc,
  description,
  onSelect,
  upload = false,
}: PresetCardProps) {
  return (
    <article className={"preset-card"}>
      <div className="image-container">
        <img
          src={imageSrc}
          alt={`Preview of ${name} layout`}
          className="card-image"
        />
      </div>

      <div className="card-content">
        <h2 className="card-title">{name}</h2>
        <p className="card-description">{description}</p>

        <button
          className={`select-button ${upload ? "upload-theme" : "preset-theme"}`}
          onClick={() => onSelect?.(name)}
        >
          {upload ? "Upload" : "Select Preset"}
        </button>
      </div>
    </article>
  );
}
