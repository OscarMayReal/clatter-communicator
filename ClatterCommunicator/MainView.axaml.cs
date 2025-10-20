using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AsyncImageLoader;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ClatterCommunicator.ClatterClasses;
using RouteNav.Avalonia;

namespace ClatterCommunicator;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
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
            this.DirectoryPanel.UpdateDirectoryList(workspace?.members);
            this.ChatPanel.SetChannels(workspace);
            this.Workspace = workspace;
            return workspace;
        }
    }

    private async void SetWorkspaceTitle(String token, String url)
    {
        Workspace workspace = await ListWorkspace(token, url);
        this.StatusTextBlock.Text = workspace.name;
        // setImageBitmaps();
    }

    private void MainPanelInner_OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        if (!Directory.Exists("./clatter-data"))
        {
            Directory.CreateDirectory("./clatter-data");
        }
        if (File.Exists("./clatter-data/user.json"))
        {
            StreamReader file = File.OpenText("./clatter-data/user.json");
            string json = file.ReadToEnd();
            LoginView.LoginRootObject? decodedjson = JsonSerializer.Deserialize<LoginView.LoginRootObject>(json);
            this.UsernameTextBlock.Text = decodedjson?.user.name;
            //this.StatusTextBlock.Text = decodedjson?.user.email;
            if (decodedjson.user.image != null)
            {
                byte[] binaryData = Convert.FromBase64String(decodedjson.user.image.Split("data:image/png;base64,")[1]);
                using (MemoryStream stream = new MemoryStream(binaryData))
                {
                    Bitmap bi = new Bitmap(stream);
                    this.UserImage.Source = bi;
                }
            }
            SetWorkspaceTitle(decodedjson.token, decodedjson.url);
        }
    }

    public void setImageBitmaps()
    {
        for (var i = 0; i < Workspace.members.Length; i++)
        {
            Console.WriteLine("img " + Workspace.members[i].user.image);
            if (Workspace.members[i].user.image == null)
            {
                Workspace.members[i].user.imageBitmap = new Bitmap(AssetLoader.Open(new Uri("avares://ClatterCommunicator/Assets/images/userimage.png")));
            }
            else
            {
                byte[] binaryData = Convert.FromBase64String(Workspace.members[i].user.image.Split("data:image/png;base64,")[1]);
                using (MemoryStream stream = new MemoryStream(binaryData))
                {
                    Bitmap bi = new Bitmap(stream);
                    Workspace.members[i].user.imageBitmap = bi;
                }
            }
        }
    }
    
    public event EventHandler onLogout;

    private void ChatTab_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        this.DirectoryPanel.IsVisible = false;
        this.ChatPanel.IsVisible = true;
        this.ChatTab.state = "active";
        this.DirectoryTab.state = "inactive";
        // this.MeetingsTab.state = "inactive";
    }

    private void DirectoryTab_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        this.DirectoryPanel.IsVisible = true;
        this.ChatPanel.IsVisible = false;
        this.ChatTab.state = "inactive";
        this.DirectoryTab.state = "active";
        // this.MeetingsTab.state = "inactive";
    }

    private void SignOutMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        this.onLogout?.Invoke(this, EventArgs.Empty);
    }
}