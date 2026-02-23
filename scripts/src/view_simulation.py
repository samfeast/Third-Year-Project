import matplotlib.pyplot as plt
import matplotlib.animation as animation
import sys
import json
import numpy as np


def main():
    vertices_path = sys.argv[1]
    snapshots_path = sys.argv[2]

    # Preprocess floorplan
    polygons_x = []
    polygons_y = []
    parsed_vertices_x = []
    parsed_vertices_y = []

    with open(vertices_path, "r") as read_file:
        data = json.load(read_file)

        for point in data["positive"]:
            parsed_vertices_x.append(point[0])
            parsed_vertices_y.append(point[1])

        polygons_x.append(parsed_vertices_x)
        polygons_y.append(parsed_vertices_y)

        for i in range(len(data["negatives"])):
            parsed_vertices_x = []
            parsed_vertices_y = []
            negative = data["negatives"][i]
            for point in negative:
                parsed_vertices_x.append(point[0])
                parsed_vertices_y.append(point[1])

            polygons_x.append(parsed_vertices_x)
            polygons_y.append(parsed_vertices_y)

    fig, ax = plt.subplots()

    plot_polygon(ax, polygons_x[0], polygons_y[0], "grey")

    for i in range(1, len(polygons_x)):
        plot_polygon(ax, polygons_x[i], polygons_y[i], "white")

    with open(snapshots_path, "r") as read_file:
        data = json.load(read_file)

        num_frames = len(data["snapshots"])

        snapshots = []
        for s in data["snapshots"]:
            snapshot_positions = []
            snapshot_speeds = []
            for agent in s["agents"]:
                pos = agent["position"]
                speed = agent["speed"]
                snapshot_positions.append(pos)
                snapshot_speeds.append(speed)

            snapshots.append(
                {"positions": np.array(snapshot_positions), "speeds": np.array(snapshot_speeds)}
            )

    print("Finished loading snapshot data")

    scat = ax.scatter([], [], s=10, c=[], cmap="viridis", vmin=90, vmax=150, zorder=5)

    ax.axis("off")
    fig.gca().set_aspect("equal", adjustable="box")

    cbar = fig.colorbar(scat)
    cbar.set_label("Speed")

    def update(frame):
        data = snapshots[frame]
        positions = data["positions"]
        speeds = data["speeds"]

        scat.set_offsets(positions)
        scat.set_array(speeds)  # This tells scatter to color by speed
        return (scat,)

    print("Rendering animation")

    ani = animation.FuncAnimation(
        fig,
        update,
        frames=len(snapshots),
        blit=True,
    )

    print("Saving video")
    ani.save("../output/simulation.mp4", writer="ffmpeg", fps=20, dpi=300)

    print("Visualisation saved to simulation.mp4")


def plot_polygon(ax, x, y, color):
    ax.fill(x, y, color=color)
    ax.plot(x + [x[0]], y + [y[0]], color="black", linewidth=2, zorder=2)


if __name__ == "__main__":
    main()
