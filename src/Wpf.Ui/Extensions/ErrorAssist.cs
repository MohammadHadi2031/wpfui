using System.Windows;

namespace Wpf.Ui.Extensions;

public static class ErrorAssist
{
    public static readonly DependencyProperty ErrorsProperty = DependencyProperty.RegisterAttached("Errors",
      typeof(string), typeof(ErrorAssist), new PropertyMetadata(""));

    public static readonly DependencyProperty ErrorsTemplateProperty = DependencyProperty.RegisterAttached("ErrorsTemplate",
        typeof(DataTemplate), typeof(ErrorAssist), new PropertyMetadata(null));

    public static void SetErrors(DependencyObject obj, string value) => obj.SetValue(ErrorsProperty, value);
    public static string GetErrors(DependencyObject obj) => (string)obj.GetValue(ErrorsProperty);

    public static void SetErrorsTemplate(DependencyObject obj, DataTemplate value) => obj.SetValue(ErrorsTemplateProperty, value);
    public static DataTemplate GetErrorsTemplate(DependencyObject obj) => (DataTemplate)obj.GetValue(ErrorsTemplateProperty);
}
