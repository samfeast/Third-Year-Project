export default function GuidePage() {
  return (
    <div>
      <h1>Guide</h1>
      <p>
        If you wish to upload a custom floorplan, the JSON file should have the
        following structure:
        <pre>{`{
          "type": "geometry",
          "version": 1,
          "positive": [
            [0, 0],
            [6850, 0],
            [6850, 500],
            [5850, 500],
            [5850, 7750],
            [0, 7750]
          ],
          "negatives": [
            [
              [1270, 6350],
              [1270, 6750],
              [2040, 6750],
              [2040, 6350]
            ]
          ],
          "exits": [[6350, 250]],
          "name": "Preset Name"
        }`}</pre>
      </p>
    </div>
  );
}
