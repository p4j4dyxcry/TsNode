using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace TsControls.AttachBehaviors
{
    /// <summary>
    /// .net internal class を外部から無理やり扱うためのクラス
    /// </summary>
    internal class InternalTextEditorHacker
    {
        private static Type InternalTextEditorType { get; }
        private static PropertyInfo IsReadOnlyProperty { get; }
        private static PropertyInfo TextViewProperty { get; }
        private static MethodInfo RegisterMethod { get; }
        private static PropertyInfo TextContainerTextViewProperty { get; }
        private static PropertyInfo TextContainerProperty { get; }

        // ! TextBlock
        public static void Bind(TextBlock tb)
        {
            var textContainer = TextContainerProperty.GetValue(tb);

            var editor = new InternalTextEditorHacker(textContainer, tb, false);
            IsReadOnlyProperty.SetValue(editor._internalTextEditor, true);
            TextViewProperty.SetValue(editor._internalTextEditor, TextContainerTextViewProperty.GetValue(textContainer));
        }

        public static void RegisterCommandHandlers(Type controlType, bool acceptsRichContent, bool readOnly, bool registerEventListeners)
        {
            RegisterMethod.Invoke(null, new object[] { controlType, acceptsRichContent, readOnly, registerEventListeners });
        }

        private readonly object _internalTextEditor;

        static InternalTextEditorHacker()
        {
            // PresentationFramework.dllを取得する
            var presentationFrameworkAssembly = Assembly.GetAssembly(typeof(TextBlock));

            //! 選択処理を行うために必要な非公開APIをリフレクションで取得する
            InternalTextEditorType = presentationFrameworkAssembly.GetType("System.Windows.Documents.TextEditor");
            IsReadOnlyProperty = InternalTextEditorType.GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
            TextViewProperty = InternalTextEditorType.GetProperty("TextView", BindingFlags.Instance | BindingFlags.NonPublic);
            RegisterMethod = InternalTextEditorType.GetMethod("RegisterCommandHandlers",
                BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(Type), typeof(bool), typeof(bool), typeof(bool) }, null);
            var textContainerType = presentationFrameworkAssembly.GetType("System.Windows.Documents.ITextContainer");
            TextContainerTextViewProperty = textContainerType.GetProperty("TextView");
            TextContainerProperty = typeof(TextBlock).GetProperty("TextContainer", BindingFlags.Instance | BindingFlags.NonPublic);

        }

        public InternalTextEditorHacker(object textContainer, FrameworkElement uiScope, bool isUndoEnabled)
        {
            _internalTextEditor = Activator.CreateInstance(InternalTextEditorType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance,
                null, new[] { textContainer, uiScope, isUndoEnabled }, null);
        }
    }

    /// <summary>
    /// TextBlockを選択可能にし、コピーができるようにする添付ビヘイビア
    /// </summary>
    public static class TextBlockSelectableService
    {
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.RegisterAttached(
            "IsSelectable",
            typeof(bool),
            typeof(TextBlockSelectableService),
            new FrameworkPropertyMetadata(false, IsSelectableChanged));

        public static bool GetIsSelectable(DependencyObject d)
        {
            return (bool)d.GetValue(WatermarkProperty);
        }

        public static void SetIsSelectable(DependencyObject d, bool value)
        {
            d.SetValue(WatermarkProperty, value);
        }

        static TextBlockSelectableService()
        {
            InternalTextEditorHacker.RegisterCommandHandlers(typeof(TextBlock), true, true, true);
        }

        private static void IsSelectableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock)
            {
                textBlock.SetValue(UIElement.FocusableProperty, true);
                textBlock.SetValue(FrameworkElement.FocusVisualStyleProperty, (object)null);
                InternalTextEditorHacker.Bind(textBlock);
            }
        }
    }
}