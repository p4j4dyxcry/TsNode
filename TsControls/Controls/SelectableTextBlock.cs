using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace TsControls.Controls
{
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

    public class SelectableTextBlock : TextBlock
    {
        static SelectableTextBlock()
        {
            FocusableProperty.OverrideMetadata(typeof(SelectableTextBlock), new FrameworkPropertyMetadata(true));
            InternalTextEditorHacker.RegisterCommandHandlers(typeof(SelectableTextBlock), true, true, true);

            FocusVisualStyleProperty.OverrideMetadata(typeof(SelectableTextBlock), new FrameworkPropertyMetadata((object)null));
        }

        public SelectableTextBlock()
        {
            InternalTextEditorHacker.Bind(this);
        }
    }
}
