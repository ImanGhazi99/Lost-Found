namespace CampusLostAndFound.Library;
public sealed class ItemPhoto
{
    public int PhotoId { get; }
    public string FilePath { get; }
    public string Caption { get; }
    internal ItemPhoto(int id, string path, string caption)
    {
        PhotoId = id; FilePath = Guard.Text(path, "Photo file"); Caption = caption.Trim();
    }
    public void Display() { Console.WriteLine(Caption + " " + FilePath); }
}
