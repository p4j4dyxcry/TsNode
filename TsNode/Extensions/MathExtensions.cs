using System;
using System.Windows;

namespace TsNode.Extensions
{
    internal static class MathExtensions
    {
        public static Vector ToVector(this Point point)
        {
            return new Vector(point.X, point.Y);
        }
        
        public static Point ToPoint(this Vector vector)
        {
            return new Point(vector.X, vector.Y);
        }
        
        public static Rect ToOffset(this Rect self , Thickness offset)
        {
            return new Rect(self.X - offset.Left , self.Y - offset.Top , self.Width + offset.Right * 2 , self.Height + offset.Bottom * 2);
        }
        
        public static Rect ToScale(this Rect self , double scaleX , double scaleY)
        {
            return new Rect( self.Left * scaleX , self.Top * scaleY , self.Width * scaleX, self.Height * scaleY );
        }
        
        public static Rect ToRect(this Thickness thickness)
        {
            return new Rect(new Point(thickness.Left,thickness.Top) , new Point(thickness.Right,thickness.Bottom));
        }
        
        public static double Clamp(this double value , double min , double max)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        public static bool HitTest(this Rect self, Rect target)
        {
            return
                Math.Abs(self.X - target.X) < self.Width / 2 + target.Height / 2
                &&
                Math.Abs(self.Y - target.Y) < self.Height / 2 + target.Height / 2;
        }
    }
}