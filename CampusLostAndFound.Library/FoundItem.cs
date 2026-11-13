namespace CampusLostAndFound.Library;
public sealed class FoundItem
{
    private readonly List<Claim> claims = new List<Claim>();
    private readonly List<ItemPhoto> photos = new List<ItemPhoto>();
    public int ItemId { get; }
    public string Description { get; }
    public DateTime DateFound { get; }
    public Location FoundAt { get; }
    public ItemCategory Category { get; }
    public string StorageNote { get; }
    public IReadOnlyList<Claim> Claims { get { return claims.AsReadOnly(); } }
    public IReadOnlyList<ItemPhoto> Photos { get { return photos.AsReadOnly(); } }
    public ItemStatus Status
    {
        get
        {
            foreach (Claim claim in claims) if (claim.Collection != null) return ItemStatus.Returned;
            foreach (Claim claim in claims) if (claim.Status == ClaimStatus.Approved) return ItemStatus.AwaitingCollection;
            foreach (Claim claim in claims) if (claim.Status == ClaimStatus.Pending) return ItemStatus.ClaimPending;
            return ItemStatus.Unclaimed;
        }
    }
    internal FoundItem(int id, string description, Location location, ItemCategory category, DateTime date, string storage)
    {
        ItemId = id; Description = Guard.Text(description, "Description"); FoundAt = location; Category = category;
        DateFound = Guard.Past(date, "Date found"); StorageNote = Guard.Text(storage, "Storage location");
    }
    internal void AddClaim(Claim claim) { claims.Add(claim); }
    internal void AddPhoto(ItemPhoto photo) { photos.Add(photo); }
    public override string ToString() { return "FI-" + ItemId.ToString("D3") + " · " + Description; }
}
