import random
import sys
import csv
import math


def segments_intersect(a, b, c, d):
    # Check if line segments ab and cd intersect

    def orient(p, q, r):
        return (q[0] - p[0]) * (r[1] - p[1]) - (q[1] - p[1]) * (r[0] - p[0])

    def on_segment(p, q, r):
        return min(p[0], r[0]) <= q[0] <= max(p[0], r[0]) and min(p[1], r[1]) <= q[1] <= max(
            p[1], r[1]
        )

    o1 = orient(a, b, c)
    o2 = orient(a, b, d)
    o3 = orient(c, d, a)
    o4 = orient(c, d, b)

    if o1 * o2 < 0 and o3 * o4 < 0:
        return True

    if o1 == 0 and on_segment(a, c, b):
        return True
    if o2 == 0 and on_segment(a, d, b):
        return True
    if o3 == 0 and on_segment(c, a, d):
        return True
    if o4 == 0 and on_segment(c, b, d):
        return True

    return False


def is_simple_polygon(points):
    # Only needed if integer rounding makes polygon complex
    n = len(points)
    for i in range(n):
        a1 = points[i]
        a2 = points[(i + 1) % n]
        for j in range(i + 1, n):
            if abs(i - j) <= 1 or (i == 0 and j == n - 1):
                continue
            b1 = points[j]
            b2 = points[(j + 1) % n]
            if segments_intersect(a1, a2, b1, b2):
                return False
    return True


def main():
    # Generate a random simple polygon with integer coordinates.

    num_vertices = int(sys.argv[1])
    random.seed(42 + num_vertices)
    out_file = sys.argv[2]

    for _ in range(1000):
        angles = sorted(random.uniform(0, 2 * math.pi) for _ in range(num_vertices))
        points = set()

        for angle in angles:
            r = random.randint(10, 80)
            x = int(round(r * math.cos(angle)))
            y = int(round(r * math.sin(angle)))
            if (x, y) != (0, 0):
                points.add((x, y))

        if len(points) != num_vertices:
            continue

        points = list(points)
        points.sort(key=lambda p: math.atan2(p[1], p[0]))

        if is_simple_polygon(points):
            with open(out_file, "w", newline="") as f:
                writer = csv.writer(f)
                for x, y in points:
                    writer.writerow([x, y])
            return

    raise RuntimeError("Failed to generate a simple polygon in 1000 attempts")


if __name__ == "__main__":
    main()
