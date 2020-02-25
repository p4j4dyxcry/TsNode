using System.Linq;
using System.Windows;
using System.Windows.Interactivity;

namespace TsControls.Behaviors
{
    /// <summary>
    /// スタイルにビヘイビアを設定するためのクラス
    /// </summary>
    public class StyleBehaviorCollection : FreezableCollection<Behavior>
    {
        public static readonly DependencyProperty StyleBehaviorsProperty =
            DependencyProperty.RegisterAttached(
                "StyleBehaviors", 
                typeof(StyleBehaviorCollection), 
                typeof(StyleBehaviorCollection), 
                new PropertyMetadata((sender, e) =>
                {
                    // 2回目に設定された場合は前回の値をクリアしないので注意。
                    if (e.OldValue == e.NewValue) { return; }

                    var value = e.NewValue as StyleBehaviorCollection;
                    if (value == null) { return; }

                    var behaviors = Interaction.GetBehaviors(sender);
                    foreach (var b in value.Select(x => (Behavior)x.Clone()))
                    {
                        behaviors.Add(b);
                    }
                }));

        public static StyleBehaviorCollection GetStyleBehaviors(DependencyObject obj)
        {
            return (StyleBehaviorCollection)obj.GetValue(StyleBehaviorsProperty);
        }

        public static void SetStyleBehaviors(DependencyObject obj, StyleBehaviorCollection value)
        {
            obj.SetValue(StyleBehaviorsProperty, value);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new StyleBehaviorCollection();
        }
    }
}