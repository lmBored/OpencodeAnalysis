import csv
import sys

import understand

METRICS = ["GitAuthors", "GitCommits", "GitOwnership", "GitMajorContributors",
           "GitMinorContributors", "GitDaysSinceLastModified"]


def number(value):
    # Understand formats decimals with the system locale ("17,50")
    if value is None or value == "":
        return ""
    return str(value).replace(",", ".")


def find(arch, name):
    if arch.name() == name:
        return arch
    for child in arch.children():
        hit = find(child, name)
        if hit is not None:
            return hit
    return None


def main(db_path, packages_csv, files_txt):
    db = understand.open(db_path)
    root = next(a for a in db.root_archs() if a.name() == "Directory Structure")
    packages = find(root, "packages") or root
    with open(packages_csv, "w", newline="") as f:
        writer = csv.writer(f)
        writer.writerow(["package", "files"] + METRICS)
        for arch in sorted(packages.children(), key=lambda a: a.name()):
            files = [e for e in arch.ents(True) if e.kindname().endswith("File")]
            if not files:
                continue
            values = arch.metric(METRICS)
            writer.writerow([arch.name(), len(files)] + [number(values.get(m)) for m in METRICS])
    with open(files_txt, "w") as f:
        for ent in sorted(db.ents("File ~Unknown ~Unresolved"), key=lambda e: e.relname()):
            f.write(ent.relname() + "\n")
    db.close()


if __name__ == "__main__":
    main(*sys.argv[1:4])
    # arguments: <project>.und out/und_packages.csv out/und_file_list.txt
