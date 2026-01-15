import matplotlib.pyplot as plt
import sys
import csv


def main():
    vertices_path = sys.argv[1]
    triangles_path = sys.argv[2]

    lines = []
    # Get lines from triangulation
    with open(triangles_path) as f:
        csv_reader = csv.reader(f, delimiter=",")
        for row in csv_reader:
            lines.append(((row[0], row[1]), (row[2], row[3])))
            lines.append(((row[2], row[3]), (row[4], row[5])))
            lines.append(((row[4], row[5]), (row[0], row[1])))

    plot_lines(lines)


def plot_lines(lines):
    for (x1, y1), (x2, y2) in lines:
        plt.plot([int(x1), int(x2)], [int(y1), int(y2)], color="red", linewidth=0.6)

    plt.axis("off")
    plt.savefig("../output/triangulation.png")


if __name__ == "__main__":
    main()
