import matplotlib.pyplot as plt
import numpy as np

# Data from the table
buckets = ['0', '1-2', '3-5', '6-10', '11+']
fan_in = [927, 2432, 496, 181, 125]
fan_out = [1937, 1182, 585, 316, 141]

x = np.arange(len(buckets))  # the label locations
width = 0.35  # the width of the bars

fig, ax = plt.subplots(figsize=(8, 5))

# Create grouped bars
rects1 = ax.bar(x - width/2, fan_in, width, label='Fan-in', color='skyblue')
rects2 = ax.bar(x + width/2, fan_out, width, label='Fan-out', color='salmon')

# Add some text for labels, title and custom x-axis tick labels, etc.
ax.set_ylabel('Count')
ax.set_xlabel('Bucket')
ax.set_title('Fan-in and Fan-out Distribution by Bucket')
ax.set_xticks(x)
ax.set_xticklabels(buckets)
ax.legend()

# Optional: Add data labels on top of the bars
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