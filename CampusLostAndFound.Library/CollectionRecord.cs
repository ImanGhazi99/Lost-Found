namespace CampusLostAndFound.Library;
public sealed class CollectionRecord
{
    public int CollectionId { get; }
    public Claim ApprovedClaim { get; }
    public Staff HandledBy { get; }
    public DateTime CollectedAt { get; internal set; } = DateTime.Now;
    public string Remarks { get; }
    internal CollectionRecord(int id, Claim claim, Staff staff, string remarks)
    {
        CollectionId = id; ApprovedClaim = claim; HandledBy = staff; Remarks = Guard.Text(remarks, "Handover verification");
    }
    public string ReceiptText()
    {
        return "CAMPUS LOST AND FOUND\nCOLLECTION RECEIPT CR-" + CollectionId.ToString("D3") +
            "\n\nItem: " + ApprovedClaim.Item + "\nClaim: CL-" + ApprovedClaim.ClaimId.ToString("D3") +
            "\nCollected by: " + ApprovedClaim.Student + "\nHandled by: " + HandledBy +
            "\nCollected at: " + CollectedAt.ToString("dd MMM yyyy HH:mm") + "\nVerification: " + Remarks;
    }
    public void DisplayReceipt() { Console.WriteLine(ReceiptText()); }
}
