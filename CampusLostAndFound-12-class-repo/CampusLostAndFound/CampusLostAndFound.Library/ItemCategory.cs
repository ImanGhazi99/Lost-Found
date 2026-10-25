namespace CampusLostAndFound.Library;

/// <summary>A category used to group found items (a class, so it can carry a description).</summary>
public class ItemCategory
{
    public int CategoryId { get; }
    public string Name { get; }
    public string Description { get; set; } = "";

    public ItemCategory(int id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required.");
        CategoryId = id; Name = name.Trim();
    }

    public void Display() => Console.WriteLine($"Category #{CategoryId}: {Name} - {Description}");
}
