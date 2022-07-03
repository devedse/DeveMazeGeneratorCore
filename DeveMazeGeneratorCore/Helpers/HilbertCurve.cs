using DeveMazeGeneratorCore.Structures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace DeveMazeGeneratorCore.Helpers
{
    public static class HilbertCurve
    {
        public static List<MazePoint> GetPointsForCurve(int n)
        {
            var points = new List<MazePoint>();
            int d = 0;
            while (d < n * n)
            {
                int x = 0;
                int y = 0;
                D2xy(n, d, ref x, ref y);
                points.Add(new MazePoint(x, y));
                d += 1;
            }
            return points;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Xy2d(int n, int x, int y)
        {
            int rx, ry, s, d = 0;
            for (s = n / 2; s > 0; s /= 2)
            {

                rx = Convert.ToInt32(((x & s) > 0));
                ry = Convert.ToInt32((y & s) > 0);
                d += s * s * ((3 * rx) ^ ry);
                Rot(s, ref x, ref y, rx, ry);
            }
            return d;
        }

        //convert d to (x,y)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void D2xy(int n, int d, ref int x, ref int y)
        {
            int rx, ry, s, t = d;
            x = y = 0;
            for (s = 1; s < n; s *= 2)
            {
                rx = 1 & (t / 2);
                ry = 1 & (t ^ rx);
                Rot(s, ref x, ref y, rx, ry);
                x += s * rx;
                y += s * ry;
                t /= 4;
            }
        }

        //rotate/flip a quadrant appropriately
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Rot(int n, ref int x, ref int y, int rx, int ry)
        {
            if (ry == 0)
            {
                if (rx == 1)
                {
                    x = n - 1 - x;
                    y = n - 1 - y;
                }

                //Swap x and y
                int t = x;
                x = y;
                y = t;
            }
        }

        public static List<string> DrawCurve(List<MazePoint> points, int n)
        {
            var canvas = new char[n, n * 3 - 2];
            for (int i = 0; i < canvas.GetLength(0); i++)
            {
                for (int j = 0; j < canvas.GetLength(1); j++)
                {
                    canvas[i, j] = ' ';
                }
            }

            for (int i = 1; i < points.Count; i++)
            {
                var lastPoint = points[i - 1];
                var curPoint = points[i];
                var deltaX = curPoint.X - lastPoint.X;
                var deltaY = curPoint.Y - lastPoint.Y;
                if (deltaX == 0)
                {
                    Debug.Assert(deltaY != 0, "Duplicate point");
                    //vertical line
                    int row = Math.Max(curPoint.Y, lastPoint.Y);
                    int col = curPoint.X * 3;
                    canvas[row, col] = '|';
                }
                else
                {
                    Debug.Assert(deltaY == 0, "Duplicate point");
                    //horizontal line
                    var row = curPoint.Y;
                    var col = Math.Min(curPoint.X, lastPoint.X) * 3 + 1;
                    canvas[row, col] = '_';
                    canvas[row, col + 1] = '_';
                }
            }

            var lines = new List<string>();
            for (int i = 0; i < canvas.GetLength(0); i++)
            {
                var sb = new StringBuilder();
                for (int j = 0; j < canvas.GetLength(1); j++)
                {
                    sb.Append(canvas[i, j]);
                }
                lines.Add(sb.ToString());
            }
            return lines;
        }
    }
}
