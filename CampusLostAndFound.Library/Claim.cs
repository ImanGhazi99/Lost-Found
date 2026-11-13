namespace CampusLostAndFound.Library;
public sealed class Claim
{
    private readonly List<ClaimEvidence> evidence = new List<ClaimEvidence>();
    private readonly List<Notification> notifications = new List<Notification>();
    public int ClaimId { get; }
    public Student Student { get; }
    public FoundItem Item { get; }
    public ClaimStatus Status { get; private set; } = ClaimStatus.Pending;
    public DateTime SubmittedAt { get; internal set; } = DateTime.Now;
    public DateTime? ResolvedAt { get; internal set; }
    public Staff? ReviewedBy { get; private set; }
    public string DecisionNote { get; private set; } = "";
    public LostReport? RelatedReport { get; }
    public CollectionRecord? Collection { get; private set; }
    public IReadOnlyList<ClaimEvidence> Evidence { get { return evidence.AsReadOnly(); } }
    public IReadOnlyList<Notification> Notifications { get { return notifications.AsReadOnly(); } }
    internal Claim(int id, Student student, FoundItem item, LostReport? report)
    {
        ClaimId = id; Student = student; Item = item; RelatedReport = report;
    }
    internal void AddEvidence(ClaimEvidence entry)
    {
        EnsurePending(); evidence.Add(entry);
    }
    internal void Approve(Staff staff, string note)
    {
        EnsurePending();
        if (evidence.Count == 0) throw new InvalidOperationException("Add ownership evidence before approval.");
        ReviewedBy = staff; DecisionNote = note; ResolvedAt = DateTime.Now; Status = ClaimStatus.Approved;
    }
    internal void Reject(Staff staff, string reason)
    {
        EnsurePending(); ReviewedBy = staff; DecisionNote = reason; ResolvedAt = DateTime.Now; Status = ClaimStatus.Rejected;
    }
    private void EnsurePending()
    {
        if (Status != ClaimStatus.Pending) throw new InvalidOperationException("This claim has already been resolved.");
    }
    internal void SetCollection(CollectionRecord record)
    {
        if (Status != ClaimStatus.Approved || Collection != null) throw new InvalidOperationException("Only an approved, uncollected claim can be collected.");
        Collection = record;
    }
    internal void AddNotification(Notification notification) { notifications.Add(notification); }
    internal void ClearNotifications() { notifications.Clear(); }
    public override string ToString() { return "CL-" + ClaimId.ToString("D3") + " · " + Student.Name; }
}
