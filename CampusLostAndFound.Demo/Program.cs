using CampusLostAndFound.Library;
LostAndFoundManager manager = new LostAndFoundManager();
manager.LoadDemoData();
Console.WriteLine("CAMPUS LOST AND FOUND | APPLICATION DEMONSTRATION");
Console.WriteLine("\n--- People, places and categories ---");
foreach (Student student in manager.Students) student.Display();
foreach (Staff staff in manager.StaffMembers) staff.Display();
foreach (Location location in manager.Locations) location.Display();
foreach (ItemCategory category in manager.Categories) category.Display();
Console.WriteLine("\n--- Search ---");
Console.WriteLine("Library results: " + manager.SearchItems("library").Count);
Console.WriteLine("Total found items: " + manager.Items.Count);
Claim claim = manager.Claims[0];
Staff reviewer = manager.StaffMembers[0];
Console.WriteLine("\n--- Claims ---");
Console.WriteLine("Phone before review: " + claim.Item.Status);
manager.ApproveClaim(claim, reviewer, "Ownership verified by unlocking the phone.");
foreach (Claim entry in manager.Claims) Console.WriteLine(entry + ": " + entry.Status);
Console.WriteLine("Phone after approval: " + claim.Item.Status);
CollectionRecord receipt = manager.RecordCollection(claim, reviewer, "Student ID card checked.");
Console.WriteLine("\n--- Collection ---");
receipt.DisplayReceipt();
Console.WriteLine("Phone after collection: " + claim.Item.Status);
Console.WriteLine("Linked report closed: " + claim.RelatedReport!.IsClosed);
Console.WriteLine("\n--- Blocked actions ---");
try { manager.RecordCollection(claim,reviewer,"ID checked"); } catch (InvalidOperationException ex) { Console.WriteLine("Blocked: " + ex.Message); }
try { manager.SubmitClaim(claim.Item,manager.Students[1],"Proof"); } catch (InvalidOperationException ex) { Console.WriteLine("Blocked: " + ex.Message); }
Console.WriteLine("\n--- Notifications ---");
foreach (Notification notification in claim.Notifications)
{
    Console.WriteLine(notification.Message);
    manager.MarkNotificationRead(claim,notification);
}
Console.WriteLine("Unread notifications: " + claim.Notifications.Count(n => !n.IsRead));
Console.WriteLine("Claims: " + manager.Claims.Count + " | Returned items: " + manager.Collections.Count);
