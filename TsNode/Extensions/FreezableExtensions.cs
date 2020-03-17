using System.Windows;

namespace TsNode.Extensions
{

    internal static partial class FreezableExtensions
    {
        public static T DoFreeze<T>(this T _this) where T : Freezable
        {
            if (_this.CanFreeze & _this.IsFrozen is false)
                _this.Freeze();
            return _this;
        }
    }
}