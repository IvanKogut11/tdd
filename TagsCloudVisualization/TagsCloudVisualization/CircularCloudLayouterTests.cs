﻿using NUnit.Framework;
using System;
using FluentAssertions;
using System.Drawing;
using System.Linq;
using NUnit.Framework.Interfaces;
using System.Collections.Generic;

namespace TagsCloudVisualization
{
    [TestFixture]
    class CircularCloudLayouterTests
    {
        private CircularCloudLayouter circularCloudLayouter;
        private List<Size> rectangleSizes = new List<Size>();

        [TearDown]
        public void LogOutputOnFailedTests()
        {
            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
            {
                var drawer = new TagCloudDrawingClass(3000, 3000, TestContext.CurrentContext.Test.FullName
                    + ".bmp",
                    circularCloudLayouter.cloudCenter);
                drawer.DrawTagCloud(rectangleSizes);
            }
            rectangleSizes = new List<Size>();
        }

        [Test]
        [TestCase(3, 2)]
        [TestCase(-3, 2)]
        [TestCase(3, -2)]
        [TestCase(-3, -2)]
        [TestCase(0, 0)]
        public void Create_IfCorrectCentrePoint(int x, int y)
        {
            circularCloudLayouter = new CircularCloudLayouter(new Point(x, y));
            circularCloudLayouter.GetType().Name.Should().Be(nameof(CircularCloudLayouter));
        }

        [Test]
        public void PutAndReturnOneRectangle_IfAllPlaneIsFree()
        {
            circularCloudLayouter = new CircularCloudLayouter(new Point(50, 50));
            rectangleSizes.Add(new Size(10, 10));

            var rectangle = circularCloudLayouter.PutNextRectangle(rectangleSizes[0]);

            rectangle.Should().NotBe(null);
        }

        [Test]
        public void PutOneRectangleInCenter_IfAllPlaneIsFree()
        {
            circularCloudLayouter = new CircularCloudLayouter(new Point(50, 50));
            rectangleSizes.Add(new Size(7, 10));

            var rectangle = circularCloudLayouter.PutNextRectangle(rectangleSizes[0]);

            circularCloudLayouter.cloudCenter.X.Should().BeInRange(rectangle.Left, rectangle.Right);
            circularCloudLayouter.cloudCenter.Y.Should().BeInRange(rectangle.Top, rectangle.Bottom);
        }

        [Test]
        [TestCase(0, 5)]
        [TestCase(3, -5)]
        [TestCase(-3, -5)]
        [TestCase(-3, 5)]
        public void ThrowsArgumentException_IfNotPositiveSizeOfRectangle(int width, int height)
        {
            circularCloudLayouter = new CircularCloudLayouter(new Point(50, 50));
            rectangleSizes.Add(new Size(width, height));
            Assert.Throws<ArgumentException>(() => circularCloudLayouter.PutNextRectangle(rectangleSizes[0]));
        }

        [Test]
        public void PutTwoRectangles_OnFreePlane()
        {
            circularCloudLayouter = new CircularCloudLayouter(new Point(50, 50));
            rectangleSizes.Add(new Size(10, 20));
            rectangleSizes.Add(new Size(13, 6));

            var rectangle1 = circularCloudLayouter.PutNextRectangle(rectangleSizes[0]);
            var rectangle2 = circularCloudLayouter.PutNextRectangle(rectangleSizes[1]);

            rectangle1.Should().NotBe(null);
            rectangle2.Should().NotBe(null);
            rectangle1.Should().NotBeEquivalentTo(rectangle2);
        }

        [Test]
        public void PutTwoRectangles_AndTheyAreNotIntersected()
        {
            circularCloudLayouter = new CircularCloudLayouter(new Point(50, 50));
            rectangleSizes.Add(new Size(10, 20));
            rectangleSizes.Add(new Size(50, 50));

            var rectangle1 = circularCloudLayouter.PutNextRectangle(rectangleSizes[0]);
            var rectangle2 = circularCloudLayouter.PutNextRectangle(rectangleSizes[1]);

            rectangle1.IntersectsWith(rectangle2).Should().Be(false);
        }

        [Test]
        public void PutThreeRectangles_AndTheyAreNotIntersected()
        {
            circularCloudLayouter = new CircularCloudLayouter(new Point(50, 50));
            rectangleSizes.Add(new Size(10, 20));
            rectangleSizes.Add(new Size(50, 50));
            rectangleSizes.Add(new Size(20, 7));

            var rectangle1 = circularCloudLayouter.PutNextRectangle(rectangleSizes[0]);
            var rectangle2 = circularCloudLayouter.PutNextRectangle(rectangleSizes[1]);
            var rectangle3 = circularCloudLayouter.PutNextRectangle(rectangleSizes[2]);

            rectangle1.IntersectsWith(rectangle2).Should().Be(false);
            rectangle1.IntersectsWith(rectangle3).Should().Be(false);
            rectangle2.IntersectsWith(rectangle3).Should().Be(false);
        }

        [Test]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        [TestCase(500)]
        [TestCase(1000)]
        public void PutManyRectangles_AndTheyAreNotIntersected(int rectanglesCount)
        {
            circularCloudLayouter = new CircularCloudLayouter(new Point(100, 100));
            const int randomRange = 1000;
            var random = new Random(randomRange);
            for (var i = 0; i < rectanglesCount; i++)
               rectangleSizes.Add(new Size(1 + random.Next(randomRange), 1 + random.Next(randomRange)));
            var rectangles = new Rectangle[rectanglesCount];

            for (var i = 0; i < rectanglesCount; i++)
                rectangles[i] = circularCloudLayouter.PutNextRectangle(rectangleSizes[i]);
            rectangles
                .SelectMany(r1 => rectangles.Select((r2) => r1 != r2 && r1.IntersectsWith(r2)))
                .Any(x => x)
                .Should()
                .BeFalse();
        }

        [Test]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        [TestCase(500)]
        [TestCase(1000)]
        public void PutManyRectangles_AndTheyAreTight(int rectanglesCount)
        {
            circularCloudLayouter = new CircularCloudLayouter(new Point(100, 100));
            const int randomRange = 1000;
            var random = new Random(randomRange);
            for (var i = 0; i < rectanglesCount; i++)
                rectangleSizes.Add(new Size(1 + random.Next(randomRange), 1 + random.Next(randomRange)));

            double maxDistanceFromCenter = 0;
            double tagCloudSquare = 0;
            for (var i = 0; i < rectanglesCount; i++)
            {
                var rectangle = circularCloudLayouter.PutNextRectangle(rectangleSizes[i]);
                var vertices = new Point[]
                {
                    new Point(rectangle.Left, rectangle.Bottom),
                    new Point(rectangle.Left, rectangle.Top),
                    new Point(rectangle.Right, rectangle.Bottom),
                    new Point(rectangle.Right, rectangle.Top),
                };
                foreach (var vertex in vertices)
                {
                    maxDistanceFromCenter = Math.Max(maxDistanceFromCenter,
                        CalcDistanceBetweenPoints(circularCloudLayouter.cloudCenter, vertex));
                }
                tagCloudSquare += rectangleSizes[i].Width * rectangleSizes[i].Height;
            }
            double circleArea = Math.PI * maxDistanceFromCenter * maxDistanceFromCenter;
            double squaresRatio = circleArea / tagCloudSquare;
            squaresRatio.Should().BeLessOrEqualTo(3);
        }

        private double CalcDistanceBetweenPoints(Point a, Point b) =>
            Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
    }
}