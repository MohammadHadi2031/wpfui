using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Wpf.Ui.Common;
using Wpf.Ui.Controls.Interfaces.Dialogs;
using static Wpf.Ui.Controls.Interfaces.Dialogs.ISimpleDialogControl;

namespace Wpf.Ui.Controls;

[ToolboxItem(true)]
public class SimpleDialog : ContentControl, ISimpleDialogControl
{
    private TaskCompletionSource? _tcs = null;

    private bool _automaticHide;
    private Action? _onCloseButtonClicked;
    private Action? _onHelpButtonClicked;

    private Button? _closeButton;
    private Button? _helpButton;

    /// <summary>
    /// Template element represented by the <c>helpButton</c> name.
    /// </summary>
    private const string ElementHelpButton = "helpButton";

    /// <summary>
    /// Template element represented by the <c>closeButton</c> name.
    /// </summary>
    private const string ElementCloseButton = "closeButton";


    #region Static properties

    /// <summary>
    /// Property for <see cref="IsShown"/>.
    /// </summary>
    public static readonly DependencyProperty IsShownProperty = DependencyProperty.Register(nameof(IsShown),
        typeof(bool), typeof(SimpleDialog), new PropertyMetadata(false, OnIsShownChange));

    /// <summary>
    /// Property for <see cref="Title"/>.
    /// </summary>
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title),
        typeof(string), typeof(SimpleDialog), new PropertyMetadata(String.Empty));

    /// <summary>
    /// Property for <see cref="Message"/>.
    /// </summary>
    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(nameof(Message),
        typeof(string), typeof(SimpleDialog), new PropertyMetadata(String.Empty));

    /// <summary>
    /// Property for <see cref="DialogWidth"/>.
    /// </summary>
    public static readonly DependencyProperty DialogWidthProperty =
        DependencyProperty.Register(nameof(DialogWidth),
            typeof(double), typeof(SimpleDialog), new PropertyMetadata(double.NaN));

    /// <summary>
    /// Property for <see cref="DialogHeight"/>.
    /// </summary>
    public static readonly DependencyProperty DialogHeightProperty =
        DependencyProperty.Register(nameof(DialogHeight),
            typeof(double), typeof(SimpleDialog), new PropertyMetadata(double.NaN));

    /// <summary>
    /// Property for <see cref="DialogMaxWidth"/>.
    /// </summary>
    public static readonly DependencyProperty DialogMaxWidthProperty =
        DependencyProperty.Register(nameof(DialogMaxWidth),
            typeof(double), typeof(SimpleDialog), new PropertyMetadata(double.PositiveInfinity));

    /// <summary>
    /// Property for <see cref="DialogMaxHeight"/>.
    /// </summary>
    public static readonly DependencyProperty DialogMaxHeightProperty =
        DependencyProperty.Register(nameof(DialogMaxHeight),
            typeof(double), typeof(SimpleDialog), new PropertyMetadata(double.PositiveInfinity));



   

    public static readonly DependencyProperty IsCloseButtonEnabledProperty =
        DependencyProperty.Register(nameof(IsCloseButtonEnabled), typeof(bool), typeof(SimpleDialog), new PropertyMetadata(true));

    /// <summary>
    /// Property for <see cref="CloseButtonIconData"/>.
    /// </summary>
    public static readonly DependencyProperty CloseButtonIconDataProperty =
        DependencyProperty.Register(nameof(CloseButtonIconData),
            typeof(string), typeof(SimpleDialog));

    /// <summary>
    /// Property for <see cref="CloseButtonCommand"/>.
    /// </summary>
    public static readonly DependencyProperty CloseButtonCommandProperty =
        DependencyProperty.Register(nameof(CloseButtonCommand),
            typeof(Common.IRelayCommand), typeof(Dialog), new PropertyMetadata(null));

    /// <summary>
    /// Property for <see cref="HelpButtonIconData"/>.
    /// </summary>
    public static readonly DependencyProperty HelpButtonIconDataProperty =
        DependencyProperty.Register(nameof(HelpButtonIconData),
            typeof(string), typeof(SimpleDialog));

    /// <summary>
    /// Property for <see cref="HelpButtonCommand"/>.
    /// </summary>
    public static readonly DependencyProperty HelpButtonCommandProperty =
        DependencyProperty.Register(nameof(HelpButtonCommand),
            typeof(IRelayCommand), typeof(Dialog), new PropertyMetadata(null));

    public static readonly DependencyProperty ButtonsVisibilityProperty =
        DependencyProperty.Register(nameof(ButtonsVisibility),
            typeof(ButtonsVisibility), typeof(SimpleDialog), new PropertyMetadata(ButtonsVisibility.Close, OnButtonsVisibilityChanged));

    #endregion Static properties

    /// <inheritdoc /> 
    public bool IsShown
    {
        get => (bool)GetValue(IsShownProperty);
        protected set => SetValue(IsShownProperty, value);
    }

    /// <inheritdoc />
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <inheritdoc />
    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <inheritdoc />
    public double DialogWidth
    {
        get => (double)GetValue(DialogWidthProperty);
        set => SetValue(DialogWidthProperty, value);
    }

    /// <inheritdoc />
    public double DialogHeight
    {
        get => (double)GetValue(DialogHeightProperty);
        set => SetValue(DialogHeightProperty, value);
    }

    /// <inheritdoc />
    public double DialogMaxWidth
    {
        get => (double)GetValue(DialogMaxWidthProperty);
        set => SetValue(DialogMaxWidthProperty, value);
    }

    /// <inheritdoc />
    public double DialogMaxHeight
    {
        get => (double)GetValue(DialogMaxHeightProperty);
        set => SetValue(DialogMaxHeightProperty, value);
    }

    public ButtonsVisibility ButtonsVisibility
    {
        get { return (ButtonsVisibility)GetValue(ButtonsVisibilityProperty); }
        set { SetValue(ButtonsVisibilityProperty, value); }
    }

    public bool IsCloseButtonEnabled
    {
        get { return (bool)GetValue(IsCloseButtonEnabledProperty); }
        set { SetValue(IsCloseButtonEnabledProperty, value); }
    }

    /// <inheritdoc />
    public string CloseButtonIconData
    {
        get => (string)GetValue(CloseButtonIconDataProperty);
        init => SetValue(CloseButtonIconDataProperty, value);
    }

    /// <inheritdoc />
    public string HelpButtonIconData
    {
        get => (string)GetValue(HelpButtonIconDataProperty);
        init => SetValue(HelpButtonIconDataProperty, value);
    }

    public string HelpButtonFilePath { get; set; }

    /// <summary>
    /// Command triggered after clicking the close button in the template.
    /// </summary>
    public Common.IRelayCommand CloseButtonCommand =>
        (Common.IRelayCommand)GetValue(CloseButtonCommandProperty);

    /// <summary>
    /// Command triggered after clicking the help button in the template.
    /// </summary>
    public Common.IRelayCommand HelpButtonCommand =>
        (Common.IRelayCommand)GetValue(HelpButtonCommandProperty);


    /// <summary>
    /// Event triggered when <see cref="SimpleDialog"/> opens.
    /// </summary>
    public static readonly RoutedEvent OpenedEvent = EventManager.RegisterRoutedEvent(nameof(Opened),
        RoutingStrategy.Bubble, typeof(RoutedDialogEvent), typeof(SimpleDialog));

    /// <inheritdoc />
    public event RoutedDialogEvent Opened
    {
        add => AddHandler(OpenedEvent, value);
        remove => RemoveHandler(OpenedEvent, value);
    }

    /// <summary>
    /// Event triggered when <see cref="SimpleDialog"/> opens.
    /// </summary>
    public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent(nameof(Closed),
        RoutingStrategy.Bubble, typeof(RoutedDialogEvent), typeof(SimpleDialog));

    /// <inheritdoc />
    public event RoutedDialogEvent Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    /// <summary>
    /// Creates new instance and sets default
    /// <see cref="CloseButtonCommandProperty"/>
    /// <see cref="HelpButtonCommandProperty"/>.
    /// </summary>
    public SimpleDialog()
    {
        SetValue(CloseButtonCommandProperty, new RelayCommand(_ => OnCloseButtonClick(this)));
        SetValue(HelpButtonCommandProperty, new RelayCommand(_ => OnHelpButtonClick(this)));

        CloseButtonIconData = Application.Current.Resources["CROSS_ICON"] as string ?? "";
        HelpButtonIconData = Application.Current.Resources["QUESTION_ICON"] as string ?? "";

        this.Focusable = true;
        this.KeyDown += (s, e) => { Dialog_PreviewKeyDown(s, e); };
    }


    private void Dialog_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            TrySetResult();
        }
    }

    private static void OnButtonsVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not SimpleDialog smipleDialog)
        {
            return;
        }

        smipleDialog.OnButtonsVisibilityChanged();
    }

    private void OnButtonsVisibilityChanged()
    {
        if (_closeButton is null)
        {
            throw new InvalidOperationException();
        }

        if (_helpButton is null)
        {
            throw new InvalidOperationException();
        }


        switch (ButtonsVisibility)
        {
            case ButtonsVisibility.Close:
                _closeButton.Visibility = Visibility.Visible;
                _helpButton.Visibility = Visibility.Collapsed;
                break;
            case ButtonsVisibility.HelpClose:
                _closeButton.Visibility = Visibility.Visible;
                _helpButton.Visibility = Visibility.Visible;
                break;
            default:
                throw new NotImplementedException();
        }
    }

    /// <inheritdoc />
    public Task ShowAndWaitAsync()
    {
        _automaticHide = false;

        Show();

        _tcs = new TaskCompletionSource();

        return _tcs.Task;
    }

    /// <inheritdoc />
    public Task ShowAndWaitAsync(bool hideOnClick, Action? onCloseButtonClicked, Action? onHelpButtonClicked = null)
    {
        _automaticHide = hideOnClick;
        _onCloseButtonClicked = onCloseButtonClicked;
        _onHelpButtonClicked = onHelpButtonClicked;

        Show();

        _tcs = new TaskCompletionSource();

        return _tcs.Task;
    }

    /// <inheritdoc />
    public Task ShowAndWaitAsync(string title, string message)
    {
        _automaticHide = false;

        if (IsShown)
            Hide();

        Show(title, message);

        _tcs = new TaskCompletionSource();

        return _tcs.Task;
    }

    /// <inheritdoc />
    public Task ShowAndWaitAsync(string title, string message, bool hideOnClick)
    {
        _automaticHide = hideOnClick;

        if (IsShown)
            Hide();

        Show(title, message);

        _tcs = new TaskCompletionSource();

        return _tcs.Task;
    }

    /// <inheritdoc />
    public bool Show()
    {
        if (IsShown)
            return false;

        _automaticHide = false;

        IsShown = true;

        return true;
    }

    /// <inheritdoc />
    public bool Show(string title, string message)
    {
        if (IsShown)
            Hide();

        _automaticHide = false;

        Title = title;
        Message = message;
        IsShown = true;

        return true;
    }

    /// <inheritdoc />
    public bool Hide()
    {
        if (!IsShown)
            return false;

        IsShown = false;

        return true;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild(ElementCloseButton) is Button closeButton)
            _closeButton = closeButton;

        if (GetTemplateChild(ElementHelpButton) is Button helpButton)
            _helpButton = helpButton;

        //_grid = (Grid)GetTemplateChild("FooterButtonsGrid");
        //_middleFooterButton = (Button)GetTemplateChild("PART_FooterButtonMiddle");

    }

    /// <summary>
    /// This virtual method is called when <see cref="SimpleDialog"/> is opening and it raises the <see cref="Opened"/> <see langword="event"/>.
    /// </summary>
    protected virtual void OnOpened()
    {
        var newEvent = new RoutedEventArgs(SimpleDialog.OpenedEvent, this);
        RaiseEvent(newEvent);
        this.Focus();
    }

    /// <summary>
    /// This virtual method is called when <see cref="SimpleDialog"/> is closing and it raises the <see cref="Closed"/> <see langword="event"/>.
    /// </summary>
    protected virtual void OnClosed()
    {
        var newEvent = new RoutedEventArgs(SimpleDialog.ClosedEvent, this);
        RaiseEvent(newEvent);
    }

    /// <summary>
    /// Triggered by clicking a button in the control template.
    /// </summary>
    /// <param name="sender">Sender of the click event.</param>
    /// <param name="parameter">Additional parameters.</param>
    protected virtual void OnCloseButtonClick(object sender)
    {
        _onCloseButtonClicked?.Invoke();
        TrySetResult();

        if (_automaticHide)
            Hide();
    }

    public void TrySetResult() => _tcs?.TrySetResult();

    /// <summary>
    /// Triggered by clicking a button in the control template.
    /// </summary>
    /// <param name="sender">Sender of the click event.</param>
    /// <param name="parameter">Additional parameters.</param>
    protected virtual void OnHelpButtonClick(object sender)
    {
        if (_onHelpButtonClicked is { })
        {
            _onHelpButtonClicked.Invoke();
            return;
        }

        if (File.Exists(HelpButtonFilePath))
        {
            Process.Start(new ProcessStartInfo(HelpButtonFilePath) { UseShellExecute = true });
        }
        else
        {
            throw new FileNotFoundException();
        }
    }


    private static void OnIsShownChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not SimpleDialog control)
            return;

        if (control.IsShown)
        {
            control.OnButtonsVisibilityChanged();
            control.OnOpened();
        }
        else
        {
            control.OnClosed();
        }
    }

    public void Reset()
    {
        Title = "";
        Message = "";
        Content = null;
        DialogWidth = double.NaN;
        DialogHeight = double.NaN;
        DialogMaxWidth = double.PositiveInfinity;
        DialogMaxHeight = double.PositiveInfinity;
        _onCloseButtonClicked = null;
        _onHelpButtonClicked = null;
    }
}
