﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;


namespace TagsCloudVisualization
{
    public class CircularCloudLayouter
    {
        private readonly Point cloudCenter;
        private List<Rectangle> RectanglesList { get; } = new List<Rectangle>();
        private const double SpiralCoefControlsPointShift = 1;
        private double spiralAngle = 0;
        private const double AngleDelta = 0.1;


        public CircularCloudLayouter(Point center)
        {
            cloudCenter = center;
        }

        public Rectangle PutNextRectangle(Size rectangleSize)
        {
            if (rectangleSize.Width <= 0 || rectangleSize.Height <= 0)
                throw new ArgumentException("Sizes of rectangle must be positive");
            var rectangle = Rectangle.Empty;
            do
            {
                var possibleX = (int)(SpiralCoefControlsPointShift * spiralAngle * Math.Cos(spiralAngle) + cloudCenter.X -
                                  rectangleSize.Width / 2.0);
                var possibleY = (int)(SpiralCoefControlsPointShift * spiralAngle * Math.Sin(spiralAngle) + cloudCenter.Y -
                                  rectangleSize.Height / 2.0);
                rectangle = new Rectangle(new Point(possibleX, possibleY), rectangleSize);
                spiralAngle += AngleDelta;
            } while (RectanglesList.Any(r => r.IntersectsWith(rectangle)));
            rectangle = MakeRectangleCloserToCentre(rectangle);
            RectanglesList.Add(rectangle);
            return rectangle;
        }

        private Rectangle MakeRectangleCloserToCentre(Rectangle rectangle)
        {
            var cX = rectangle.X < cloudCenter.X ? 1 : rectangle.X > cloudCenter.X ? -1 : 0;
            var cY = rectangle.Y < cloudCenter.Y ? 1 : rectangle.Y > cloudCenter.Y ? -1 : 0;
            var wasMoved = false;
            do
            {
                wasMoved = false;
                if (rectangle.X != cloudCenter.X)
                {
                    rectangle.X += cX;
                    if (RectanglesList.Any(r => r.IntersectsWith(rectangle)))
                        rectangle.X -= cX;
                    else
                        wasMoved = true;
                }
                if (rectangle.Y != cloudCenter.Y)
                {
                    rectangle.Y += cY;
                    if (RectanglesList.Any(r => r.IntersectsWith(rectangle)))
                        rectangle.Y -= cY;
                    else
                        wasMoved = true;
                }
            } while (wasMoved);
            return rectangle;
        }
    }
}