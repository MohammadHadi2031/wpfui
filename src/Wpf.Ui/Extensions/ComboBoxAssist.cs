using System.Windows;

namespace Wpf.Ui.Extensions
{
    public static class ComboBoxAssist
    {
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.RegisterAttached("Placeholder", typeof(string), typeof(ComboBoxAssist), new PropertyMetadata(""));

        public static void SetPlaceholder(DependencyObject obj, string value) => obj.SetValue(PlaceholderProperty, value);

        public static string GetPlaceholder(DependencyObject obj) => (string)obj.GetValue(PlaceholderProperty);
    }
}
