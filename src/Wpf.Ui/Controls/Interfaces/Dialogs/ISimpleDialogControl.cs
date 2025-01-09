// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System;
using System.Threading.Tasks;
using System.Windows;

using Wpf.Ui.Common;

namespace Wpf.Ui.Controls.Interfaces.Dialogs;

/// <summary>
/// Represents a Dialog control.
/// </summary>
public interface ISimpleDialogControl
{
    public enum ButtonsVisibility
    {
        Close,
        HelpClose,
    }

    /// <summary>
    /// Gets the information whether the <see cref="ISimpleDialogControl"/> is visible.
    /// </summary>
    bool IsShown { get; }

    /// <summary>
    /// Gets or sets maximum dialog width.
    /// </summary>
    double DialogWidth { get; set; }

    /// <summary>
    /// Gets or sets maximum dialog height.
    /// </summary>
    double DialogHeight { get; set; }

    /// <summary>
    /// Gets or sets the title displayed at the top of the <see cref="ISimpleDialogControl"/>.
    /// </summary>
    string Title { get; set; }

    /// <summary>
    /// Gets or sets the message displayed inside the <see cref="ISimpleDialogControl"/>.
    /// </summary>
    string Message { get; set; }

    /// <summary>
    /// <see cref="FrameworkElement"/> or <see langword="string"/> displayed below the <see cref="Title"/> and <see cref="Message"/>.
    /// </summary>
    object Content { get; set; }

    /// <summary>
    /// Occurs when the dialog is about to open.
    /// </summary>
    public event RoutedDialogEvent Opened;

    /// <summary>
    /// Occurs when the dialog is about to close.
    /// </summary>
    public event RoutedDialogEvent Closed;

    /// <summary>
    /// Reveals the <see cref="ISimpleDialogControl"/>.
    /// </summary>
    /// <returns><see langword="true"/> if the operation was successful.</returns>
    bool Show();

    /// <summary>
    /// Reveals the <see cref="ISimpleDialogControl"/>.
    /// </summary>
    /// <param name="title"><see cref="Title"/> at the top of the dialog.</param>
    /// <param name="message"><see cref="Message"/> above the <see cref="Content"/> of the dialog.</param>
    /// <returns><see langword="true"/> if the operation was successful.</returns>
    bool Show(string title, string message);

    /// <summary>
    /// Reveals the <see cref="ISimpleDialogControl"/> and waits for the user to click on of the footer buttons.
    /// </summary>
    /// <returns>Information about which button was pressed.</returns>
    Task ShowAndWaitAsync();

    /// <summary>
    /// Reveals the <see cref="ISimpleDialogControl"/> and waits for the user to click on of the footer buttons.
    /// </summary>
    /// <param name="hideOnClick">Whether the dialogue should be hidden after pressing any button in the footer.</param>
    /// <returns>Information about which button was pressed.</returns>
    Task ShowAndWaitAsync(bool hideOnClick, Action? onCloseButtonClicked, Action? onHelpButtonClicked = null);

    /// <summary>
    /// Reveals the <see cref="ISimpleDialogControl"/> and waits for the user to click on of the footer buttons.
    /// </summary>
    /// <param name="title"><see cref="Title"/> at the top of the dialog.</param>
    /// <param name="message"><see cref="Message"/> above the <see cref="Content"/> of the dialog.</param>
    /// <returns>Information about which button was pressed.</returns>
    Task ShowAndWaitAsync(string title, string message);

    /// <summary>
    /// Reveals the <see cref="ISimpleDialogControl"/> and waits for the user to click on of the footer buttons.
    /// </summary>
    /// <param name="title"><see cref="Title"/> at the top of the dialog.</param>
    /// <param name="message"><see cref="Message"/> above the <see cref="Content"/> of the dialog.</param>
    /// <param name="hideOnClick">Whether the dialogue should be hidden after pressing any button in the footer.</param>
    /// <returns>Information about which button was pressed.</returns>
    Task ShowAndWaitAsync(string title, string message, bool hideOnClick);

    /// <summary>
    /// Hides the <see cref="ISimpleDialogControl"/>.
    /// <returns><see langword="true"/> if the operation was successful.</returns>
    /// </summary>
    bool Hide();
}
