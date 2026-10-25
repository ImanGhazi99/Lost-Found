namespace CampusLostAndFound.Library;

/// <summary>A message about a claim. Delivery is simulated by printing it in the console demo.</summary>
public class Notification
{
    public int NotificationId { get; }
    public string Message { get; }
    public DateTime CreatedAt { get; }
    public bool IsRead { get; private set; }

    public Notification(int id, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Notification message is required.");
        NotificationId = id; Message = message.Trim(); CreatedAt = DateTime.Now;
    }

    public void MarkAsRead() => IsRead = true;
}
