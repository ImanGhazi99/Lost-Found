namespace CampusLostAndFound.Library;

/// <summary>A student's request to be recognised as the owner of a found item.</summary>
public class Claim
{
    public int ClaimId { get; }
    public Student Student { get; }
    public FoundItem Item { get; }
    public ClaimStatus Status { get; private set; } = ClaimStatus.Pending;
    public List<ClaimEvidence> Evidence { get; } = new();
    public List<Notification> Notifications { get; } = new();

    public Claim(int id, Student student, FoundItem item)
    {
        ArgumentNullException.ThrowIfNull(student);
        ArgumentNullException.ThrowIfNull(item);
        ClaimId = id; Student = student; Item = item;
        Notify($"Claim #{ClaimId} for '{Item.Description}' was received and is pending review.");
    }

    public void AddEvidence(ClaimEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        EnsurePending();
        Evidence.Add(evidence);
    }

    public void Approve()
    {
        EnsurePending();
        Status = ClaimStatus.Approved;
        Notify($"Claim #{ClaimId} was approved. Please collect '{Item.Description}' from staff.");
    }

    public void Reject()
    {
        EnsurePending();
        Status = ClaimStatus.Rejected;
        Notify($"Claim #{ClaimId} for '{Item.Description}' was rejected.");
    }

    private void EnsurePending()
    {
        if (Status != ClaimStatus.Pending)
            throw new InvalidOperationException($"Claim #{ClaimId} is already {Status.ToString().ToLower()}.");
    }

    private void Notify(string message) =>
        Notifications.Add(new Notification(Notifications.Count + 1, message));
}
