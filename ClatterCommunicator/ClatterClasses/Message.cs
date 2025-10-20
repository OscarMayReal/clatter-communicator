using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace ClatterCommunicator.ClatterClasses;

public class Message
{
    public string id { get; set; }
    public string messagetype { get; set; }
    public string parenttype { get; set; }
    public string parentid { get; set; }
    public string content { get; set; }
    public object parentmessageid { get; set; }
    public string DateCreated { get; set; }
    public string sender { get; set; }
    public string sendername { get; set; }
    public bool pinned { get; set; }
    public object[] reactions { get; set; }
    public Message[] childmessages { get; set; }
    public Avalonia.Media.FlowDirection isOwnMessage { get; set; }
    public IBrush background { get; set; }
    public IBrush foreground { get; set; }
    public bool HideInfo {get; set;}
    public Bitmap userImage { get; set; }
}
