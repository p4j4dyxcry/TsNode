using System.Windows;
using System.Windows.Media;
using TsNode.Extensions;
using TsNode.Foundations;
using TsNode.Interface;
using Xunit;

namespace TsNode.Test
{
    public class TransformHolder : ITransformHolder
    {
        public ScaleTransform ScaleMatrix { get; } = new ScaleTransform();
        public TranslateTransform TranslateMatrix { get; } = new TranslateTransform();
    }
    
    public class Compute
    {
        [Fact]
        public void FitRectNoScale()
        {
            var holder = new TransformHolder();

            var rect = new Rect(0, 0, 500, 500);
            var width = 500;
            var height = 500;
            
            var result = holder.ComputeFitRect( rect , width, height);

            Assert.Equal(250 , result.X);
            Assert.Equal(250 , result.Y);
            Assert.Equal(1 , result.Scale);
        }
        
        [Fact]
        public void FitRectScalingTest()
        {
            var holder = new TransformHolder();

            var rect = new Rect(0, 0, 1000, 1000);
            var width = 500;
            var height = 500;
            
            var result = holder.ComputeFitRect( rect , width, height);

            Assert.Equal(500 , result.X);
            Assert.Equal(500 , result.Y);
            Assert.Equal(0.5 , result.Scale);
        }

        [Fact]
        public void RectOffsetTest()
        {
            var rect = new Rect(-50,50,100,200);
            var result = rect.ToOffset(new Thickness(50));
            
            Assert.Equal(-100 , result.Left);
            Assert.Equal(   0 , result.Top);
            Assert.Equal( 100 , result.Right);
            Assert.Equal( 300 , result.Bottom);
        }

        [Fact]
        public void RectScaleTest()
        {
            var rect = new Rect(-50,50,100,200);
            var result = rect.ToScale(2,4);
            
            Assert.Equal(-100 , result.Left);
            Assert.Equal( 200 , result.Top);
            Assert.Equal( 100 , result.Right);
            Assert.Equal(1000 , result.Bottom);
        }
        
        [Fact]
        public void LerpTest()
        {
            Assert.Equal(0.5  , EasingHelper.Lerp(0, 1, 0.5));
            Assert.Equal(0.25 , EasingHelper.Lerp(0, 1, 0.25));
            Assert.Equal(0.75 , EasingHelper.Lerp(0, 1, 0.75));
        }

        [Fact]
        public void QubicTest()
        {
            Assert.Equal(1,EasingHelper.Cubic(1));
            Assert.Equal(0.5,EasingHelper.Cubic(0.5));
            Assert.Equal(0,EasingHelper.Cubic(0));
        }
        
        [Fact]
        public void EaseInTest()
        {
            Assert.Equal(1,EasingHelper.EaseIn(1));
            Assert.Equal(0,EasingHelper.EaseIn(0));
        }
        
        [Fact]
        public void EaseOutTest()
        {
            Assert.Equal(1,EasingHelper.EaseOut(1));
            Assert.Equal(0,EasingHelper.EaseOut(0));
        }
    }
}