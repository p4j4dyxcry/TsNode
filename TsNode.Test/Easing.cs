using TsNode.Foundations;
using Xunit;

namespace TsNode.Test
{
    public class Easing
    {
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