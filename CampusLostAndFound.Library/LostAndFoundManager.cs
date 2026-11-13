namespace CampusLostAndFound.Library;

public sealed class LostAndFoundManager
{
    private readonly List<Student> students = new List<Student>();
    private readonly List<Staff> staff = new List<Staff>();
    private readonly List<Location> locations = new List<Location>();
    private readonly List<ItemCategory> categories = new List<ItemCategory>();
    private readonly List<FoundItem> items = new List<FoundItem>();
    private readonly List<LostReport> reports = new List<LostReport>();
    private readonly List<Claim> claims = new List<Claim>();
    private readonly List<CollectionRecord> collections = new List<CollectionRecord>();
    public IReadOnlyList<Student> Students { get { return students.AsReadOnly(); } }
    public IReadOnlyList<Staff> StaffMembers { get { return staff.AsReadOnly(); } }
    public IReadOnlyList<Location> Locations { get { return locations.AsReadOnly(); } }
    public IReadOnlyList<ItemCategory> Categories { get { return categories.AsReadOnly(); } }
    public IReadOnlyList<FoundItem> Items { get { return items.AsReadOnly(); } }
    public IReadOnlyList<LostReport> Reports { get { return reports.AsReadOnly(); } }
    public IReadOnlyList<Claim> Claims { get { return claims.AsReadOnly(); } }
    public IReadOnlyList<CollectionRecord> Collections { get { return collections.AsReadOnly(); } }

