namespace CampusLostAndFound.Library;

/// <summary>An item that has been handed in. Its status is derived from its claims.</summary>
public class FoundItem
{
    private bool isReturned;

    public int ItemId { get; }
    public string Description { get; }
    public DateTime DateFound { get; set; }
    public Location? FoundAt { get; set; }
    public ItemCategory? Category { get; set; }
    public List<ItemPhoto> Photos { get; } = new();
    public List<Claim> Claims { get; } = new();

    /// <summary>Returned once collected; ClaimPending while any claim is pending or approved; otherwise Unclaimed.</summary>
    public ItemStatus Status
    {
        get
        {
            if (isReturned) return ItemStatus.Returned;
            if (Claims.Any(c => c.Status == ClaimStatus.Pending || c.Status == ClaimStatus.Approved))
                return ItemStatus.ClaimPending;
            return ItemStatus.Unclaimed;
        }
    }

    public FoundItem(int id, string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Item description is required.");
        ItemId = id; Description = description.Trim();
    }

    public void AddPhoto(ItemPhoto photo)
    {
        ArgumentNullException.ThrowIfNull(photo);
        Photos.Add(photo);
    }

    /// <summary>Marks the item as handed back. Needs an approved claim and can only happen once.</summary>
    public void MarkReturned()
    {
        if (isReturned) throw new InvalidOperationException("Item has already been returned.");
        if (!Claims.Any(c => c.Status == ClaimStatus.Approved))
            throw new InvalidOperationException("Item needs an approved claim before it can be returned.");
        isReturned = true;
    }
}
