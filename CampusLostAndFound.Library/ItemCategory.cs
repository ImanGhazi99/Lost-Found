namespace CampusLostAndFound.Library;
public sealed class ItemCategory
{
    public int CategoryId { get; }
    public string Name { get; }
    public string Description { get; }
    internal ItemCategory(int id, string name, string description)
    {
        CategoryId = id; Name = Guard.Text(name, "Category"); Description = description.Trim();
    }
    public override string ToString() { return Name; }
    public void Display() { Console.WriteLine(Name + ": " + Description); }
}
