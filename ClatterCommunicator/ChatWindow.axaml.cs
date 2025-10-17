using System;
using System.IO;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using ClatterCommunicator.ClatterClasses;
using Livekit;

namespace ClatterCommunicator;

public partial class ChatWindow : Window
{
    
    private Channel channel { get; set; }
    
    private Message[] messages { get; set; }
    public ChatWindow(Channel channel)
    {
        InitializeComponent();
        this.TitleBarText.Text = $"#{channel.name} - Clatter Communicator";
        this.HeaderChannelName.Text = "#" + channel.name;
        this.MessageInputTextBox.Watermark = $"Send a message to #{channel.name}";
        this.channel = channel;
        loadMessages();
        setupSocketConnection();
    }

    public async void setupSocketConnection()
    {
        SocketIOClient.SocketIO client = new SocketIOClient.SocketIO("https://beta.clatter.work");
        client.On("clatter.channel.join.response", response =>
        {
            Console.WriteLine("connected");
        });
        await client.ConnectAsync();
        await client.EmitAsync("clatter.channel.join", "{\"room\":\"" + this.channel.id + "\"}");
    }

    public async void loadMessages()
    {
        StreamReader file = File.OpenText("./clatter-data/user.json");
        string json = file.ReadToEnd();
        LoginView.LoginRootObject? decodedjson = JsonSerializer.Deserialize<LoginView.LoginRootObject>(json);
        Message[] messages = await channel.GetMessages();
        for (var i = 0; i < messages.Length; i++)
        {
            if (i != 0)
            {
                if (messages[i].sender == messages[i - 1].sender)
                {
                    messages[i].HideInfo = true;
                }
            }

            if (decodedjson.user.id == messages[i].sender)
            {
                messages[i].isOwnMessage = FlowDirection.RightToLeft;
                messages[i].background = Brush.Parse("#0079A2");
                messages[i].foreground = Brush.Parse("#ffffff");
            }
            else
            {
                messages[i].isOwnMessage = FlowDirection.LeftToRight;
                messages[i].background = Brush.Parse("#9CECFF");
                messages[i].foreground = Brush.Parse("#666666");
            }
        }
        this.MessageArea.ItemsSource = messages;
        this.messages = messages;
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