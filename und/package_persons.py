"""
Authors each package before and after identity merging, over the same files Understand analysed.
"""
import collections
import csv
import os
import subprocess
import sys


def package_files(repo, files_txt):
    by_package = collections.defaultdict(list)
    for line in open(files_txt):
        rel = line.strip()
        if not rel:
            continue
        path = rel if os.path.exists(os.path.join(repo, rel)) else os.path.join("packages", rel)
        parts = path.split("/")
        if len(parts) > 2 and parts[0] == "packages":
            by_package[parts[1]].append(path)
    return by_package


def log(repo, files):
    rows = []
    for i in range(0, len(files), 400):
        out = subprocess.run(["git", "-C", repo, "log", "--format=%H%x1f%aN%x1f%aE", "--", *files[i:i + 400]],
                             check=True, capture_output=True, text=True).stdout
        rows += [tuple(l.split("\x1f")) for l in out.splitlines() if l]
    return list({r[0]: r for r in rows}.values())  # one row per commit


def main(repo, files_txt, identities_csv, und_csv, out_csv):
    person = {}
    bot = {}
    for r in csv.DictReader(open(identities_csv)):
        key = (r["name"], r["email"].lower())
        person[key] = r["person_id"]
        bot[r["person_id"]] = r["is_bot"] == "True"
    und = {r["package"]: r for r in csv.DictReader(open(und_csv))}

    fields = ["package", "files", "loc", "und_authors", "und_ownership", "und_major", "und_minor", "und_days_since_modified",
              "commits", "raw_names", "raw_emails", "persons", "human_persons", "bot_persons", "bot_commit_share",
              "top_human", "top_human_share", "unmapped_identities"]
    with open(out_csv, "w", newline="") as f:
        writer = csv.DictWriter(f, fieldnames=fields)
        writer.writeheader()
        for package, files in sorted(package_files(repo, files_txt).items()):
            commits = log(repo, files)
            if not commits:
                continue
            keys = [(name, email.lower()) for _, name, email in commits]
            ids = [person.get(k, "name:" + k[0].lower()) for k in keys]
            per_person = collections.Counter(ids)
            humans = {p: n for p, n in per_person.items() if not bot.get(p, False)}
            top, top_n = max(humans.items(), key=lambda kv: (kv[1], kv[0])) if humans else ("", 0)
            bot_commits = sum(n for p, n in per_person.items() if bot.get(p, False))
            loc = sum(sum(1 for _ in open(os.path.join(repo, f), errors="ignore")) for f in files)
            u = und.get(package, {})
            writer.writerow({
                "package": package,
                "files": len(files),
                "loc": loc,
                "und_authors": u.get("GitAuthors", ""),
                "und_ownership": u.get("GitOwnership", ""),
                "und_major": u.get("GitMajorContributors", ""),
                "und_minor": u.get("GitMinorContributors", ""),
                "und_days_since_modified": u.get("GitDaysSinceLastModified", ""),
                "commits": len(commits),
                "raw_names": len({k[0] for k in keys}),
                "raw_emails": len({k[1] for k in keys}),
                "persons": len(per_person),
                "human_persons": sum(1 for p in per_person if not bot.get(p, False)),
                "bot_persons": sum(1 for p in per_person if bot.get(p, False)),
                "bot_commit_share": f"{100 * bot_commits / len(commits):.2f}",
                "top_human": top,
                "top_human_share": f"{100 * top_n / len(commits):.2f}",
                "unmapped_identities": len({k for k in keys if k not in person}),
            })


if __name__ == "__main__":
    main(*sys.argv[1:6])
