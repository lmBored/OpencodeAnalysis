"""
Figures for Chapter 3 (Project Health, items 1-3)
"""
import csv
import datetime as dt
from pathlib import Path

import matplotlib.dates as mdates
import matplotlib.pyplot as plt
import numpy as np

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / "out"
FIG = ROOT.parent / "final_report" / "Figures" / "Health"
FIG.mkdir(parents=True, exist_ok=True)

C_BOT, C_KEY, C_REG, C_OCC, C_CAS, C_GREY = "#9a9a9a", "#d62728", "#1f77b4", "#7fb2d9", "#c6dbef", "#bdbdbd"
GROUPS = [("bot", "Bots", C_BOT), ("key", "Key", C_KEY), ("regular", "Regular", C_REG),
          ("occasional", "Occasional", C_OCC), ("casual", "Casual", C_CAS)]
KEY_SHARE, DORMANT_Q, HORIZON = 0.8, 0.9, 90

# Policy changes, from the commits that added them to opencode (git log on b471c2b); explained in the caption
EVENTS = [
    (dt.date(2026, 1, 26), "1"),# c700b928e4 stale PRs closed after 60 days
    (dt.date(2026, 2, 18), "2"),# 639d1dd8fe compliance auto-close of issues and PRs
    (dt.date(2026, 3, 24), "3"),# 79e9d19019 issues closed after 90 days without activity
    (dt.date(2026, 5, 14), "4"),# d59d99665b PR cleanup (older than a month, < 2 positive reactions)
]

plt.rcParams.update({"font.family": "serif", "font.size": 9, "pdf.fonttype": 42,
                     "axes.spines.top": False, "axes.spines.right": False})


def read(name):
    with open(OUT / name, newline="") as f:
        return list(csv.DictReader(f))


def save(fig, name):
    fig.savefig(FIG / name, bbox_inches="tight")
    plt.close(fig)
    print("wrote", FIG / name)


# F1: who produces each activity type
labels = {"issue_comment": "Issue/PR comments", "issue": "Issues opened", "pr": "PRs opened",
          "pr_merged": "PRs merged", "commit": "Commits", "review": "Reviews", "review_comment": "Review comments"}
activity = {r["kind"]: r for r in read("health_activity.csv")}
kinds = [k for k in ["issue_comment", "issue", "pr", "pr_merged", "commit", "review", "review_comment"] if k in activity]

fig, ax = plt.subplots(figsize=(6.2, 2.9))
for i, k in enumerate(kinds):
    r = activity[k]
    total = int(r["total"])
    left = 0.0
    for g, name, colour in GROUPS:
        share = (int(r[g]) + (int(r["deleted"]) if g == "casual" else 0)) / total
        ax.barh(i, share, left=left, color=colour, edgecolor="white", linewidth=0.5, label=name if i == 0 else None)
        if share >= 0.07:
            ax.text(left + share / 2, i, f"{100 * share:.0f}", ha="center", va="center", fontsize=7,
                    color="white" if g in ("bot", "key", "regular") else "black")
        left += share
    ax.text(1.01, i, f"{total:,}", va="center", fontsize=7.5)
ax.text(1.01, len(kinds) - 0.35, "events", fontsize=7.5, style="italic")
ax.set_yticks(range(len(kinds)), [labels[k] for k in kinds])
ax.invert_yaxis()
ax.set_xlim(0, 1)
ax.xaxis.set_major_formatter(plt.FuncFormatter(lambda x, _: f"{100 * x:.0f}%"))
ax.set_xlabel("Share of events in the window (numbers in bars: %)")
ax.legend(ncol=5, loc="lower center", bbox_to_anchor=(0.5, 1.0), frameon=False, fontsize=8, handlelength=1.2)
save(fig, "activity_by_group.pdf")


# F2: where the thresholds come from
rank = read("health_rank.csv")
gaps = np.array([float(r["gap_days"]) for r in read("health_gaps.csv")])
ranks = np.array([int(r["rank"]) for r in rank])
cum = np.array([float(r["cum_share"]) for r in rank])
team = np.array([r["team"] == "True" for r in rank])
knee = int(ranks[np.argmax(cum >= KEY_SHARE)])
p90 = float(np.quantile(gaps, DORMANT_Q))

