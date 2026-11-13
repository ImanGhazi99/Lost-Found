namespace CampusLostAndFound.Library;
public sealed class Student
{
    public string StudentId { get; }
    public string Name { get; }
    public string Email { get; }
    internal Student(string id, string name, string email)
    {
        StudentId = Guard.Text(id, "ID");
        Name = Guard.Text(name, "Name");
        Email = Guard.Email(email);
    }
    public override string ToString() { return Name + " (" + StudentId + ")"; }
    public void Display() { Console.WriteLine(ToString()); }
}
