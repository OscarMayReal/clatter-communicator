using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using ClatterCommunicator.ClatterClasses;
using Livekit;

namespace ClatterCommunicator;

public partial class ChatWindow : Window
{
    
    private Channel channel { get; set; }
    private SocketIOClient.SocketIO client { get; set; }
    private ObservableCollection<Message> messages { get; set; }
    private string userid { get; set; }
    public class SocketSendMessageObject
    {
        public string room { get; set; }
        public string content { get; set; }
        public string type { get; set; }
        public string userId { get; set; }
        public string sendername { get; set; }
        public string method { get; set; }
        public string token { get; set; }
    }
    
    public class IncomingSocketmessage
    {
        public string room { get; set; }
        public string content { get; set; }
        public string type { get; set; }
        public string userId { get; set; }
        public string sendername { get; set; }
        public string method { get; set; }
        public string token { get; set; }
        public string DateCreated { get; set; }
        public string id { get; set; }
    }


    public ChatWindow(Channel channel)
    {
        InitializeComponent();
        this.TitleBarText.Text = $"#{channel.name} - Clatter Communicator";
        this.HeaderChannelName.Text = "#" + channel.name;
        this.MessageInputTextBox.Watermark = $"Send a message to #{channel.name}";
        this.channel = channel;
        loadMessages();
    }

    public async void setupSocketConnection(string url)
    {
        this.client = new SocketIOClient.SocketIO(url);
        this.client.On("clatter.channel.join.response", response =>
        {
            Console.WriteLine("connected");
        });
        this.client.On("clatter.channel.message.recieve", message =>
        {
            Console.WriteLine(message);
            string recievedMessageString = message.GetValue<String>();
            IncomingSocketmessage recievedMessage = JsonSerializer.Deserialize<IncomingSocketmessage>(recievedMessageString);
            Console.WriteLine(recievedMessage.content);
            Message newmessage = new Message
            {
                messagetype = recievedMessage.type,
                content = recievedMessage.content,
                sendername = recievedMessage.sendername,
                sender = recievedMessage.userId,
                DateCreated = recievedMessage.DateCreated,
                id = recievedMessage.id,
                parentid = recievedMessage.room
            };
            if (this.messages[this.messages.Count - 1].sender == newmessage.sender)
            {
                newmessage.HideInfo = true;
            }

            if (newmessage.sender == this.userid)
            {
                newmessage.isOwnMessage = FlowDirection.RightToLeft;
                newmessage.background = Brush.Parse("#0079A2");
                newmessage.foreground = Brush.Parse("#ffffff");
            }
            else
            {
                newmessage.isOwnMessage = FlowDirection.LeftToRight;
                newmessage.background = Brush.Parse("#9CECFF");
                newmessage.foreground = Brush.Parse("#666666");
            }
            Console.WriteLine(newmessage.sendername);
            this.messages.Add(newmessage);
            // Console.WriteLine(this.messages[this.messages.Length - 1].content);
            // this.MessageArea.ItemsSource = this.messages;
        });
        await this.client.ConnectAsync();
        await this.client.EmitAsync("clatter.channel.join", "{\"room\":\"" + this.channel.id + "\"}");
    }

    public async void SendMessage(string message)
    {
        StreamReader file = File.OpenText("./clatter-data/user.json");
        string json = file.ReadToEnd();
        LoginView.LoginRootObject?  decodedjson = JsonSerializer.Deserialize<LoginView.LoginRootObject>(json);
        await this.client.EmitAsync("clatter.channel.message.send", JsonSerializer.Serialize(new SocketSendMessageObject
        {
            room = this.channel.id,
            content = message,
            type = "text",
            userId = decodedjson.user.id,
            sendername = decodedjson.user.name,
            method = "modern",
            token = decodedjson.token
        }));
    }

    public async void loadMessages()
    {
        StreamReader file = File.OpenText("./clatter-data/user.json");
        string json = file.ReadToEnd();
        LoginView.LoginRootObject? decodedjson = JsonSerializer.Deserialize<LoginView.LoginRootObject>(json);
        this.userid = decodedjson.user.id;
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
        this.messages = new ObservableCollection<Message>(messages);
        this.messages.CollectionChanged += (s, e) =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                this.MessageArea.ScrollIntoView(this.messages.Count - 1);
            });
        };
        this.MessageArea.ItemsSource = this.messages;
        ScrollChatToBottom();
        setupSocketConnection(decodedjson.url);
    }

    private async void ScrollChatToBottom()
    {
        await Task.Run(()=>Task.Delay(100));
        this.MessageArea.ScrollIntoView(this.messages.Count - 1);
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

    private void SendButton_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        SendMessage(this.MessageInputTextBox.Text);
        this.MessageInputTextBox.Text = string.Empty;
    }
}