import matplotlib.pyplot as plt
import sys
import csv


def main():
    vertices_path = sys.argv[1]
    triangles_path = sys.argv[2]

    polygons_x = []
    polygons_y = []
    parsed_vertices_x = []
    parsed_vertices_y = []
    with open(vertices_path) as f:
        csv_reader = csv.reader(f, delimiter=",")
        for row in csv_reader:
            if len(row) == 0:
                polygons_x.append(parsed_vertices_x)
                polygons_y.append(parsed_vertices_y)
                parsed_vertices_x = []
                parsed_vertices_y = []
                continue
            parsed_vertices_x.append(int(row[0]))
            parsed_vertices_y.append(int(row[1]))

    polygons_x.append(parsed_vertices_x)
    polygons_y.append(parsed_vertices_y)

    plot_polygon(polygons_x[0], polygons_y[0], "grey")
    for i in range(1, len(polygons_x)):
        plot_polygon(polygons_x[i], polygons_y[i], "white")

    lines = []
    # Get lines from triangulation
    with open(triangles_path) as f:
        csv_reader = csv.reader(f, delimiter=",")
        for row in csv_reader:
            lines.append([(int(row[0]), int(row[1])), (int(row[2]), int(row[3]))])
            lines.append([(int(row[2]), int(row[3])), (int(row[4]), int(row[5]))])
            lines.append([(int(row[4]), int(row[5])), (int(row[0]), int(row[1]))])

    lines = deduplicate_lines(lines)
    plot_lines(lines)

    plt.axis("off")
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
        plt.plot([int(x1), int(x2)], [int(y1), int(y2)], color="red", linewidth=0.6, zorder=1)


if __name__ == "__main__":
    main()
