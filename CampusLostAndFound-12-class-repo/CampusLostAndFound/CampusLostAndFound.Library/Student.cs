namespace CampusLostAndFound.Library;

/// <summary>A student who can report lost items and claim found items.</summary>
public class Student
{
    public string StudentId { get; }
    public string Name { get; }
    public string Email { get; }

    public Student(string id, string name, string email)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Student ID, name and email are required.");
        StudentId = id.Trim(); Name = name.Trim(); Email = email.Trim();
    }

    public void Display() => Console.WriteLine($"Student {StudentId}: {Name} <{Email}>");
}
