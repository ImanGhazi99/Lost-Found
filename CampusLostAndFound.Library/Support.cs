namespace CampusLostAndFound.Library;
public enum ItemStatus { Unclaimed, ClaimPending, AwaitingCollection, Returned }
public enum ClaimStatus { Pending, Approved, Rejected }
internal static class Guard
{
    internal static string Text(string value, string label)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(label + " is required.");
        return value.Trim();
    }
    internal static string Email(string value)
    {
        value = Text(value, "Email");
        if (!System.Net.Mail.MailAddress.TryCreate(value, out var address) || address.Address != value)
            throw new ArgumentException("Enter a valid email address.");
        return value;
    }
    internal static DateTime Past(DateTime value, string label)
    {
        if (value == default || value.Date > DateTime.Today) throw new ArgumentException(label + " must be today or earlier.");
        return value.Date;
    }
}