    public Student RegisterStudent(string id, string name, string email)
    {
        Student entry = new Student(id, name, email);
        foreach (Student existing in students)
            if (string.Equals(existing.StudentId, entry.StudentId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("This student ID is already registered.");
        students.Add(entry); return entry;
    }
    public Staff RegisterStaff(string id, string name, string email)
    {
        Staff entry = new Staff(id, name, email);
        foreach (Staff existing in staff)
            if (string.Equals(existing.StaffId, entry.StaffId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("This staff ID is already registered.");
        staff.Add(entry); return entry;
    }
    public void RemoveStudent(Student student)
    {
        Require(students, student, "Student");
        foreach (LostReport report in reports)
            if (report.Owner == student) throw new InvalidOperationException("This student has a lost report and cannot be removed. Their name is needed for the saved history.");
        foreach (Claim claim in claims)
            if (claim.Student == student) throw new InvalidOperationException("This student has a claim and cannot be removed. Their name is needed for the saved history.");
        students.Remove(student);
    }
    public void RemoveStaff(Staff member)
    {
        Require(staff, member, "Staff");
        foreach (Claim claim in claims)
            if (claim.ReviewedBy == member) throw new InvalidOperationException("This staff member reviewed a claim and cannot be removed. Their name is needed for the saved history.");
        foreach (CollectionRecord record in collections)
            if (record.HandledBy == member) throw new InvalidOperationException("This staff member handled a collection and cannot be removed. Their name is needed for the saved receipt.");
        staff.Remove(member);
    }
    public Location AddLocation(string name, string building)
    {
        Location entry = new Location(locations.Count + 1, name, building);
        foreach (Location existing in locations)
            if (existing.Name.Equals(entry.Name, StringComparison.OrdinalIgnoreCase) && existing.Building.Equals(entry.Building, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("This location already exists.");
        locations.Add(entry); return entry;
    }
    public ItemCategory AddCategory(string name, string description)
    {
        ItemCategory entry = new ItemCategory(categories.Count + 1, name, description);
        foreach (ItemCategory existing in categories)
            if (existing.Name.Equals(entry.Name, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("This category already exists.");
        categories.Add(entry); return entry;
    }
    public FoundItem AddItem(string description, Location location, ItemCategory category, DateTime date, string storage)
    {
        Require(locations, location, "Location"); Require(categories, category, "Category");
        FoundItem item = new FoundItem(items.Count + 1, description, location, category, date, storage);
        items.Add(item); return item;
    }
    public ItemPhoto AddPhoto(FoundItem item, string filePath, string caption)
    {
        Require(items, item, "Item");
        ItemPhoto photo = new ItemPhoto(item.Photos.Count + 1, filePath, caption);
        item.AddPhoto(photo); return photo;
    }
    public LostReport SubmitLostReport(Student student, string description, Location location, DateTime date)
    {
        Require(students, student, "Student"); Require(locations, location, "Location");
        LostReport report = new LostReport(reports.Count + 1, student, description, location, date);
        reports.Add(report); return report;
    }
    public void CloseReport(LostReport report)
    {
        Require(reports, report, "Lost report"); report.CloseReport();
    }
    public List<FoundItem> SearchItems(string keyword)
    {
        List<FoundItem> results = new List<FoundItem>();
        keyword = (keyword ?? "").Trim();
        foreach (FoundItem item in items)
        {
            string text = item.Description + " " + item.Category.Name + " " + item.FoundAt.Name + " " + item.FoundAt.Building;
            if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase)) results.Add(item);
        }
        return results;
    }
    public Claim SubmitClaim(FoundItem item, Student student, string proof, string filePath = "", LostReport? report = null)
    {
        Require(items, item, "Item"); Require(students, student, "Student");
        if (item.Status == ItemStatus.Returned || item.Status == ItemStatus.AwaitingCollection)
            throw new InvalidOperationException("This item is already approved for collection or returned.");
        foreach (Claim existing in item.Claims)
            if (existing.Student == student && existing.Status == ClaimStatus.Pending)
                throw new InvalidOperationException("This student already has a pending claim for this item.");
        if (report != null)
        {
            Require(reports, report, "Lost report");
            if (report.Owner != student || report.IsClosed) throw new InvalidOperationException("Choose an open lost report belonging to the claimant.");
        }
        ClaimEvidence evidence = new ClaimEvidence(1, proof, filePath);
        Claim claim = new Claim(claims.Count + 1, student, item, report);
        claim.AddEvidence(evidence); item.AddClaim(claim); claims.Add(claim);
        Notify(claim, "Claim received for " + item.Description + ". Waiting for staff review.");
        return claim;
    }
    public void AddEvidence(Claim claim, string description, string filePath = "")
    {
        Require(claims, claim, "Claim");
        claim.AddEvidence(new ClaimEvidence(claim.Evidence.Count + 1, description, filePath));
    }
    public void ApproveClaim(Claim claim, Staff reviewer, string note)
    {
        Require(claims, claim, "Claim"); Require(staff, reviewer, "Staff");
        note = Guard.Text(note, "Verification notes");
        if (claim.Item.Status == ItemStatus.Returned || claim.Item.Status == ItemStatus.AwaitingCollection)
            throw new InvalidOperationException("Another claim is already approved, or the item has been returned.");
        claim.Approve(reviewer, note);
        Notify(claim, "Claim approved. Please collect " + claim.Item.Description + " from staff.");
        foreach (Claim rival in claim.Item.Claims)
            if (rival != claim && rival.Status == ClaimStatus.Pending)
            {
                rival.Reject(reviewer, "Another claimant was verified as the owner.");
                Notify(rival, "Claim rejected: another claimant was verified as the owner.");
            }
    }
    public void RejectClaim(Claim claim, Staff reviewer, string reason)
    {
        Require(claims, claim, "Claim"); Require(staff, reviewer, "Staff");
        reason = Guard.Text(reason, "Reason"); claim.Reject(reviewer, reason);
        Notify(claim, "Claim rejected: " + reason);
    }
    public CollectionRecord RecordCollection(Claim claim, Staff handler, string remarks)
    {
        Require(claims, claim, "Claim"); Require(staff, handler, "Staff");
        if (claim.Status != ClaimStatus.Approved) throw new InvalidOperationException("Approve the claim before collection.");
        if (claim.Collection != null) throw new InvalidOperationException("This item has already been collected.");
        CollectionRecord record = new CollectionRecord(collections.Count + 1, claim, handler, remarks);
        claim.SetCollection(record); collections.Add(record);
        if (claim.RelatedReport != null && !claim.RelatedReport.IsClosed) claim.RelatedReport.CloseReport();
        Notify(claim, "Item collected: " + claim.Item.Description + ". Thank you.");
        return record;
    }
    public void MarkNotificationRead(Claim claim, Notification notification)
    {
        Require(claims, claim, "Claim");
        if (!claim.Notifications.Contains(notification)) throw new InvalidOperationException("Notification does not belong to this claim.");
        notification.MarkAsRead();
    }
    private static void Require<T>(List<T> list, T value, string name)
    {
        if (value == null || !list.Contains(value)) throw new InvalidOperationException(name + " does not belong to this workspace.");
    }
    private static void Notify(Claim claim, string text)
    {
        claim.AddNotification(new Notification(claim.Notifications.Count + 1, text));
    }
    public void EnsureDefaultOptions()
    {
        string[,] categoryOptions = new string[,]
        {
            { "Personal Electronics", "Smartphones, laptops, tablets, e-readers." },
            { "Audio & Wearables", "Wireless earbuds, headphones, smartwatches, fitness trackers." },
            { "Power & Cables", "Laptop chargers, USB cables, power banks, universal adapters." },
            { "Wallets & Purses", "Wallets, coin pouches, cardholders, loose cash." },
            { "Identification & Access", "Student matriculation cards, national IDs, Touch 'n Go cards, hostel access cards." },
            { "Keys", "Hostel room keys, motorcycle keys, car keys, locker keys." },
            { "Bags & Luggage", "Backpacks, tote bags, laptop sleeves, gym duffels." },
            { "Academic Textbooks", "Course modules, reference textbooks, library books." },
            { "Notes & Stationery", "Pencil cases, scientific calculators, notebooks, printed lecture notes." },
            { "Lab & Workshop Gear", "Lab coats, safety goggles, drafting tools, engineering measurement kits." },
            { "Outerwear", "Hoodies, jackets, sweaters, cardigans." },
            { "General Clothing", "T-shirts, caps, scarves, hats." },
            { "Eyewear", "Prescription glasses, sunglasses, reading glasses, contact lens cases." },
            { "Drinkware", "Thermal flasks, plastic water bottles, tumblers, protein shakers." },
            { "Food Containers", "Lunch boxes, Tupperware, reusable cutlery sets." },
            { "Umbrellas & Rain Gear", "Folding umbrellas, raincoats, helmet visors." },
            { "Sports & Recreation", "Badminton rackets, sports shoes, gym towels, athletic gear." },
            { "Personal Care", "Makeup pouches, lip balms, hand sanitizers, perfumes." },
            { "Jewelry & Accessories", "Rings, necklaces, earrings, hair clips." },
            { "Miscellaneous", "Any found item that does not fit into the primary categories." }
        };
        for (int i = 0; i < categoryOptions.GetLength(0); i++)
        {
            bool exists = false;
            foreach (ItemCategory category in categories)
                if (category.Name.Equals(categoryOptions[i, 0], StringComparison.OrdinalIgnoreCase)) exists = true;
            if (!exists) AddCategory(categoryOptions[i, 0], categoryOptions[i, 1]);
        }
        string[,] locationOptions = new string[,]
        {
            { "Main Library", "Library Building" },
            { "Student Cafeteria", "Campus dining area" },
            { "Lecture Hall", "Academic area" },
            { "Laboratory", "Academic area" },
            { "Hostel", "Residential area" },
            { "Security Office", "Campus security" },
            { "Sports Centre", "Sports area" },
            { "Parking Area", "Campus parking" },
            { "Other campus location", "Specify the exact place in the item description" }
        };
        for (int i = 0; i < locationOptions.GetLength(0); i++)
        {
            bool exists = false;
            foreach (Location location in locations)
                if (location.Name.Equals(locationOptions[i, 0], StringComparison.OrdinalIgnoreCase)) exists = true;
            if (!exists) AddLocation(locationOptions[i, 0], locationOptions[i, 1]);
        }
    }
    public void LoadDemoData()
    {
        if (students.Count + staff.Count + items.Count + reports.Count + claims.Count + collections.Count != 0)
            throw new InvalidOperationException("Demo records can only be loaded into an empty workspace.");
        EnsureDefaultOptions();
        Student ali = RegisterStudent("S1001", "Ali Rahman", "ali@example.com");
        Student maya = RegisterStudent("S1002", "Maya Tan", "maya@example.com");
        RegisterStaff("ST01", "Hafiz Karim", "hafiz@example.com");
        Location library = locations.First(l => l.Name.Equals("Main Library", StringComparison.OrdinalIgnoreCase));
        Location cafe = locations.First(l => l.Name.Equals("Student Cafeteria", StringComparison.OrdinalIgnoreCase));
        ItemCategory electronics = categories.First(c => c.Name.Equals("Personal Electronics", StringComparison.OrdinalIgnoreCase));
        ItemCategory belongings = categories.First(c => c.Name.Equals("Drinkware", StringComparison.OrdinalIgnoreCase));
        ItemCategory audio = categories.First(c => c.Name.Equals("Audio & Wearables", StringComparison.OrdinalIgnoreCase));
        FoundItem phone = AddItem("Black smartphone with a clear case", library, electronics, DateTime.Today.AddDays(-1), "Security office · Cabinet A1");
        AddItem("Blue water bottle", cafe, belongings, DateTime.Today, "Security office · Shelf B2");
        AddItem("Wireless headphones in grey pouch", library, audio, DateTime.Today, "Security office · Cabinet A2");
        LostReport report = SubmitLostReport(ali, "Black smartphone with a clear case", library, DateTime.Today.AddDays(-2));
        SubmitClaim(phone, ali, "I can unlock the phone and identify the lock-screen photo.", "", report);
        SubmitClaim(phone, maya, "I lost a similar phone and can supply its serial number.");
    }
    internal Claim RestoreClaim(int id, Student student, FoundItem item, LostReport? report)
    {
        if (id != claims.Count + 1) throw new InvalidDataException("Invalid claim sequence.");
        Claim claim = new Claim(id, student, item, report);
        claims.Add(claim); item.AddClaim(claim); return claim;
    }
    internal void RestoreCollection(CollectionRecord record)
    {
        record.ApprovedClaim.SetCollection(record); collections.Add(record);
    }
}
