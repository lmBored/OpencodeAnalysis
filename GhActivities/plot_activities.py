"""Aggregate GhActivities output into weekly series and plot them."""
from __future__ import annotations

import csv
import datetime as dt
import json
from pathlib import Path

import matplotlib

matplotlib.use("Agg")
import matplotlib.dates as mdates
import matplotlib.pyplot as plt

ROOT = Path(__file__).resolve().parents[1]
DATA = ROOT / "data" / "activities"
FIGDIR = ROOT / "out"
FIGDIR.mkdir(exist_ok=True)

WINDOW_START = dt.date(2025, 9, 27)
WINDOW_END = dt.date(2026, 9, 27)


def parse_day(value: str) -> dt.date | None:
    value = (value or "").strip()
    return dt.datetime.fromisoformat(value.replace("Z", "+00:00")).date() if value else None


def week_start(day: dt.date) -> dt.date:
    return day - dt.timedelta(days=day.weekday())


def bucket(path: Path, column: str) -> dict[dt.date, int]:
    counts: dict[dt.date, int] = {}
    with path.open(newline="", encoding="utf-8") as fh:
        for row in csv.DictReader(fh):
            day = parse_day(row.get(column, ""))
            if day and WINDOW_START <= day <= WINDOW_END:
                ws = week_start(day)
                counts[ws] = counts.get(ws, 0) + 1
    return counts


commits = bucket(DATA / "commits.csv", "committer_date")
releases = bucket(DATA / "releases.csv", "published_at")
reviews = bucket(DATA / "reviews.csv", "submitted_at") if (DATA / "reviews.csv").exists() else {}

issues: dict[dt.date, int] = {}
prs: dict[dt.date, int] = {}
with (DATA / "search_weekly.csv").open(newline="", encoding="utf-8") as fh:
    for row in csv.DictReader(fh):
        ws = dt.date.fromisoformat(row["week_start"])
        issues[ws] = int(row["issues"])
        prs[ws] = int(row["prs"])

all_weeks = sorted(set(issues) | set(prs) | set(commits) | set(releases) | set(reviews))
series = {
    "commits": [commits.get(w, 0) for w in all_weeks],
    "prs": [prs.get(w, 0) for w in all_weeks],
    "issues": [issues.get(w, 0) for w in all_weeks],
    "releases": [releases.get(w, 0) for w in all_weeks],
    "reviews": [reviews.get(w, 0) for w in all_weeks],
}

print(f"weeks: {len(all_weeks)}  {all_weeks[0]} .. {all_weeks[-1]}")
for name, values in series.items():
    total = sum(values)
    peak_i = max(range(len(values)), key=values.__getitem__)
    print(
        f"{name:9s} total={total:6d}  mean/wk={total / len(values):7.1f}  peak={values[peak_i]:4d} (wk {all_weeks[peak_i]})"
    )

stats_path = DATA / "commit_activity.json"
if stats_path.exists():
    stats = json.loads(stats_path.read_text(encoding="utf-8"))
    in_window = [
        w["total"]
        for w in stats
        if WINDOW_START <= dt.datetime.fromtimestamp(w["week"], dt.timezone.utc).date() <= WINDOW_END
    ]
    print(
        f"\ncross-check (GitHub stats/commit_activity): {sum(in_window)} commits vs commits.csv {sum(series['commits'])}"
    )

x = [dt.datetime.combine(w, dt.time()) for w in all_weeks]
fig, (ax1, ax2) = plt.subplots(
    2, 1, figsize=(12, 7.5), sharex=True, gridspec_kw={"height_ratios": [3, 1]}
)

ax1.plot(x, series["prs"], color="#1a9850", lw=1.8, marker="o", ms=2.5, label="Pull requests opened")
ax1.plot(x, series["commits"], color="#2166ac", lw=1.8, marker="o", ms=2.5, label="Commits (default branch)")
ax1.plot(x, series["issues"], color="#d95f02", lw=1.8, marker="o", ms=2.5, label="Issues opened")
ax1.plot(x, series["reviews"], color="#6a3d9a", lw=1.5, marker="o", ms=2, label="Reviews submitted")
ax1.set_ylabel("events per week")
ax1.set_ylim(0, 1280)
ax1.grid(alpha=0.25)
ax1.legend(loc="upper left", frameon=False, ncol=3, fontsize=9)
ax1.set_title("anomalyco/opencode - GitHub activity per week, 2025-09-27 to 2026-09-27")

ax1.annotate(
    "Jan-2026 surge: issues 978 -> 3,010/month (peak 904/wk),\n"
    "commits peak 656/wk; v1.1.x release line starts 2026-01-04;\n"
    "top threads: #7410 'Broken Claude Max' (386c), #8030 Copilot quota",
    xy=(dt.datetime(2026, 1, 12), 904),
    xytext=(dt.datetime(2025, 9, 24), 1030),
    arrowprops=dict(arrowstyle="->", color="#d95f02", lw=1.2),
    fontsize=8,
    color="#8c2d04",
)
ax1.annotate(
    "PR peak: 1,064/wk (2026-08-24); August = 4,025 PRs,\n"
    "while commits on dev fall below 100/wk",
    xy=(dt.datetime(2026, 8, 24), 1064),
    xytext=(dt.datetime(2026, 3, 20), 1150),
    arrowprops=dict(arrowstyle="->", color="#1a9850", lw=1.2),
    fontsize=8,
    color="#0b6b32",
)

ax2.bar(x, series["releases"], width=5, color="#7b7b7b")
ax2.set_ylabel("releases/wk", labelpad=8)
ax2.set_ylim(0, 47)
ax2.grid(alpha=0.25)
ax2.annotate(
    "37 releases in one week (2025-11-17);\nDec-2025: 82 releases, 10 on 2025-12-30",
    xy=(dt.datetime(2025, 11, 17), 37),
    xytext=(dt.datetime(2025, 10, 1), 45),
    va="top",
    arrowprops=dict(arrowstyle="->", color="#4d4d4d", lw=1.0),
    fontsize=7.5,
    color="#333333",
)

ax2.xaxis.set_major_locator(mdates.MonthLocator())
ax2.xaxis.set_major_formatter(mdates.DateFormatter("%Y-%m"))
fig.autofmt_xdate(rotation=45, ha="right")

fig.tight_layout()
fig.savefig(FIGDIR / "activities.pdf")
fig.savefig(FIGDIR / "activities.png", dpi=150)
print(f"\nwrote {FIGDIR / 'activities.pdf'} and .png")
