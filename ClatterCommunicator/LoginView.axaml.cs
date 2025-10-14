using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RouteNav.Avalonia;

namespace ClatterCommunicator;

public partial class LoginView : UserControl
{
    public event EventHandler<Avalonia.Interactivity.RoutedEventArgs>? OnLogin
    {
        add => this.SignInButton.Click += value;
        remove => this.SignInButton.Click -= value;
    }
    public LoginView()
    {
        InitializeComponent();
    }
}