"""
Figures for Chapter 8 (Collaboration)
"""
import csv
import math
from pathlib import Path

import matplotlib.pyplot as plt
from matplotlib.patches import FancyArrowPatch

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / "out"
FIG = ROOT.parent / "interim_report" / "Figures"
FIG.mkdir(parents=True, exist_ok=True)

PAIR1 = frozenset({"kitlangton", "rekram1-node"})
PAIR2 = frozenset({"Hona", "Brendonovich"})
COUNTER = frozenset({"nexxeln", "jlongster"})
HUB = frozenset({"rekram1-node", "Hona"})
C_P1, C_P2, C_CE, C_GREY = "#1f77b4", "#2ca02c", "#d62728", "#9a9a9a"

plt.rcParams.update({"font.family": "serif", "font.size": 9, "pdf.fonttype": 42})


def read(name):
    with open(OUT / name, newline="") as f:
        return list(csv.DictReader(f))


pairs = read("pairs_ranked.csv")
overlap = {frozenset({r["a"], r["b"]}): r for r in read("counter_example.csv")}

# directed counts: edges[(x, y)] = interactions from x to y
edges = {}
people = set()
for r in pairs:
    a, b = r["a"], r["b"]
    people |= {a, b}
    edges[(b, a)] = sum(int(r[k]) for k in ("b_on_a_reviews", "b_on_a_comments", "b_mentions_a"))
    edges[(a, b)] = sum(int(r[k]) for k in ("a_on_b_reviews", "a_on_b_comments", "a_mentions_b"))


def colour(pair):
    return {PAIR1: C_P1, PAIR2: C_P2, COUNTER: C_CE}.get(pair, C_GREY)


# Figure 1: directed interaction graph
order = ["kitlangton", "rekram1-node", "Hona", "Brendonovich", "iamdavidhill",
         "adamdotdevin", "fwang", "thdxr", "jlongster", "nexxeln"]
order = [p for p in order if p in people] + sorted(people - set(order))
n = len(order)
pos = {p: (math.cos(math.pi / 2 - 2 * math.pi * i / n), math.sin(math.pi / 2 - 2 * math.pi * i / n))
       for i, p in enumerate(order)}

fig, ax = plt.subplots(figsize=(6.2, 5.6))
maxw = max(edges.values())
for (x, y), w in sorted(edges.items(), key=lambda e: e[1]):
    if w == 0:
        continue
    pair = frozenset({x, y})
    c = colour(pair)
    highlight = c != C_GREY
    ax.add_patch(FancyArrowPatch(
        pos[x], pos[y], connectionstyle="arc3,rad=0.15", arrowstyle="-|>",
        mutation_scale=7 + 7 * w / maxw, lw=(0.8 if highlight else 0.3) + 3.5 * w / maxw, color=c,
        alpha=0.95 if highlight else 0.3, shrinkA=16, shrinkB=16, zorder=2 if highlight else 1))
    if highlight or pair == HUB:
        (x0, y0), (x1, y1) = pos[x], pos[y]
        mx, my = (x0 + x1) / 2, (y0 + y1) / 2
        dx, dy = x1 - x0, y1 - y0
        # offset label to the side the arc bends to (right of travel direction)
        L = math.hypot(dx, dy)
        lx, ly = mx + 0.15 * dy / L * L * 0.5 * 1.0, my - 0.15 * dx / L * L * 0.5
        ax.text(lx, ly, str(w), ha="center", va="center", fontsize=8, color=c if highlight else "#444",
                bbox=dict(boxstyle="round,pad=0.15", fc="white", ec="none", alpha=0.9), zorder=4)

# counter-example: dashed line, no interaction
(x0, y0), (x1, y1) = pos["nexxeln"], pos["jlongster"]
ax.plot([x0, x1], [y0, y1], ls=(0, (4, 3)), color=C_CE, lw=1.4, zorder=1)
ax.text((x0 + x1) / 2, (y0 + y1) / 2, "151 shared files,\n0 interactions", ha="center", va="center",
        fontsize=7.5, color=C_CE, bbox=dict(boxstyle="round,pad=0.2", fc="white", ec=C_CE, lw=0.6), zorder=4)

