namespace CampusLostAndFound.Library;
public sealed class LostReport
{
    public int ReportId { get; }
    public Student Owner { get; }
    public string Description { get; }
    public Location LastSeenAt { get; }
    public DateTime DateLost { get; }
    public bool IsClosed { get; private set; }
    internal LostReport(int id, Student owner, string description, Location location, DateTime date)
    {
        ReportId = id; Owner = owner; Description = Guard.Text(description, "Description"); LastSeenAt = location; DateLost = Guard.Past(date, "Date lost");
    }
    internal void CloseReport()
    {
        if (IsClosed) throw new InvalidOperationException("This lost report is already closed.");
        IsClosed = true;
    }
    public override string ToString() { return "LR-" + ReportId.ToString("D3") + " · " + Description; }
}
