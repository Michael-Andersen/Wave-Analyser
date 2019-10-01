﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Wave_Analyser
{
	class DrawTools
	{
		public static void Text(Canvas canvas, double x, double y, string text, Brush colour)
		{
			TextBlock textBlock = new TextBlock();
			textBlock.Text = text;
			textBlock.Foreground = colour;
			Canvas.SetLeft(textBlock, x);
			Canvas.SetTop(textBlock, y);
			canvas.Children.Add(textBlock);
		}

		public static void DrawLine(Canvas canvas, double x1, double x2, double y1, double y2, Brush color, double thickness = 1)
		{
			Line line = new Line();
			line.X1 = x1;
			line.X2 = x2;
			line.Y1 = y1;
			line.Y2 = y2;
			line.Stroke = color;
			line.StrokeThickness = thickness;
			canvas.Children.Add(line);
		}
	}
}
