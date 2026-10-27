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

def find_root() -> Path:
    for candidate in (Path.cwd(), *Path(__file__).resolve().parents):
        if (candidate / "OpencodeAnalysis.sln").exists():
            return candidate
    raise SystemExit("cannot find OpencodeAnalysis.sln; run from the repository root")


ROOT = find_root()
DATA = ROOT / "data" / "activities"
FIGDIR = ROOT / "out" / "activities"
FIGDIR.mkdir(parents=True, exist_ok=True)

WINDOW_START = dt.date(2025, 9, 27)
WINDOW_END = dt.date(2026, 9, 27)


weekly_path = ROOT / "out" / "activities" / "activities_weekly.csv"
if not weekly_path.exists():
    raise SystemExit(f"missing {weekly_path}; run the activities command first")

with weekly_path.open(newline="", encoding="utf-8") as fh:
    weekly = list(csv.DictReader(fh))
all_weeks = [dt.date.fromisoformat(row["week_start"]) for row in weekly]
series = {
    name: [int(row[name]) for row in weekly]
    for name in ("commits", "prs", "issues", "releases", "reviews")
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
    "Jan-2026 surge: issues 978 -> 3,010/month (peak 905/wk),\n"
    "commits peak 656/wk; v1.1.x release line starts 2026-01-04;\n"
    "top threads: #7410 'Broken Claude Max' (386c), #8030 Copilot quota",
    xy=(dt.datetime(2026, 1, 12), 905),
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
