namespace CampusLostAndFound.Library;

/// <summary>A lost-and-found staff member who hands approved items back to students.</summary>
public class Staff
{
    public string StaffId { get; }
    public string Name { get; }
    public string Email { get; }

    public Staff(string id, string name, string email)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Staff ID, name and email are required.");
        StaffId = id.Trim(); Name = name.Trim(); Email = email.Trim();
    }

    public void Display() => Console.WriteLine($"Staff {StaffId}: {Name} <{Email}>");
}
