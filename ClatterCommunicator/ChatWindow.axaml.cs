using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using ClatterCommunicator.ClatterClasses;
using Livekit;
using NetCoreAudio;

namespace ClatterCommunicator;

public partial class ChatWindow : Window
{
    private Player audioPlayer { get; set; }
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
        audioPlayer = new Player();
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
            else
            {
                newmessage.userImage = Workspace.members.Where(item =>
                {
                    return item.user.id == newmessage.sender;
                }).First().user.imageBitmap;
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
            if (newmessage.sender != this.userid && !IsActive)
            {
                audioPlayer.Play(Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"Assets/audio/notify.mp3"));
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    new Notification(newmessage, "#" + this.channel.name, this).Show();
                });
            }
            // Console.WriteLine(this.messages[this.messages.Length - 1].content);
            // this.MessageArea.ItemsSource = this.messages;
        });
        await this.client.ConnectAsync();
        await this.client.EmitAsync("clatter.channel.join", "{\"room\":\"" + this.channel.id + "\"}");
    }
    
    public void setImageBitmaps()
    {
        for (var i = 0; i < Workspace.members.Length; i++)
        {
            if (Workspace.members[i].user.image == null || Workspace.members[i].user.image == String.Empty || !Workspace.members[i].user.image.Contains("data:image/png;base64,"))
            {
                Workspace.members[i].user.imageBitmap = new Bitmap(AssetLoader.Open(new Uri("avares://ClatterCommunicator/Assets/images/userimage.png")));
            }
            else
            {
                Console.WriteLine("img " + Workspace.members[i].user.image.Split("data:image/png;base64,").Length);
                byte[] binaryData = Convert.FromBase64String(Workspace.members[i].user.image.Split("data:image/png;base64,")[1]);
                using (MemoryStream stream = new MemoryStream(binaryData))
                {
                    Bitmap bi = new Bitmap(stream);
                    Workspace.members[i].user.imageBitmap = bi;
                }
            }
        }
    }
    
    public Workspace Workspace { get; set; }
    
    private async Task<Workspace> ListWorkspace(string token, string url)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{url}/api/auth/organization/get-full-organization"),
            Headers =
            {
                { "Authorization", $"Bearer {token}" }
            }
        };
        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            Workspace workspace = JsonSerializer.Deserialize<Workspace>(body);
            this.Workspace = workspace;
            return workspace;
        }
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
        await ListWorkspace(decodedjson.token, decodedjson.url);
        setImageBitmaps();
        for (var i = 0; i < messages.Length; i++)
        {
            if (i != 0)
            {
                if (messages[i].sender == messages[i - 1].sender)
                {
                    messages[i].HideInfo = true;
                } 
                else
                {
                    messages[i].userImage = Workspace.members.Where(item =>
                    {
                        return item.user.id == messages[i].sender;
                    }).First().user.imageBitmap;
                }
            }
            else
            {
                messages[i].userImage = Workspace.members.Where(item =>
                {
                    return item.user.id == messages[i].sender;
                }).First().user.imageBitmap;
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

    private void MessageInputTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SendMessage(this.MessageInputTextBox.Text);
            this.MessageInputTextBox.Text = string.Empty;
        }
    }
}