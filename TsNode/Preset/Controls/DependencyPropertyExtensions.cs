
using System.Windows;

namespace TsNode.Preset.Controls
{
    internal static class DepProp
    {
        public static DependencyProperty Register<TOwner, TProperty>(string propertyName)
        {
            return DependencyProperty.Register(propertyName, typeof(TProperty), typeof(TOwner),
                new PropertyMetadata(default(TProperty)));
        }

        public static DependencyProperty Register<TOwner, TProperty>(string propertyName, TProperty @default)
        {
            return DependencyProperty.Register(propertyName, typeof(TProperty), typeof(TOwner),
                new PropertyMetadata(@default));
        }

        public static DependencyProperty Register<TOwner, TProperty>(string propertyName, PropertyChangedCallback propertyChanged)
        {
            return DependencyProperty.Register(propertyName, typeof(TProperty), typeof(TOwner),
                new PropertyMetadata(default(TProperty),propertyChanged));
        }

        public static DependencyProperty Register<TOwner, TProperty>(string propertyName, FrameworkPropertyMetadataOptions options)
        {
            return DependencyProperty.Register(propertyName, typeof(TProperty), typeof(TOwner),
                new FrameworkPropertyMetadata(default(TProperty),options));
        }

        public static DependencyProperty Register<TOwner, TProperty>(string propertyName, FrameworkPropertyMetadataOptions options, PropertyChangedCallback propertyChanged)
        {
            return DependencyProperty.Register(propertyName, typeof(TProperty), typeof(TOwner),
                new FrameworkPropertyMetadata(default(TProperty),options,propertyChanged));
        }

        public static DependencyProperty Register<TOwner, TProperty>(string propertyName, TProperty @default,
            PropertyChangedCallback propertyChanged)
        {
            return DependencyProperty.Register(propertyName, typeof(TProperty), typeof(TOwner),
                new PropertyMetadata(@default, propertyChanged));
        }

        public static DependencyProperty Register<TOwner, TProperty>(string propertyName, TProperty @default , FrameworkPropertyMetadataOptions options)
        {
            return DependencyProperty.Register(propertyName, typeof(TProperty), typeof(TOwner),
                new FrameworkPropertyMetadata(@default,options));
        }

        public static DependencyProperty Register<TOwner, TProperty>(string propertyName, TProperty @default , FrameworkPropertyMetadataOptions options , PropertyChangedCallback propertyChanged)
        {
            return DependencyProperty.Register(propertyName, typeof(TProperty), typeof(TOwner),
                new FrameworkPropertyMetadata(@default,options,propertyChanged));
        }

        public static DependencyPropertyKey RegisterReadOnly<TOwner,TProperty>(string propertyName)
        {
            return DependencyProperty.RegisterReadOnly(propertyName, typeof(TProperty), typeof(TOwner),
                new PropertyMetadata(default(TProperty)));
        }

        public static DependencyPropertyKey RegisterReadOnly<TOwner,TProperty>(string propertyName , TProperty @default)
        {
            return DependencyProperty.RegisterReadOnly(propertyName, typeof(TProperty), typeof(TOwner),
                new PropertyMetadata(@default));
        }
    }
}