using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using RouteNav.Avalonia;

namespace ClatterCommunicator;

public partial class LoginView : UserControl
{
    public class LoginRootObject
    {
        public bool redirect { get; set; }
        public string token { get; set; }
        public User user { get; set; }
        public string url { get; set; }
    }

    public class User
    {
        public string id { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public string image { get; set; }
        public bool emailVerified { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
    }

    public class LoggedInSessionEventArgs(LoginRootObject session) : EventArgs
    {
        public LoginRootObject session { get; set; }
    }

    public async void doLogin(string username, string password, string url)
    {
        if (!Directory.Exists("./clatter-data"))
        {
            Directory.CreateDirectory("./clatter-data");
        }
        var httpclient = new HttpClient();
        httpclient.DefaultRequestHeaders.Accept.Clear();
        httpclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(url + "/api/auth/sign-in/email"),
            Content = new StringContent("{\"email\":\"" + username.Split("::")[0] + "\",\"password\":\"" + password + "\"}")
            {
                Headers =
                {
                    ContentType = new MediaTypeHeaderValue("application/json")
                }
            }
        };
        using (var response = await httpclient.SendAsync(request))
        {
            string json = await response.Content.ReadAsStringAsync();
            LoginRootObject? decodedjson = JsonSerializer.Deserialize<LoginRootObject>(json);
            decodedjson.url = url;
            string newjson = JsonSerializer.Serialize<LoginRootObject>(decodedjson);
            if (File.Exists("./clatter-data/user.json"))
            {
                File.Delete("./clatter-data/user.json");
            }
            FileStream fileopen = File.OpenWrite("./clatter-data/user.json");
            byte[] bytes = new UTF8Encoding(true).GetBytes(newjson);
            fileopen.Write(bytes, 0, bytes.Length);
            fileopen.Close();
            var newhttpclient = new HttpClient();
            newhttpclient.DefaultRequestHeaders.Accept.Clear();
            newhttpclient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            Console.WriteLine(username.Split("::")[1]);
            Console.WriteLine(decodedjson.token);
            var newrequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url + "/api/auth/organization/set-active"),
                Content = new StringContent("{\"organizationSlug\":\"" + username.Split("::")[1] + "\"}")
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                },
                Headers =
                {
                    { "Authorization", $"Bearer {decodedjson?.token}" }
                }
            };
            using (var newresponse = await newhttpclient.SendAsync(newrequest))
            {
                OnLogin?.Invoke(this, new LoggedInSessionEventArgs(session: decodedjson));   
            }
        }
    }

    public event EventHandler<LoggedInSessionEventArgs>? OnLogin;
    public LoginView()
    {
        InitializeComponent();
        if (!Directory.Exists("./clatter-data"))
        {
            Directory.CreateDirectory("./clatter-data");
        }
        if (File.Exists("./clatter-data/user.json"))
        {
            StreamReader file = File.OpenText("./clatter-data/user.json");
            string json = file.ReadToEnd();
            LoginRootObject? decodedjson = JsonSerializer.Deserialize<LoginRootObject>(json);
            try
            {
                this.SignInPanel.IsVisible = false;
                this.ExistingUsernameBox.Text = decodedjson.user.email;
                this.ExistingUrlBox.Text = decodedjson.url;
                this.ExistingSessionPanel.IsVisible = true;
            }
            catch
            {
                this.SignInPanel.IsVisible = true;
                this.ExistingSessionPanel.IsVisible = false;
            }
        }
    }

    private void DoLoginWIthExistingSession()
    {
        StreamReader file = File.OpenText("./clatter-data/user.json");
        string json = file.ReadToEnd();
        LoginRootObject? decodedjson = JsonSerializer.Deserialize<LoginRootObject>(json);
        OnLogin?.Invoke(this, new LoggedInSessionEventArgs(session: decodedjson));
    }

    private void SignInButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if ((string.IsNullOrEmpty(this.PasswordBox.Text) || string.IsNullOrEmpty(this.UrlBox.Text) ||
            string.IsNullOrEmpty(this.UsernameBox.Text)) && !this.ExistingSessionPanel.IsVisible)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Please fill all the fields", ButtonEnum.Ok);
            box.ShowAsync();
        }
        else
        {
            if (this.ExistingSessionPanel.IsVisible)
            {
                DoLoginWIthExistingSession();
            }
            else
            {
                doLogin(this.UsernameBox.Text!, this.PasswordBox.Text!, this.UrlBox.Text!);   
            }
        }
    }

    private void SwitchAccountsButton_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        this.ExistingSessionPanel.IsVisible = false;
        this.SignInPanel.IsVisible = true;
    }
}