import json


def get_lines(vertices):
    lines = []

    for i in range(len(vertices)):
        j = (i + 1) % len(vertices)
        lines.append(
            [
                vertices[i][0],
                vertices[i][1],
                vertices[j][0],
                vertices[j][1],
            ]
        )

    return lines


with open("../data/vertices4.json", "r") as read_file:
    data = json.load(read_file)

flattened_lines = []

flattened_lines += get_lines(data["positive"])

for negative in data["negatives"]:
    flattened_lines += get_lines(negative)

print(flattened_lines)
