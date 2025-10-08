using System;
using Avalonia.Controls;
using Avalonia.Input;

namespace ClatterCommunicator;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void TitleBarXButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Console.Out.WriteLine("Close");
        Close();
    }
}