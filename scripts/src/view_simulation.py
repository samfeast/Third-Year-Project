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
            for agent in s["agents"]:
                pos = agent["position"]
                snapshot_positions.append(pos)

            snapshots.append(np.array(snapshot_positions))

    print("Finished loading snapshot data")

    scat = ax.scatter([], [], s=10, c="blue")

    ax.axis("off")
    fig.gca().set_aspect("equal", adjustable="box")

    def update(frame):
        scat.set_offsets(snapshots[frame])
        return (scat,)

    print("Rendering animation")

    ani = animation.FuncAnimation(
        fig,
        update,
        frames=len(snapshots),
        blit=True,
    )

    print("Saving video")
    ani.save("../output/simulation.mp4", writer="ffmpeg", fps=10, dpi=300)

    print("Visualisation saved to simulation.mp4")


def plot_polygon(ax, x, y, color):
    ax.fill(x, y, color=color)
    ax.plot(x + [x[0]], y + [y[0]], color="black", linewidth=2, zorder=2)


if __name__ == "__main__":
    main()
