using System;
using System.Threading.Tasks;

namespace TsNode.Foundations
{
    internal static class EasingHelper
    {
        public static double Lerp(double a, double b, double t)
        {
            return a * (1 - t) + b * t;
        }

        public static async Task StartAnimation(Action<double> action , TimeSpan timeSpan , Func<double,double> easing = null)
        {
            if (easing is null)
                easing = Cubic;
            
            var end = DateTime.Now + timeSpan;

            while (DateTime.Now <= end)
            {
                var diff = end - DateTime.Now;
                
                // 60fps
                await Task.Delay(17);

                var t = 1 - (diff.TotalMilliseconds / timeSpan.TotalMilliseconds);
                t = easing(t);

                action(t);
            }

            action(1.0);
        }
        
        public static double None(double t)
        {
            return t;
        }
        
        public static double Cubic(double t)
        {
            return t * t * (3 - 2 * t);
        }

        public static double EaseIn(double t)
        {
            return t * t;
        }
        
        public static double EaseOut(double t)
        {
            return t * ( 2 - t );
        }
    }
    

}