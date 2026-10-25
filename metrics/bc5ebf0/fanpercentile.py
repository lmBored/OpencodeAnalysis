import matplotlib.pyplot as plt
import numpy as np

# Data from Table 2.4
statistics = ['Mean', 'Median', 'p90', 'p99', 'Max']
fan_in = [2.2, 1, 4, 27, 145]
fan_out = [2.2, 1, 6, 20, 94]

x = np.arange(len(statistics))  # the label locations
width = 0.35  # the width of the bars

fig, ax = plt.subplots(figsize=(9, 5))

# Create grouped bars (using logarithmic scale if desired, but linear works with text annotations)
rects1 = ax.bar(x - width/2, fan_in, width, label='Fan-in', color='skyblue')
rects2 = ax.bar(x + width/2, fan_out, width, label='Fan-out', color='salmon')

# Add some text for labels, title and custom x-axis tick labels, etc.
ax.set_ylabel('Value')
ax.set_xlabel('Statistic')
ax.set_title('Summary Statistics of Fan-in and Fan-out')
ax.set_xticks(x)
ax.set_xticklabels(statistics)
ax.legend()

# Add data labels on top of the bars for clarity (since 'Max' is much larger)
def autolabel(rects):
    for rect in rects:
        height = rect.get_height()
        ax.annotate(f'{height}',
                    xy=(rect.get_x() + rect.get_width() / 2, height),
                    xytext=(0, 3),  # 3 points vertical offset
                    textcoords="offset points",
                    ha='center', va='bottom', fontsize=9)

autolabel(rects1)
autolabel(rects2)

fig.tight_layout()

# Show the plot
plt.show()