using System;
using Avalonia.Controls;
using Avalonia.Input;

namespace ClatterCommunicator;

public partial class ChatWindow : Window
{
    public ChatWindow()
    {
        InitializeComponent();
    }

    private void TitleBarDraggable_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        this.BeginMoveDrag(e);
    }

    private void TitleBarXButton_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        Close();
    }

    private void TitleBarMinimizeButton_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void TitleBarResizeButton_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        this.WindowState = this.WindowState == WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;
    }
}