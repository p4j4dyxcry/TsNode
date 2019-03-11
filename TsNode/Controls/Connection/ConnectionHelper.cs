using System.Windows;
using System.Windows.Media;

namespace TsNode.Controls.Connection
{
    internal static class ConnectionHelper
    {
        public struct BezierConnectionInfo
        {
            public Point Start { get; set; }
            public Point End { get; set; }
            public Point Center { get; set; }
            public Point StartToCenter { get; set; }
            public Point CenterToEnd { get; set; }
        }

        internal static BezierConnectionInfo CalcBezierInfo(double sourceX, double sourceY, double destX, double destY)
        {
            var source = new Vector(sourceX, sourceY);
            var dest = new Vector(destX, destY);

            var scaler = 0.25d;
            var invScaler = 1.0d - scaler;

            bool isSourceLarge = source.X > dest.X;

            var sourceCenter = new Point
            {
                X = isSourceLarge ?
                    source.X + (source.X - dest.X) * invScaler
                    : source.X - (source.X - dest.X) * scaler,
                Y = source.Y
            };

            var destCenter = new Point
            {
                X = isSourceLarge ?
                    dest.X - (source.X - dest.X) * invScaler
                    : source.X - (source.X - dest.X) * invScaler,
                Y = dest.Y
            };

            var center = (source - (source - dest) / 2.0d);

            return new BezierConnectionInfo()
            {
                Start = source.ToPoint(),
                End = dest.ToPoint(),
                Center = center.ToPoint(),
                StartToCenter = sourceCenter,
                CenterToEnd = destCenter
            };
        }

        internal static Geometry MakeBezierPathGeometry(BezierConnectionInfo bezierInfo)
        {
            PathFigure MakePath(Point start, Point center, Point end)
            {
                var figure = new PathFigure()
                {
                    StartPoint = start,
                    IsClosed = false,
                };
                var segment = new QuadraticBezierSegment
                {
                    Point1 = center,
                    Point2 = end
                };
                figure.Segments.Add(segment);
                return figure;
            }

            var geometry = new PathGeometry();
            {
                geometry.Figures.Add(MakePath(bezierInfo.Start, bezierInfo.StartToCenter, bezierInfo.Center));
                geometry.Figures.Add(MakePath(bezierInfo.Center, bezierInfo.CenterToEnd, bezierInfo.End));
            }

            return geometry;
        }
    }
}
