import matplotlib.pyplot as plt
import sys
import csv
import json

# Generate triangulation.png for input vertices and triangles
# Vertices format: JSON geometry v1
# Triangles format: JSON mesh v1
def main():
    vertices_path = sys.argv[1]
    triangles_path = sys.argv[2]

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

    plot_polygon(polygons_x[0], polygons_y[0], "grey")

    for i in range(1, len(polygons_x)):
        plot_polygon(polygons_x[i], polygons_y[i], "white")

    lines = []
    with open(triangles_path, "r") as read_file:
        data = json.load(read_file)

        for triangle in data["triangles"]:
            for i in range(3):
                a = triangle[i]
                b = triangle[(i+1) % 3]
                lines.append([(a[0], a[1]), (b[0], b[1])])

    lines = deduplicate_lines(lines)
    plot_lines(lines)

    plt.axis("off")
    plt.gca().set_aspect("equal", adjustable="box")
    plt.savefig("../output/triangulation.png", dpi=300)


def deduplicate_lines(lines):
    unique = set()

    for p1, p2 in lines:
        line = tuple(sorted((p1, p2)))
        unique.add(line)

    return [list(line) for line in unique]


def plot_polygon(x, y, color):
    plt.fill(x, y, color=color)
    plt.plot(x + [x[0]], y + [y[0]], color="black", linewidth=2, zorder=2)


def plot_lines(lines):
    for (x1, y1), (x2, y2) in lines:
        plt.plot([x1, x2], [y1, y2], color="red", linewidth=0.6, zorder=1)


if __name__ == "__main__":
    main()
