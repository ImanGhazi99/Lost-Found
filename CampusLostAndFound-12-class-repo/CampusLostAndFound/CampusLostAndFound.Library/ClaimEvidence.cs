namespace CampusLostAndFound.Library;

/// <summary>One piece of proof of ownership attached to a claim.</summary>
public class ClaimEvidence
{
    public int EvidenceId { get; }
    public string Description { get; }
    public string FilePath { get; set; } = "";

    public ClaimEvidence(int id, string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Evidence description is required.");
        EvidenceId = id; Description = description.Trim();
    }

    public void Display() => Console.WriteLine($"Evidence #{EvidenceId}: {Description}");
}