fig, (a1, a2) = plt.subplots(1, 2, figsize=(6.2, 2.6))
a1.plot(ranks, cum, color=C_REG, lw=1.2)
show = ranks <= 30
a1.scatter(ranks[show & team], cum[show & team], s=12, color=C_KEY, zorder=3, label="in TEAM_MEMBERS")
a1.scatter(ranks[show & ~team], cum[show & ~team], s=12, facecolor="white", edgecolor=C_REG, zorder=3, label="not in team")
a1.axhline(KEY_SHARE, color=C_GREY, ls="--", lw=0.8)
a1.axvline(knee, color=C_GREY, ls="--", lw=0.8)
a1.annotate(f"80% at rank {knee}", (knee, KEY_SHARE), xytext=(knee * 2.2, 0.55), fontsize=8,
            arrowprops=dict(arrowstyle="-", color="black", lw=0.6))
a1.set_xscale("log")
a1.set_xlabel("Persons ranked by merged PRs (log)")
a1.set_ylabel("Cumulative share of merged PRs")
a1.set_ylim(0, 1.02)
a1.yaxis.set_major_formatter(plt.FuncFormatter(lambda y, _: f"{100 * y:.0f}%"))
a1.legend(loc="lower right", frameon=False, fontsize=7.5, handletextpad=0.3)
a1.set_title("(a) Key contributors", fontsize=9)

bins = np.unique(np.round(np.logspace(0, np.log10(gaps.max() + 1), 30)))
a2.hist(gaps, bins=bins, color=C_REG, edgecolor="white", linewidth=0.4)
a2.axvline(p90, color=C_KEY, ls="--", lw=1)
a2.text(p90 * 1.1, a2.get_ylim()[1] * 0.85, f"P90 = {p90:.0f} days", color=C_KEY, fontsize=8)
a2.set_xscale("log")
a2.set_yscale("log")
a2.set_xlabel("Days between active days of regulars (log)")
a2.set_ylabel("Gaps (log)")
a2.set_title("(b) Dormant threshold", fontsize=9)
fig.tight_layout(w_pad=2)
save(fig, "thresholds.pdf")


# F3: active persons per month, newcomer retention, auto-closed PRs, policy events
monthly = [r for r in read("health_monthly.csv") if r["month"] != "2025-09"]  # window starts 27 Sep: 4 days only
months = [dt.date(int(r["month"][:4]), int(r["month"][5:]), 15) for r in monthly]
x = mdates.date2num(months)
fig, (top, bot) = plt.subplots(2, 1, figsize=(6.2, 4.4), sharex=True, gridspec_kw={"height_ratios": [1.25, 1]})
bottom = np.zeros(len(monthly))
for g, name, colour in [("key", "Key", C_KEY), ("regular", "Regular", C_REG), ("occasional", "Occasional", C_OCC),
                        ("casual", "Casual", C_CAS)]:
    v = np.array([int(r[g]) for r in monthly])
    top.bar(x, v, width=24, bottom=bottom, color=colour, edgecolor="white", linewidth=0.4, label=name)
    bottom += v
newc = np.array([int(r["newcomers"]) for r in monthly])
top.plot(x, newc, color="black", marker="o", ms=2.5, lw=1, label="Newcomers (first contribution)")
top.set_ylabel("Active human persons")
top.legend(ncol=5, loc="lower center", bbox_to_anchor=(0.5, 1.08), frameon=False, fontsize=7.5, handlelength=1.2,
           columnspacing=1.0)
top.yaxis.set_major_formatter(plt.FuncFormatter(lambda y, _: f"{y:,.0f}"))

