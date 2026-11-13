namespace CampusLostAndFound.Library;
public sealed class Notification
{
    public int NotificationId { get; }
    public string Message { get; }
    public DateTime CreatedAt { get; internal set; }
    public bool IsRead { get; private set; }
    internal Notification(int id, string message)
    {
        NotificationId = id; Message = Guard.Text(message, "Message"); CreatedAt = DateTime.Now;
    }
    internal void MarkAsRead() { IsRead = true; }
}
