using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
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
}