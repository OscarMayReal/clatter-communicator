using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ClatterCommunicator.ClatterClasses;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace ClatterCommunicator;

public partial class Notification : Window
{
    public Message? Message { get; set; }
    public string ChannelName { get; set; }
    private Window? TargetWindow;

    // Computed property for binding
    public string SenderAndChannel => $"{Message?.sendername} - {ChannelName}";

    public Notification(Message? message, string channelName, Window targetWindow)
    {
        InitializeComponent();
        Message = message;
        ChannelName = channelName;
        TargetWindow = targetWindow;

        DataContext = this;

        ShowInTaskbar = false;
        Topmost = true;
        ShowActivated = false;

        // Handle click
        this.PointerPressed += (_, __) =>
        {
            TargetWindow?.Activate();
            Close();
        };

        // Auto-close
        Opened += async (_, __) =>
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await Task.Delay(50); // ensure layout initialized

                var screen = Screens.Primary;
                if (screen != null)
                {
                    var workingArea = screen.WorkingArea;
                    var windowSize = new PixelSize((int)this.Width, (int)this.Height);
                    const int margin = 0;
                    bool isMac = OperatingSystem.IsMacOS();

                    int x = workingArea.Width - windowSize.Width - margin;
                    int y = isMac ? margin : workingArea.Height - windowSize.Height - margin;

                    Position = new PixelPoint(x, y);
                }

                await Task.Delay(5000);
                Close(); // auto-close silently
            });
        };
    }
}