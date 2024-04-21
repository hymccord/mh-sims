from sjvisualizer import DataHandler, Canvas, BarRace, StaticImage
import json

EXCEL_FILE = "data.xlsx"
FPS = 60
DURATION = 0.5

# load the data into a data frame
df = DataHandler.DataHandler(excel_file=EXCEL_FILE, number_of_frames=FPS*DURATION*60).df

# creating the canvas
canvas = Canvas.canvas()

# adding the racing bar chart
bar_chart = BarRace.bar_race(df=df, canvas=canvas.canvas)
canvas.add_sub_plot(bar_chart)

# adding a title
canvas.add_title("Room Counts By Strat", color=(0,0,0))
canvas.add_sub_title("For 10,000 visits", color=(150,150,150))

# adding a time
canvas.add_time(df=df, time_indicator="year")

# play the animation
canvas.play(fps=FPS)