elig = np.array([int(r["newcomers_eligible"]) for r in monthly])
ret = np.array([int(r["newcomers_retained"]) for r in monthly])
retention = np.where(elig > 0, 100 * ret / np.maximum(elig, 1), np.nan)
full = np.array([int(r["newcomers_eligible"]) == int(r["newcomers"]) for r in monthly])
retention[~full] = np.nan  # cohort not fully observable for 90 days
prs = np.array([int(r["human_prs"]) for r in monthly])
auto = 100 * np.array([int(r["auto_closed_prs"]) for r in monthly]) / np.maximum(prs, 1)
bot.plot(x, retention, color=C_REG, marker="o", ms=3, lw=1.2, label=f"Newcomers active again after 30–{HORIZON} days")
bot.plot(x, auto, color=C_KEY, marker="s", ms=3, lw=1.2, label="Human PRs closed by automation")
bot.set_ylabel("Share (%)")
bot.set_ylim(0, 55)
bot.legend(loc="upper left", frameon=False, fontsize=7.5)
bot.annotate("PRs opened last month\nnot yet auto-closed", (x[-1], auto[-1]), xytext=(x[-1] - 95, 4), fontsize=7,
             color="dimgray", arrowprops=dict(arrowstyle="-", color="dimgray", lw=0.6))

for day, text in EVENTS:
    for ax in (top, bot):
        ax.axvline(mdates.date2num(day), color="dimgray", ls=":", lw=0.9)
    top.text(mdates.date2num(day), top.get_ylim()[1] * 1.01, text, fontsize=7, ha="center", va="bottom", color="black",
             bbox=dict(boxstyle="circle,pad=0.15", fc="white", ec="dimgray", lw=0.6))
bot.xaxis.set_major_locator(mdates.MonthLocator(bymonth=[1, 3, 5, 7, 9, 11]))
bot.xaxis.set_major_formatter(mdates.DateFormatter("%b %Y"))
fig.tight_layout(h_pad=0.6)
save(fig, "monthly.pdf")


# F4: Understand ownership per package vs merged persons
pk = [r for r in read("und_persons.csv") if int(r["und_authors"]) >= 5]
pk.sort(key=lambda r: int(r["und_authors"]))
names = [r["package"] for r in pk]
y = np.arange(len(pk))
und = np.array([int(r["und_authors"]) for r in pk])
persons = np.array([int(r["persons"]) for r in pk])
humans = np.array([int(r["human_persons"]) for r in pk])
top_share = np.array([float(r["top_human_share"]) for r in pk])
bot_share = np.array([float(r["bot_commit_share"]) for r in pk])
und_own = np.array([float(r["und_ownership"]) for r in pk])

fig, (l, r) = plt.subplots(1, 2, figsize=(6.2, 4.1), sharey=True, gridspec_kw={"width_ratios": [1.15, 1]})
l.hlines(y, humans, und, color=C_GREY, lw=1.5, zorder=1)
l.scatter(und, y, color="black", s=16, zorder=3, label="Understand GitAuthors (raw names)")
l.scatter(humans, y, color=C_REG, s=16, zorder=3, label="Human persons after filtering")
for yi, u, h in zip(y, und, humans):
    if u > h:
        l.text(u * 1.08, yi, f"−{100 * (u - h) / u:.0f}%", va="center", fontsize=6.5, color="dimgray")
l.set_xscale("log")
l.set_xlim(2, 1000)
l.set_yticks(y, names)
l.set_xlabel("Authors of the package's files (log)")
l.legend(loc="lower center", bbox_to_anchor=(0.5, 1.07), frameon=False, fontsize=7, handletextpad=0.2, ncol=1)
l.set_title("(a) Who touched the package", fontsize=9)

r.barh(y, top_share, color=C_KEY, height=0.65, label="Top human person")
r.barh(y, bot_share, left=top_share, color=C_BOT, height=0.65, label="All bots")
r.scatter(und_own, y, marker="|", s=60, color="black", zorder=3, label="Understand GitOwnership")
for yi, rr, ts in zip(y, pk, top_share):
    r.text(1, yi, rr["top_human"], va="center", fontsize=6.5, color="white" if ts > 22 else "black")
r.set_xlim(0, 100)
r.set_xlabel("Share of the package's commits (%)")
r.legend(loc="lower center", bbox_to_anchor=(0.5, 1.07), frameon=False, fontsize=7, handletextpad=0.3, ncol=2,
         columnspacing=0.8)
r.set_title("(b) Who owns it", fontsize=9)
fig.tight_layout(w_pad=1)
save(fig, "ownership.pdf")
