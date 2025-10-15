using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AsyncImageLoader;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using RouteNav.Avalonia;

namespace ClatterCommunicator;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }
    
    public class Workspace
    {
        public string name { get; set; }
        public string slug { get; set; }
        public string logo { get; set; }
        public string createdAt { get; set; }
        public object metadata { get; set; }
        public string id { get; set; }
    }

    private async Task<Workspace[]> ListWorkspace(string token)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://beta.clatter.work/api/auth/organization/list"),
            Headers =
            {
                { "Authorization", $"Bearer {token}" }
            }
        };
        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            Workspace[] workspaces = JsonSerializer.Deserialize<Workspace[]>(body);
            return workspaces;
        }
    }

    private async void SetWorkspaceTitle(String token)
    {
        Workspace[] workspaces = await ListWorkspace(token);
        this.StatusTextBlock.Text = workspaces[0].name;
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
            byte[] binaryData = Convert.FromBase64String(decodedjson.user.image.Split("data:image/png;base64,")[1]);
            using (MemoryStream stream = new MemoryStream(binaryData))
            {
                Bitmap bi = new Bitmap(stream);
                this.UserImage.Source = bi;
            }
            SetWorkspaceTitle(decodedjson.token);
        }
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var chatwindow = new ChatWindow();
        chatwindow.Show();
    }
}