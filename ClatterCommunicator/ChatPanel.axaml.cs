using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ClatterCommunicator.ClatterClasses;

namespace ClatterCommunicator;

public partial class ChatPanel : UserControl
{
    public ChatPanel()
    {
        InitializeComponent();
    }

    public async void SetChannels(Workspace workspace)
    {
        Channel[] channels = await workspace.GetChannels();
        this.ChannelListbox.ItemsSource = channels;
        this.ChannelsExpander.Header = $"Channels ({channels.Length})";
        
    }

    private void ChannelListbox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Channel item = this.ChannelListbox.SelectedItems[0]  as Channel;
        new ChatWindow(item).Show();
    }
}