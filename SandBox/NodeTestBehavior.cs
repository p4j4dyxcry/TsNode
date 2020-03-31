using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using TsNode.Extensions;

namespace SandBox
{
    public class NodeTestBehavior  : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.MouseLeftButtonDown += (s,e)=>
            {
                Console.WriteLine("MouseLeftButtonDown");                
            };
            
            AssociatedObject.MouseLeave += (s, e) =>
            {
                Console.WriteLine("MouseLeave");
            };
            
            AssociatedObject.MouseMove += (s, e) =>
            {
                Console.WriteLine("MouseMove");
            };
            
            AssociatedObject.MouseLeftButtonUp += (s, e) =>
            {
                Console.WriteLine("MouseLeftButtonUp");
            };

            AssociatedObject.KeyDown += (s, e) =>
            {
                Console.WriteLine("KeyDown");
            };

            AssociatedObject.Loaded += (ss, ee) =>
            {
                var textBox = AssociatedObject.FindChild<TextBox>();

                textBox.KeyDown += (s, e) =>
                {
                    Console.WriteLine("TextBox.KeyDown");
                };
            };

        }
    }
}