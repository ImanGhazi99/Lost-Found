namespace CampusLostAndFound.Library;
public sealed class Staff
{
    public string StaffId { get; }
    public string Name { get; }
    public string Email { get; }
    internal Staff(string id, string name, string email)
    {
        StaffId = Guard.Text(id, "ID");
        Name = Guard.Text(name, "Name");
        Email = Guard.Email(email);
    }
    public override string ToString() { return Name + " (" + StaffId + ")"; }
    public void Display() { Console.WriteLine(ToString()); }
}
