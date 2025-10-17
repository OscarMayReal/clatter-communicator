using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace ClatterCommunicator;

public partial class MainWindow : Window
{
    public MainWindow()
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
        this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }
    
    private void LoginView_OnOnLogin(object? sender, LoginView.LoggedInSessionEventArgs e)
    {
        this.LoginView.IsVisible = false;
        this.MainView.IsVisible = true;
    }
}