using CampusLostAndFound.Library;

var manager = new LostAndFoundManager();

// ---- Set up people, places and categories ----
var library   = new Location(1, "Main Library", "Library Building");
var cafeteria = new Location(2, "Student Cafeteria", "Block C");
var electronics = new ItemCategory(1, "Electronics") { Description = "Phones, chargers, laptops" };
var clothing    = new ItemCategory(2, "Clothing")    { Description = "Jackets, hoodies, scarves" };
var aisyah = new Student("S1001", "Aisyah Rahman", "aisyah@example.com");
var daniel = new Student("S1002", "Daniel Lim", "daniel@example.com");
var hafiz  = new Staff("ST01", "Hafiz Karim", "hafiz@example.com");

Heading("People, places and categories");
aisyah.Display(); daniel.Display(); hafiz.Display();
library.Display(); cafeteria.Display();
electronics.Display(); clothing.Display();

// ---- A student reports a lost phone ----
Heading("Lost report");
var report = new LostReport(1, aisyah)
{
    Description = "Black smartphone with cracked case",
    LastSeenAt = library,
    DateLost = DateTime.Today.AddDays(-2)
};
Console.WriteLine($"Report #{report.ReportId} by {report.Owner.Name}: {report.Description} " +
                  $"(last seen at {report.LastSeenAt?.Name})");

// ---- Staff record found items ----
var phone = new FoundItem(1, "Black smartphone with cracked case")
{ DateFound = DateTime.Today.AddDays(-1), FoundAt = library, Category = electronics };
phone.AddPhoto(new ItemPhoto(1, "photos/phone-front.jpg") { Caption = "Front of the phone" });

var hoodie = new FoundItem(2, "Grey hoodie, size M")
{ DateFound = DateTime.Today.AddDays(-1), FoundAt = cafeteria, Category = clothing };

manager.AddItem(phone);
manager.AddItem(hoodie);

Heading("Search results");
Console.WriteLine($"\"library\": {manager.SearchItems("library").Count} item(s)");
Console.WriteLine($"\"clothing\": {manager.SearchItems("clothing").Count} item(s)");
Console.WriteLine($"All items: {manager.SearchItems("").Count}");

// ---- Students submit claims; two students want the same phone ----
Heading("Claims");
var claim1 = manager.SubmitClaim(phone, aisyah);
claim1.AddEvidence(new ClaimEvidence(1, "Can unlock the phone with her PIN") { FilePath = "evidence/unlock.jpg" });
var claim2 = manager.SubmitClaim(phone, daniel);
claim2.AddEvidence(new ClaimEvidence(2, "Says he has a purchase receipt"));
var claim3 = manager.SubmitClaim(hoodie, daniel);
claim3.AddEvidence(new ClaimEvidence(3, "Name tag inside the collar"));

Console.WriteLine($"Phone: {phone.Status} ({phone.Claims.Count} claims)");
Console.WriteLine($"Hoodie: {hoodie.Status} ({hoodie.Claims.Count} claim)");

// ---- Staff review the evidence ----
Heading("Review");
manager.ApproveClaim(claim1);   // also rejects Daniel's competing claim
claim3.Reject();                // evidence could not be verified
Console.WriteLine($"Claim 1 (Aisyah, phone): {claim1.Status}");
Console.WriteLine($"Claim 2 (Daniel, phone): {claim2.Status}");
Console.WriteLine($"Claim 3 (Daniel, hoodie): {claim3.Status}");
Console.WriteLine($"Phone: {phone.Status} (approved, waiting for collection)");
Console.WriteLine($"Hoodie: {hoodie.Status}");

// ---- Collection ----
Heading("Collection");
var receipt = manager.RecordCollection(claim1, hafiz);
receipt.Remarks = "Student ID card checked.";
receipt.DisplayReceipt();
Console.WriteLine($"Phone: {phone.Status}");

// ---- Rules that stop invalid actions ----
Heading("Blocked actions");
Try(() => manager.SubmitClaim(phone, daniel));
Try(() => manager.RecordCollection(claim1, hafiz));
Try(() => claim2.Approve());

// ---- Notifications (delivery is simulated by printing) ----
Heading("Notifications for Aisyah's claim");
foreach (var n in claim1.Notifications)
{
    Console.WriteLine($"[#{n.NotificationId}] {n.Message}");
    n.MarkAsRead();
}
Console.WriteLine($"Unread: {claim1.Notifications.Count(n => !n.IsRead)}");

// ---- Close the lost report ----
Heading("Summary");
report.CloseReport();
Console.WriteLine($"Lost report #{report.ReportId} closed: {report.IsClosed}");
Console.WriteLine($"Claims: {phone.Claims.Count + hoodie.Claims.Count} | " +
                  $"Returned items: {manager.SearchItems("").Count(i => i.Status == ItemStatus.Returned)}");

static void Heading(string text)
{
    Console.WriteLine();
    Console.WriteLine($"--- {text} ---");
}

static void Try(Action action)
{
    try { action(); Console.WriteLine("No error"); }
    catch (InvalidOperationException ex) { Console.WriteLine($"Blocked: {ex.Message}"); }
}
