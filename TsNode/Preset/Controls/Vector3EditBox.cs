using System;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;

namespace TsNode.Preset.Controls
{
    /// <summary>
    /// Vector3の編集コントロール
    /// </summary>
    [TemplatePart(Name="PART_X",Type=typeof(TextBox))]
    [TemplatePart(Name="PART_Y",Type=typeof(TextBox))]
    [TemplatePart(Name="PART_Z",Type=typeof(TextBox))]
    public class Vector3EditBox : Control
    {
        public static readonly DependencyProperty ValueProperty =
            DepProp.Register<Vector3EditBox, Vector3>(nameof(Value),FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,ValueChanged);

        public Vector3 Value
        {
            get => (Vector3) GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty XProperty =
            DepProp.Register<Vector3EditBox, float>(nameof(X), (x, e) =>
            {
                if (x is Vector3EditBox vector3EditBox)
                {
                    vector3EditBox.SetCurrentValue(ValueProperty,
                        new Vector3(vector3EditBox.X,vector3EditBox.Value.Y,vector3EditBox.Value.Z));
                }
            });

        public float X
        {
            get => (float) GetValue(XProperty);
            set => SetValue(XProperty, value);
        }

        public static readonly DependencyProperty YProperty =
            DepProp.Register<Vector3EditBox, float>(nameof(Y), (x, e) =>
            {
                if (x is Vector3EditBox vector3EditBox)
                {
                    vector3EditBox.SetCurrentValue(ValueProperty,
                        new Vector3(vector3EditBox.Value.X,vector3EditBox.Y,vector3EditBox.Value.Z));
                }
            });

        public float Y
        {
            get => (float) GetValue(YProperty);
            set => SetValue(YProperty, value);
        }

        public static readonly DependencyProperty ZProperty =
            DepProp.Register<Vector3EditBox, float>(nameof(Z), (x, e) =>
            {
                if (x is Vector3EditBox vector3EditBox)
                {
                    vector3EditBox.SetCurrentValue(ValueProperty,
                        new Vector3(vector3EditBox.Value.X,vector3EditBox.Value.Y,vector3EditBox.Z));
                }
            });

        public float Z
        {
            get => (float) GetValue(ZProperty);
            set => SetValue(ZProperty, value);
        }

        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is Vector3EditBox v)
                v.OnValueChanged();
        }

        private void OnValueChanged()
        {
            if (Math.Abs(X - Value.X) > 0.0001d)
                SetCurrentValue(XProperty,Value.X);

            if (Math.Abs(Y - Value.Y) > 0.0001d)
                SetCurrentValue(YProperty,Value.Y);

            if (Math.Abs(Z - Value.Z) > 0.0001d)
                SetCurrentValue(ZProperty,Value.Z);
        }

        public Vector3EditBox()
        {
            SetCurrentValue(FocusableProperty,false);
        }
    }
}