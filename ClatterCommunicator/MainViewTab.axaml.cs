using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Lucide.Avalonia;

namespace ClatterCommunicator;

public partial class MainViewTab : UserControl
{
    public MainViewTab()
    {
        InitializeComponent();
    }

    public string label
    {
        set => this.TabName.Text = value;
    }

    public LucideIconKind icon
    {
        set => this.Icon.Kind = value;
    }

    public int iconSize
    {
        set => this.Icon.Size = value;
    }

    public string state
    {
        set
        {
            if (value == "active")
            {
                this.Icon.Foreground = Brush.Parse("#0079A2");
                this.TabName.Foreground = Brush.Parse("#0079A2");
            }
            else
            {
                this.Icon.Foreground = Brush.Parse("#666666");
                this.TabName.Foreground = Brush.Parse("#666666");
            }
        }
    }
}