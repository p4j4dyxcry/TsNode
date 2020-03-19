using System;
using System.Windows;

namespace TsNode.Extensions
{
    public static class MathExtensions
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

        public static bool HitTest(this Rect a, Rect b)
        {
            return a.Left <= b.Right  && 
                   b.Left <= a.Right  &&
                   a.Top  <= b.Bottom &&
                   b.Top  <= a.Bottom;
        }
        
        
        public static Rect ValidateRect(this Rect rect, double width, double height)
        {
            // offset to fitRect
            {
                var deltaW = Math.Max(width - rect.Width, 0);
                var deltaH = Math.Max(height - rect.Height, 0);
                var offset = Math.Min(deltaW, deltaH) / 2;
                rect.Inflate(offset, offset);                
            }

            return rect;
        }
    }
}