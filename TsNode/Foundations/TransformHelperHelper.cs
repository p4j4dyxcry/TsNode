using System;
using System.Windows;
using TsNode.Interface;
using TsNode.Extensions;

namespace TsNode.Foundations
{
    public static class TransformHolderHelper
    {
        public static TransformResult ComputeFitRect(Rect fitRect ,double width, double height)
        {
            fitRect = fitRect.ValidateRect(width, height);
            
            // compute translate
            var centerPoint = new Point(fitRect.X + fitRect.Width / 2, fitRect.Y + fitRect.Height / 2);
            
            // compute scale
            var scaleW = width / fitRect.Width;
            var scaleH = height / fitRect.Height;
            var newScale = Math.Min(scaleW, scaleH);
            
            return new TransformResult(centerPoint.X,centerPoint.Y,newScale);
        }
    }

    internal static class TransformHolderExtensions
    {
        public static Point TransformPoint(this ITransformHolder self, double x, double y)
        {
            return new Point(
                (x - self.TranslateMatrix.X) / self.ScaleMatrix.ScaleX,
                (y - self.TranslateMatrix.Y) / self.ScaleMatrix.ScaleY);
        }

        public static void Scale(this ITransformHolder self, double scale, double centerX, double centerY)
        {
            var d0 = self.TransformPoint(centerX, centerY);

            self.ScaleMatrix.ScaleX = scale;
            self.ScaleMatrix.ScaleY = scale;

            var d1 = self.TransformPoint(centerX, centerY);

            var diff = d1 - d0;

            self.TranslateMatrix.X += diff.X * scale;
            self.TranslateMatrix.Y += diff.Y * scale;
        }

        public static void Translate(this ITransformHolder self, double offsetX , double offsetY)
        {
            self.TranslateMatrix.X += offsetX ;
            self.TranslateMatrix.Y += offsetY ;
        }
        public static void SetTranslate(this ITransformHolder self, double offsetX, double offsetY)
        {
            self.TranslateMatrix.X = offsetX;
            self.TranslateMatrix.Y = offsetY;
        }

        public static void SetTranslateX(this ITransformHolder self, double offsetX)
        {
            self.TranslateMatrix.X = offsetX;
        }
        public static void SetTranslateY(this ITransformHolder self, double offsetY)
        {
            self.TranslateMatrix.Y = offsetY;
        }
        public static void TranslateX(this ITransformHolder self, double offsetX)
        {
            self.TranslateMatrix.X += offsetX;
        }
        public static void TranslateY(this ITransformHolder self, double offsetY)
        {
            self.TranslateMatrix.Y += offsetY;
        }
        public static Point GetTranslateToPosition(this ITransformHolder self )
        {
            return new Point(self.TranslateMatrix.X, self.TranslateMatrix.Y);
        }
    }
    
    public class TransformResult
    {
        public double X { get; }
        public double Y { get; }
        public double Scale { get; }
        
        public TransformResult(double x, double y, double scale)
        {
            X = x;
            Y = y;
            Scale = scale;
        }
    }
}