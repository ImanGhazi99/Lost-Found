namespace CampusLostAndFound.Library;

/// <summary>A photo of a found item, used by staff and students to identify it.</summary>
public class ItemPhoto
{
    public int PhotoId { get; }
    public string FilePath { get; }
    public string Caption { get; set; } = "";

    public ItemPhoto(int id, string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Photo file path is required.");
        PhotoId = id; FilePath = filePath.Trim();
    }

    public void Display() => Console.WriteLine($"Photo #{PhotoId}: {FilePath} {Caption}".TrimEnd());
}