for p, (x, y) in pos.items():
    c = colour(PAIR1) if p in PAIR1 else colour(PAIR2) if p in PAIR2 else colour(COUNTER) if p in COUNTER else "#555"
    ax.scatter([x], [y], s=380, color="white", edgecolor=c, linewidth=1.8, zorder=5)
    ha = "center" if abs(x) < 0.3 else ("left" if x > 0 else "right")
    ax.text(x * 1.13, y * 1.15, p, ha=ha, va="center", fontsize=9, family="monospace", zorder=6)

ax.plot([], [], color=C_P1, lw=2.5, label="Pair 1")
ax.plot([], [], color=C_P2, lw=2.5, label="Pair 2")
ax.plot([], [], color=C_CE, lw=1.4, ls=(0, (4, 3)), label="Counter-example")
ax.plot([], [], color=C_GREY, lw=2, alpha=0.6, label="Other pairs")
ax.legend(loc="lower center", bbox_to_anchor=(0.5, -0.08), ncol=4, frameon=False, fontsize=8)
ax.set_xlim(-1.45, 1.45)
ax.set_ylim(-1.35, 1.35)
ax.set_aspect("equal")
ax.axis("off")
fig.tight_layout()
fig.savefig(FIG / "ch8_interaction_graph.pdf", bbox_inches="tight")
plt.close(fig)

# Figure 2: shared files vs. interaction
fig, ax = plt.subplots(figsize=(6.2, 3.6))
pts = []
for r in pairs:
    key = frozenset({r["a"], r["b"]})
    o = overlap.get(key)
    if o is None:
        continue
    pts.append((key, int(o["shared_files"]), int(r["total"]), int(r["weaker_direction"]), float(o["jaccard"])))

for key, sh, tot, wd, j in pts:
    c = colour(key)
    if c == C_GREY and key != HUB:
        ax.scatter(sh, tot, s=28, facecolor=C_GREY, edgecolor="white", linewidth=0.5, alpha=0.7, zorder=2)
    else:
        ax.scatter(sh, tot, s=25 + 22 * wd, facecolor=c, edgecolor="white", linewidth=0.6,
                   alpha=0.95, zorder=3)

ax.scatter([], [], s=28, facecolor=C_GREY, edgecolor="white", alpha=0.7, label="other pairs of top-10 members")
ax.legend(loc="upper center", bbox_to_anchor=(0.5, -0.2), frameon=False, fontsize=7.5,
          title="(area of coloured markers ~ weaker direction)", title_fontsize=7)

labels = {PAIR1: ("Pair 1: kitlangton / rekram1-node\n(weaker direction 8, Jaccard 0.07)", (14, 18)),
          PAIR2: ("Pair 2: Hona / Brendonovich\n(weaker direction 7, Jaccard 0.21)", (6, 24)),
          COUNTER: ("Counter-example: nexxeln / jlongster\n151 shared files, 0 interactions", (10, 22)),
          HUB: ("rekram1-node / Hona\n(total 35, weaker direction 5)", (-14, 18))}
for key, sh, tot, wd, j in pts:
    if key in labels:
        text, off = labels[key]
        ax.annotate(text, (sh, tot), xytext=off, textcoords="offset points", fontsize=7.5,
                    color=colour(key) if colour(key) != C_GREY else "#444",
                    ha="right" if off[0] < 0 else "left",
                    arrowprops=dict(arrowstyle="-", lw=0.5, color="#666"))

ax.axhspan(-0.5, 0.5, color=C_CE, alpha=0.06, lw=0, zorder=0)
ax.text(318, 0.12, "no interaction at all (10 pairs)", ha="right", va="center", fontsize=7, color=C_CE)
ax.set_xlabel("Shared changed files (sampled merged PRs, seed 42)")
ax.set_ylabel("Total interactions\n(both directions)")
ax.set_yscale("symlog", linthresh=1)
ax.set_yticks([0, 1, 2, 5, 10, 20, 40])
ax.set_yticklabels(["0", "1", "2", "5", "10", "20", "40"])
ax.set_xlim(-8, 320)
ax.set_ylim(-0.5, 200)
ax.grid(True, lw=0.4, alpha=0.4)
ax.spines[["top", "right"]].set_visible(False)
fig.tight_layout()
fig.savefig(FIG / "ch8_overlap_vs_interaction.pdf", bbox_inches="tight")
plt.close(fig)
print("wrote", FIG / "ch8_interaction_graph.pdf", "and", FIG / "ch8_overlap_vs_interaction.pdf")
