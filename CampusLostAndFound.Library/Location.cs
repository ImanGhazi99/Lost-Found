namespace CampusLostAndFound.Library;
public sealed class Location
{
    public int LocationId { get; }
    public string Name { get; }
    public string Building { get; }
    internal Location(int id, string name, string building)
    {
        LocationId = id; Name = Guard.Text(name, "Location"); Building = Guard.Text(building, "Building");
    }
    public override string ToString() { return Name + " · " + Building; }
    public void Display() { Console.WriteLine(ToString()); }
}
