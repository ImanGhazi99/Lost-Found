namespace CampusLostAndFound.Library;
public sealed class ClaimEvidence
{
    public int EvidenceId { get; }
    public string Description { get; }
    public string FilePath { get; }
    internal ClaimEvidence(int id, string description, string filePath)
    {
        EvidenceId = id; Description = Guard.Text(description, "Evidence description"); FilePath = filePath.Trim();
    }
    public void Display() { Console.WriteLine(Description); }
}
