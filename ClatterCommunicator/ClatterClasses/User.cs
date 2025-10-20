using Avalonia.Media.Imaging;

namespace ClatterCommunicator.ClatterClasses;

public class User
{
    public string id { get; set; }
    public string name { get; set; }
    public string email { get; set; }
    public string image { get; set; }
    public Bitmap imageBitmap { get; set; }
}