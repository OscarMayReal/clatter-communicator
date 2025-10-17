using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using ClatterCommunicator.ClatterClasses;

namespace ClatterCommunicator;

public partial class DirectoryPanel : UserControl
{
    public DirectoryPanel()
    {
        InitializeComponent();
    }

    public void UpdateDirectoryList(Member[]  members)
    {
        this.DirectoryListbox.ItemsSource = members;
    }
}