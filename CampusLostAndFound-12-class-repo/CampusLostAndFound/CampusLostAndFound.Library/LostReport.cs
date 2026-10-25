namespace CampusLostAndFound.Library;

/// <summary>A student's report that something has been lost.</summary>
public class LostReport
{
    public int ReportId { get; }
    public Student Owner { get; }
    public string Description { get; set; } = "";
    public Location? LastSeenAt { get; set; }
    public DateTime DateLost { get; set; }
    public bool IsClosed { get; private set; }

    public LostReport(int id, Student owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ReportId = id; Owner = owner;
    }

    public void CloseReport()
    {
        if (IsClosed) throw new InvalidOperationException("Report is already closed.");
        IsClosed = true;
    }
}
