namespace CampusLostAndFound.Library;

/// <summary>A place on campus where an item was found or last seen.</summary>
public class Location
{
    public int LocationId { get; }
    public string Name { get; }
    public string Building { get; }

    public Location(int id, string name, string building)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(building))
            throw new ArgumentException("Location name and building are required.");
        LocationId = id; Name = name.Trim(); Building = building.Trim();
    }

    public void Display() => Console.WriteLine($"Location #{LocationId}: {Name} ({Building})");
}
