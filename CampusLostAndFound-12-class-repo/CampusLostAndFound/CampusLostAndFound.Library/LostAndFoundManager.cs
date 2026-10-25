namespace CampusLostAndFound.Library;

/// <summary>Holds every found item and controls how claims are submitted, approved and collected.</summary>
public class LostAndFoundManager
{
    private readonly List<FoundItem> items = new();

    public void AddItem(FoundItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (items.Any(i => i.ItemId == item.ItemId))
            throw new InvalidOperationException("An item with this ID is already recorded.");
        if (item.FoundAt is null) throw new ArgumentException("A found item needs a location.");
        if (item.Category is null) throw new ArgumentException("A found item needs a category.");
        if (item.DateFound == default || item.DateFound > DateTime.Now)
            throw new ArgumentException("Date found must be set and cannot be in the future.");
        items.Add(item);
    }

    /// <summary>Case-insensitive search of description, category, location name and building.</summary>
    public List<FoundItem> SearchItems(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return items.ToList();
        keyword = keyword.Trim();
        return items.Where(i =>
                i.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                i.Category!.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                i.FoundAt!.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                i.FoundAt!.Building.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public Claim SubmitClaim(FoundItem item, Student student)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(student);
        if (!items.Contains(item))
            throw new InvalidOperationException("Item is not recorded by this manager.");
        if (item.Status == ItemStatus.Returned)
            throw new InvalidOperationException("Item has already been returned.");
        if (item.Claims.Any(c => c.Status == ClaimStatus.Approved))
            throw new InvalidOperationException("An approved claim is already waiting for collection.");
        if (item.Claims.Any(c => c.Student == student && c.Status == ClaimStatus.Pending))
            throw new InvalidOperationException("Student already has a pending claim for this item.");

        int claimId = items.Sum(i => i.Claims.Count) + 1;
        var claim = new Claim(claimId, student, item);
        item.Claims.Add(claim);
        return claim;
    }

    /// <summary>Approves one claim and rejects every competing pending claim for the same item.</summary>
    public void ApproveClaim(Claim claim)
    {
        ArgumentNullException.ThrowIfNull(claim);
        if (!items.Contains(claim.Item))
            throw new InvalidOperationException("Claim does not belong to an item recorded by this manager.");

        claim.Approve();
        foreach (var rival in claim.Item.Claims.Where(c => c != claim && c.Status == ClaimStatus.Pending).ToList())
            rival.Reject();
    }

    /// <summary>Records the handover of an approved claim and marks its item as Returned.</summary>
    public CollectionRecord RecordCollection(Claim claim, Staff staff)
    {
        ArgumentNullException.ThrowIfNull(claim);
        ArgumentNullException.ThrowIfNull(staff);
        if (claim.Status != ClaimStatus.Approved)
            throw new InvalidOperationException("Only an approved claim can be collected.");
        if (claim.Item.Status == ItemStatus.Returned)
            throw new InvalidOperationException("Item has already been collected.");

        var record = new CollectionRecord(claim.ClaimId, claim, staff); // one collection per approved claim
        claim.Item.MarkReturned();
        claim.Notifications.Add(new Notification(claim.Notifications.Count + 1,
            $"'{claim.Item.Description}' was collected on {record.CollectedAt:d MMM yyyy}."));
        return record;
    }
}
