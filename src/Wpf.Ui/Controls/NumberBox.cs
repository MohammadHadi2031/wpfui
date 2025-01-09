// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wpf.Ui.Controls;

// TODO: Handle backspace
// TODO: Handle editing
// TODO: Implement mask

/// <summary>
/// Text field for entering numbers with the possibility of specifying a pattern.
/// </summary>
[ToolboxItem(true)]
[ToolboxBitmap(typeof(NumberBox), "NumberBox.bmp")]
public class NumberBox : Wpf.Ui.Controls.TextBox
{
    // In both expressions, we allow the lonely characters '-', '.' and ',' so the numbers can be typed in real-time.

    /// <summary>
    /// Accepts a string of digits separated by a comma or period. Allows for a leading minus sign.
    /// </summary>
    private readonly string _decimalExpression = /* language=regex */ @"^\-?(\d+(?:[\.\,]|[\.\,]\d+)?)?$";

    /// <summary>
    /// Accepts a string of digits only. Allows for a leading minus sign.
    /// </summary>
    private readonly string _integerExpression = /* language=regex */ @"^\-?(\d+)*$";

    /// <summary>
    /// Property for <see cref="Value"/>.
    /// </summary>
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value),
        typeof(decimal), typeof(NumberBox), new FrameworkPropertyMetadata(0M, ValuePropertyChangedCallback)
        {
            BindsTwoWayByDefault = true,
        });

    /// <summary>
    /// Property for <see cref="Step"/>.
    /// </summary>
    public static readonly DependencyProperty StepProperty = DependencyProperty.Register(nameof(Step),
        typeof(decimal), typeof(NumberBox), new PropertyMetadata(1M));

    /// <summary>
    /// Property for <see cref="Max"/>.
    /// </summary>
    public static readonly DependencyProperty MaxProperty = DependencyProperty.Register(nameof(Max),
        typeof(decimal), typeof(NumberBox), new PropertyMetadata(decimal.MaxValue));

    /// <summary>
    /// Property for <see cref="Min"/>.
    /// </summary>
    public static readonly DependencyProperty MinProperty = DependencyProperty.Register(nameof(Min),
        typeof(decimal), typeof(NumberBox), new PropertyMetadata(decimal.MinValue));

    /// <summary>
    /// Property for <see cref="DecimalPlaces"/>.
    /// </summary>
    public static readonly DependencyProperty DecimalPlacesProperty = DependencyProperty.Register(nameof(DecimalPlaces),
        typeof(int), typeof(NumberBox), new PropertyMetadata(2, OnDecimalPlacesChanged));

    /// <summary>
    /// Property for <see cref="Mask"/>.
    /// </summary>
    public static readonly DependencyProperty MaskProperty = DependencyProperty.Register(nameof(Mask),
        typeof(string), typeof(NumberBox), new PropertyMetadata(String.Empty));

    /// <summary>
    /// Property for <see cref="SpinButtonsEnabled"/>.
    /// </summary>
    public static readonly DependencyProperty SpinButtonsEnabledProperty = DependencyProperty.Register(nameof(SpinButtonsEnabled),
        typeof(bool), typeof(NumberBox), new PropertyMetadata(true));

    /// <summary>
    /// Property for <see cref="IntegersOnly"/>.
    /// </summary>
    public static readonly DependencyProperty IntegersOnlyProperty = DependencyProperty.Register(nameof(IntegersOnly),
        typeof(bool), typeof(NumberBox), new PropertyMetadata(false));

    /// <summary>
    /// Routed event for <see cref="Incremented"/>.
    /// </summary>
    public static readonly RoutedEvent IncrementedEvent = EventManager.RegisterRoutedEvent(
        nameof(Incremented), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NumberBox));

    /// <summary>
    /// Routed event for <see cref="Decremented"/>.
    /// </summary>
    public static readonly RoutedEvent DecrementedEvent = EventManager.RegisterRoutedEvent(
        nameof(Decremented), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NumberBox));

    public static readonly DependencyProperty HasOutOfRangeErrorProperty = DependencyProperty.Register(nameof(HasOutOfRangeError),
        typeof(bool), typeof(NumberBox), new PropertyMetadata(false));

    public static readonly DependencyProperty OutOfRangeErrorTemplateProperty = DependencyProperty.Register(nameof(OutOfRangeErrorTemplate),
        typeof(DataTemplate), typeof(NumberBox), new PropertyMetadata(null));

    private string _cachedText = "";
    private bool _isUpdatingTextByCode;
    private decimal _lastInRangeValue = 0M;
    private bool _isUpdatingWithInRangeValue;

    private static void ValuePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NumberBox numberBox)
        {
            return;
        }

        numberBox.ValuePropertyChangedCallback(e);
    }

    private void ValuePropertyChangedCallback(DependencyPropertyChangedEventArgs e)
    {
        var value = (decimal)e.NewValue;

        if (value > Max || value < Min)
        {
            value = _lastInRangeValue;
        }
        else
        {
            _lastInRangeValue = value;
            _cachedText = FormatValueToString(_lastInRangeValue);
        }

        var text = FormatValueToString(value);

        _isUpdatingTextByCode = true;
        Text = text;
        _isUpdatingTextByCode = false;
    }

    /// <summary>
    /// <see cref="NumberBox"/> does no accept returns.
    /// </summary>
    public new bool AcceptsReturn
    {
        get => false;
        set => throw new NotImplementedException($"{typeof(NumberBox)} does not accept returns.");
    }

    /// <summary>
    /// <see cref="NumberBox"/> does not accept changes to the number of lines.
    /// </summary>
    public new int MaxLines
    {
        get => 1;
        set => throw new NotImplementedException($"{typeof(NumberBox)} does not accept changes to the number of lines.");
    }

    /// <summary>
    /// <see cref="NumberBox"/> does not accept changes to the number of lines.
    /// </summary>
    public new int MinLines
    {
        get => 1;
        set => throw new NotImplementedException($"{typeof(NumberBox)} does not accept changes to the number of lines.");
    }

    /// <summary>
    /// Gets or sets current numeric value.
    /// </summary>
    public decimal Value
    {
        get => (decimal)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Gets or sets value by which the given number will be increased or decreased after pressing the button.
    /// </summary>
    public decimal Step
    {
        get => (decimal)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    /// <summary>
    /// Maximum allowable value.
    /// </summary>
    public decimal Max
    {
        get => (decimal)GetValue(MaxProperty);
        set => SetValue(MaxProperty, value);
    }

    /// <summary>
    /// Minimum allowable value.
    /// </summary>
    public decimal Min
    {
        get => (decimal)GetValue(MinProperty);
        set => SetValue(MinProperty, value);
    }

    /// <summary>
    /// Number of decimal places.
    /// </summary>
    public int DecimalPlaces
    {
        get => (int)GetValue(DecimalPlacesProperty);
        set => SetValue(DecimalPlacesProperty, value);
    }

    /// <summary>
    /// Gets or sets numbers pattern.
    /// </summary>
    public string Mask
    {
        get => (string)GetValue(MaskProperty);
        set => SetValue(MaskProperty, value);
    }

    /// <summary>
    /// Gets or sets value determining whether to display the button controls.
    /// </summary>
    public bool SpinButtonsEnabled
    {
        get => (bool)GetValue(SpinButtonsEnabledProperty);
        set => SetValue(SpinButtonsEnabledProperty, value);
    }

    public bool HasOutOfRangeError
    {
        get { return (bool)GetValue(HasOutOfRangeErrorProperty); }
        private set { SetValue(HasOutOfRangeErrorProperty, value); }
    }

    public DataTemplate OutOfRangeErrorTemplate
    {
        get { return (DataTemplate)GetValue(OutOfRangeErrorTemplateProperty); }
        set { SetValue(OutOfRangeErrorTemplateProperty, value); }
    }

    /// <summary>
    /// Gets or sets value which determines whether only integers can be entered.
    /// </summary>
    public bool IntegersOnly
    {
        get => (bool)GetValue(IntegersOnlyProperty);
        set => SetValue(IntegersOnlyProperty, value);
    }

    /// <summary>
    /// Event occurs when a value is incremented by button or arrow key.
    /// </summary>
    public event RoutedEventHandler Incremented
    {
        add => AddHandler(IncrementedEvent, value);
        remove => RemoveHandler(IncrementedEvent, value);
    }

    /// <summary>
    /// Event occurs when a value is decremented by button or arrow key.
    /// </summary>
    public event RoutedEventHandler Decremented
    {
        add => AddHandler(DecrementedEvent, value);
        remove => RemoveHandler(DecrementedEvent, value);
    }

    static NumberBox()
    {
        AcceptsReturnProperty.OverrideMetadata(typeof(PasswordBox), new FrameworkPropertyMetadata(false));
        MaxLinesProperty.OverrideMetadata(typeof(PasswordBox), new FrameworkPropertyMetadata(1));
        MinLinesProperty.OverrideMetadata(typeof(PasswordBox), new FrameworkPropertyMetadata(1));
    }

    /// <summary>
    /// Creates new instance of <see cref="NumberBox"/>.
    /// </summary>
    public NumberBox()
    {
        DataObject.AddPastingHandler(this, OnClipboardPaste);

        Loaded += OnLoaded;
    }

    protected virtual void OnValueChanged()
    {

    }

    /// <inheritdoc/>
    protected override void OnTemplateButtonClick(object sender, object parameter)
    {
        base.OnTemplateButtonClick(sender, parameter);

        if (sender is not NumberBox)
            return;

        if (parameter is not string parameterString)
            return;

        switch (parameterString)
        {
            case "increment":
                IncrementValue();
                break;

            case "decrement":
                DecrementValue();
                break;
        }
    }

    /// <summary>
    /// Updates <see cref="Value"/> and <see cref="System.Windows.Controls.TextBox.Text"/>.
    /// </summary>
    private void UpdateValue(decimal value, bool updateText)
    {
        Value = value;

        if (!updateText)
            return;

        var newText = FormatValueToString(value);

        Text = newText;
        CaretIndex = newText.Length;
    }

    /// <summary>
    /// Updates <see cref="Value"/> and <see cref="System.Windows.Controls.TextBox.Text"/>.
    /// </summary>
    private void UpdateValue(decimal value, string updateText)
    {
        Value = value;

        Text = updateText;
        CaretIndex = updateText.Length;
    }

    /// <summary>
    /// Increments current <see cref="Value"/>.
    /// </summary>
    private void IncrementValue()
    {
        var currentText = Text?.Trim();

        if (string.IsNullOrEmpty(currentText))
        {
            UpdateValue(Min, true);
            return;
        }

        if (!TryParseStringToDecimal(currentText, out var parsedNumber))
        {
            return;
        }

        parsedNumber += Step;

        if (string.IsNullOrWhiteSpace(currentText) || parsedNumber > Max)
        {
            UpdateValue(Max, true);

            return;
        }

        if (parsedNumber < Min)
        {
            UpdateValue(Min, true);
            return;
        }

        UpdateValue(parsedNumber, true);

        OnIncremented();
    }

    /// <summary>
    /// Decrements current <see cref="Value"/>.
    /// </summary>
    private void DecrementValue()
    {
        var currentText = Text;

        if (string.IsNullOrEmpty(currentText))
        {
            UpdateValue(Min, true);
            return;
        }

        if (!TryParseStringToDecimal(currentText, out var parsedNumber))
        {
            return;
        }

        parsedNumber -= Step;

        if (string.IsNullOrWhiteSpace(currentText) || parsedNumber < Min)
        {
            UpdateValue(Min, true);
            return;
        }

        if (parsedNumber > Max)
        {
            UpdateValue(Max, true);
            return;
        }


        UpdateValue(parsedNumber, true);

        OnDecremented();
    }

    /// <summary>
    /// Formats decimal number according to configuration.
    /// </summary>
    private string FormatValueToString(decimal number)
    {
        if (IntegersOnly || DecimalPlaces < 1)
            return number.ToString("F0", CultureInfo.InvariantCulture);

        if (DecimalPlaces < 5)
            return number.ToString($"F{DecimalPlaces}", CultureInfo.InvariantCulture);

        return number.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Tests provided text with regular expression according to configuration.
    /// </summary>
    private bool IsNumberTextValid(string inputText)
    {
        // If the mask is used this method will not work

        var decimalPlaces = DecimalPlaces;
        var integerRegex = new Regex(_integerExpression);
        var decimalRegex = new Regex(_decimalExpression);

        if (IntegersOnly || decimalPlaces < 1)
            return integerRegex.IsMatch(inputText);

        if (!decimalRegex.IsMatch(inputText))
            return false;

        return true;
    }

    private int GetDecimalPlaces(string inputText)
    {
        if (inputText.Contains(","))
            return inputText.Substring(inputText.IndexOf(",") + 1).Length;

        if (inputText.Contains("."))
            return inputText.Substring(inputText.IndexOf(",") + 1).Length;

        return 0;
    }


    /// <summary>
    /// Tries to format provided string according to the mask.
    /// </summary>
    private string FormatWithMask(string currentInput, string newInput)
    {
        // TODO: Format text according to MaskProperty

        return currentInput;
    }

    /// <summary>
    /// Tries to parse provided string to decimal with invariant culture.
    /// </summary>
    private bool TryParseStringToDecimal(string inputText, out decimal number)
    {
        return decimal.TryParse(inputText, NumberStyles.Any, CultureInfo.InvariantCulture, out number);
    }

    /// <summary>
    /// Occurs when controls is loaded.
    /// </summary>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _isUpdatingTextByCode = true;
        Text = FormatValueToString(Value);
        _isUpdatingTextByCode = false;
    }

    /// <inheritdoc />
    protected override void OnKeyUp(KeyEventArgs e)
    {
        //if (e.Key == Key.Up)
        //{
        //    IncrementValue();

        //    e.Handled = true;
        //}

        //if (e.Key == Key.Down)
        //{
        //    DecrementValue();

        //    e.Handled = true;
        //}

        base.OnKeyUp(e);
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Up)
        {
            IncrementValue();

            e.Handled = true;
        }

        if (e.Key == Key.Down)
        {
            DecrementValue();

            e.Handled = true;
        }

        if (e.Key == Key.Enter)
        {
            UpdateText();
            e.Handled = true;
        }

        base.OnPreviewKeyDown(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
    }

    /// <inheritdoc />
    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        base.OnTextChanged(e);

        if (!_isUpdatingTextByCode)
        {
            return;
        }

        //var currentText = Text;
        //if (!TryParseStringTodecimal(currentText, out var parsedNumber))
        //{
        //    return;
        //}

        //if (parsedNumber > Max)
        //{
        //    UpdateValue(Max, true);
        //    return;
        //}

        //if (parsedNumber < Min)
        //{
        //    UpdateValue(Min, true);
        //    return;
        //}

        //UpdateValue(parsedNumber, true);
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        UpdateText();
    }

    private void UpdateText()
    {
        var currentText = Text?.Trim();

        if (!string.IsNullOrEmpty(currentText) &&
                    !IsNumberTextValid(currentText))
        {
            Text = _cachedText;
            return;
        }

        if (!string.IsNullOrEmpty(currentText) &&
            Min >= 0 && currentText.StartsWith("-"))
        {
            Text = _cachedText;
            return;
        }

        if (string.IsNullOrEmpty(currentText))
        {
            UpdateValue(Min, true);
            return;
        }

        if (!TryParseStringToDecimal(currentText, out var parsedNumber))
        {
            return;
        }

        if (parsedNumber > Max)
        {
            HasOutOfRangeError = true;

            _isUpdatingWithInRangeValue = true;
            UpdateValue(_lastInRangeValue, true);
            _isUpdatingWithInRangeValue = false;
            return;
        }

        if (parsedNumber < Min)
        {
            HasOutOfRangeError = true;

            _isUpdatingWithInRangeValue = true;
            UpdateValue(_lastInRangeValue, true);
            _isUpdatingWithInRangeValue = false;
            return;
        }

        parsedNumber = Math.Round(parsedNumber, DecimalPlaces);
        var newText = FormatValueToString(parsedNumber);
        Text = newText;

        PlaceholderEnabled = currentText.Length < 1;

        if (!_isUpdatingWithInRangeValue)
        {
            HasOutOfRangeError = false;
        }

        Value = parsedNumber;
    }

    /// <inheritdoc />
    protected override void OnPreviewTextInput(TextCompositionEventArgs e)
    {
        //var newText = Text + (e.Text ?? String.Empty);

        //if (!String.IsNullOrEmpty(newText))
        //    e.Handled = !IsNumberTextValid(newText);

        //// Do not allow a leading minus sign if the min value is greater than zero.
        //if (Min >= 0 && newText.StartsWith("-"))
        //    e.Handled = true;


        base.OnPreviewTextInput(e);
    }

    /// <summary>
    /// This virtual method is called after incrementing a value using button or arrow key.
    /// </summary>
    protected virtual void OnIncremented()
    {
        RaiseEvent(new RoutedEventArgs(IncrementedEvent, this));
    }

    /// <summary>
    /// This virtual method is called after decrementing a value using button or arrow key.
    /// </summary>
    protected virtual void OnDecremented()
    {
        RaiseEvent(new RoutedEventArgs(DecrementedEvent, this));
    }

    /// <summary>
    /// This virtual method is called after <see cref="DecimalPlaces"/> is changed.
    /// </summary>
    protected virtual void OnDecimalPlacesChanged(int decimalPlaces)
    {
        if (decimalPlaces < 0)
            DecimalPlaces = 0;
    }



    private void OnClipboardPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (sender is not NumberBox control)
            return;

        var clipboardText = (string)e.DataObject.GetData(typeof(string));

        if (!IsNumberTextValid(clipboardText))
            e.CancelCommand();
    }

    private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NumberBox numberBox)
            return;

        numberBox.OnValueChanged();
    }

    private static void OnDecimalPlacesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NumberBox control)
            return;

        if (e.NewValue is not int newValue)
            return;

        control.OnDecimalPlacesChanged(newValue);
    }
}
