namespace ClatterCommunicator.ClatterClasses;

public class Member
{
    public string organizationId { get; set; }
    public string userId { get; set; }
    public string role { get; set; }
    public string createdAt { get; set; }
    public string id { get; set; }
    public User user { get; set; }
}