using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClatterCommunicator.ClatterClasses;

public class Channel
{
    public string id { get; set; }
    public string name { get; set; }
    public string type { get; set; }
    public string parentworkspace { get; set; }
    public string DateCreated { get; set; }
    public object[] members { get; set; }
    public object owner { get; set; }
    public object extradata { get; set; }
    public bool discoverable { get; set; }

    public async Task<Message[]> GetMessages()
    {
        StreamReader file = File.OpenText("./clatter-data/user.json");
        string json = file.ReadToEnd();
        LoginView.LoginRootObject? decodedjson = JsonSerializer.Deserialize<LoginView.LoginRootObject>(json);
        var httpclient = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://beta.clatter.work/api/channel/" + this.id + "/messages/list"),
            Headers =
            {
                {"Authorization", $"Bearer {decodedjson?.token}"}
            }
        };
        using (var response = await httpclient.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Message[] messages = JsonSerializer.Deserialize<Message[]>(content);
            return messages;
        }
    }
}

