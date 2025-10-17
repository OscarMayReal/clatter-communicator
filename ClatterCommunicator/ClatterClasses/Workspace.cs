using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClatterCommunicator.ClatterClasses;

public class Workspace
{
    public string name { get; set; }
    public string slug { get; set; }
    public string logo { get; set; }
    public string createdAt { get; set; }
    public object metadata { get; set; }
    public string id { get; set; }
    public string[] invitations { get; set; }
    public Member[]  members { get; set; }
    public string teams { get; set; }

    public async Task<Channel[]> GetChannels()
    {
        StreamReader file = File.OpenText("./clatter-data/user.json");
        string json = file.ReadToEnd();
        LoginView.LoginRootObject? decodedjson = JsonSerializer.Deserialize<LoginView.LoginRootObject>(json);
        var httpclient = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://beta.clatter.work/api/channels/list"),
            Headers =
            {
                {"Authorization", $"Bearer {decodedjson?.token}"}
            }
        };
        using (var response = await httpclient.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Channel[] channels = JsonSerializer.Deserialize<Channel[]>(content);
            return channels.Where(c => c.type == "clatter.channeltype.message").ToArray();
        }
    }
    public async Task<Channel[]> GetDirects()
    {
        StreamReader file = File.OpenText("./clatter-data/user.json");
        string json = file.ReadToEnd();
        LoginView.LoginRootObject? decodedjson = JsonSerializer.Deserialize<LoginView.LoginRootObject>(json);
        var httpclient = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://beta.clatter.work/api/channels/list"),
            Headers =
            {
                {"Authorization", $"Bearer {decodedjson?.token}"}
            }
        };
        using (var response = await httpclient.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Channel[] channels = JsonSerializer.Deserialize<Channel[]>(content);
            return channels.Where(c => c.type == "clatter.directtype.1x1").ToArray();
        }
    }
}