using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TsNode.Interface
{
    public interface ITransformHolder
    {
        ScaleTransform ScaleMatrix { get; }

        TranslateTransform TranslateMatrix { get; }
    }

    public static class TransformHolderExtensions
    {
        internal static Point TransformPoint(this ITransformHolder self, double x, double y)
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
        public static Point GetTranslateToPosition(this ITransformHolder self)
        {
            return new Point(self.TranslateMatrix.X, self.TranslateMatrix.Y);
        }
    }
}
