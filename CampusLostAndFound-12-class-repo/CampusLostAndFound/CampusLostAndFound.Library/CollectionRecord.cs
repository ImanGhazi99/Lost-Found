namespace CampusLostAndFound.Library;

/// <summary>Proof that staff handed an item to the student whose claim was approved.</summary>
public class CollectionRecord
{
    public int CollectionId { get; }
    public Claim ApprovedClaim { get; }
    public Staff HandledBy { get; }
    public DateTime CollectedAt { get; }
    public string Remarks { get; set; } = "";

    public CollectionRecord(int id, Claim claim, Staff staff)
    {
        ArgumentNullException.ThrowIfNull(claim);
        ArgumentNullException.ThrowIfNull(staff);
        if (claim.Status != ClaimStatus.Approved)
            throw new InvalidOperationException("Only an approved claim can have a collection record.");
        CollectionId = id; ApprovedClaim = claim; HandledBy = staff; CollectedAt = DateTime.Now;
    }

    public void DisplayReceipt()
    {
        Console.WriteLine($"Receipt #{CollectionId}: '{ApprovedClaim.Item.Description}' collected by " +
                          $"{ApprovedClaim.Student.Name}, handled by {HandledBy.Name} on {CollectedAt:d MMM yyyy}.");
        if (!string.IsNullOrWhiteSpace(Remarks)) Console.WriteLine($"Remarks: {Remarks}");
    }
}
