using System.Text.Json;

namespace CampusLostAndFound.Library;

public static class WorkspaceStore
{
    public static void Save(LostAndFoundManager manager, string path)
    {
        string json = JsonSerializer.Serialize(new
        {
            Version = 1,
            Students = manager.Students.Select(s => new { Id = s.StudentId, s.Name, s.Email }),
            Staff = manager.StaffMembers.Select(s => new { Id = s.StaffId, s.Name, s.Email }),
            Locations = manager.Locations.Select(l => new { Id = l.LocationId, l.Name, l.Building }),
            Categories = manager.Categories.Select(c => new { Id = c.CategoryId, c.Name, c.Description }),
            Items = manager.Items.Select(i => new { Id = i.ItemId, i.Description, i.DateFound, Location = i.FoundAt.LocationId, Category = i.Category.CategoryId, i.StorageNote, i.Photos }),
            Reports = manager.Reports.Select(r => new { Id = r.ReportId, Owner = r.Owner.StudentId, r.Description, Location = r.LastSeenAt.LocationId, r.DateLost, r.IsClosed }),
            Claims = manager.Claims.Select(c => new { Id = c.ClaimId, Student = c.Student.StudentId, Item = c.Item.ItemId, Report = c.RelatedReport?.ReportId, Status = c.Status.ToString(), c.SubmittedAt, c.ResolvedAt, Reviewer = c.ReviewedBy?.StaffId, c.DecisionNote, c.Evidence, c.Notifications }),
            Collections = manager.Collections.Select(c => new { Id = c.CollectionId, Claim = c.ApprovedClaim.ClaimId, Staff = c.HandledBy.StaffId, c.CollectedAt, c.Remarks })
        }, new JsonSerializerOptions { WriteIndented = true });
        string full = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        string temporary = full + ".tmp";
        File.WriteAllText(temporary, json);
        if (File.Exists(full)) File.Copy(full, full + ".bak", true);
        File.Move(temporary, full, true);
    }
    public static LostAndFoundManager Load(string path)
    {
        if (!File.Exists(path)) return new LostAndFoundManager();
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
        JsonElement root = document.RootElement;
        if (root.GetProperty("Version").GetInt32() != 1) throw new InvalidDataException("Unsupported workspace version.");
        LostAndFoundManager manager = new LostAndFoundManager();
        foreach (JsonElement j in root.GetProperty("Students").EnumerateArray()) manager.RegisterStudent(S(j,"Id"),S(j,"Name"),S(j,"Email"));
        foreach (JsonElement j in root.GetProperty("Staff").EnumerateArray()) manager.RegisterStaff(S(j,"Id"),S(j,"Name"),S(j,"Email"));
        foreach (JsonElement j in root.GetProperty("Locations").EnumerateArray()) Check(manager.AddLocation(S(j,"Name"),S(j,"Building")).LocationId,I(j,"Id"));
        foreach (JsonElement j in root.GetProperty("Categories").EnumerateArray()) Check(manager.AddCategory(S(j,"Name"),S(j,"Description")).CategoryId,I(j,"Id"));
        foreach (JsonElement j in root.GetProperty("Items").EnumerateArray())
        {
            FoundItem item = manager.AddItem(S(j,"Description"),manager.Locations.Single(l=>l.LocationId==I(j,"Location")), manager.Categories.Single(c=>c.CategoryId==I(j,"Category")),j.GetProperty("DateFound").GetDateTime(),S(j,"StorageNote"));
            Check(item.ItemId,I(j,"Id"));
            foreach (JsonElement p in j.GetProperty("Photos").EnumerateArray()) Check(manager.AddPhoto(item,S(p,"FilePath"),S(p,"Caption")).PhotoId,I(p,"PhotoId"));
        }
        foreach (JsonElement j in root.GetProperty("Reports").EnumerateArray())
        {
            LostReport report = manager.SubmitLostReport(manager.Students.Single(s=>s.StudentId==S(j,"Owner")),S(j,"Description"),manager.Locations.Single(l=>l.LocationId==I(j,"Location")),j.GetProperty("DateLost").GetDateTime());
            Check(report.ReportId,I(j,"Id"));
            if (j.GetProperty("IsClosed").GetBoolean()) report.CloseReport();
        }
        foreach (JsonElement j in root.GetProperty("Claims").EnumerateArray())
        {
            LostReport? report = j.GetProperty("Report").ValueKind == JsonValueKind.Null ? null : manager.Reports.Single(r=>r.ReportId==I(j,"Report"));
            Student student = manager.Students.Single(s=>s.StudentId==S(j,"Student"));
            if (report != null && report.Owner != student) throw new InvalidDataException("Claim report belongs to another student.");
            Claim claim = manager.RestoreClaim(I(j,"Id"),student,manager.Items.Single(i=>i.ItemId==I(j,"Item")),report);
            claim.SubmittedAt = j.GetProperty("SubmittedAt").GetDateTime();
            foreach (JsonElement e in j.GetProperty("Evidence").EnumerateArray())
            {
                Check(claim.Evidence.Count + 1,I(e,"EvidenceId"));
                claim.AddEvidence(new ClaimEvidence(I(e,"EvidenceId"),S(e,"Description"),S(e,"FilePath")));
            }
            if (claim.Evidence.Count == 0) throw new InvalidDataException("A claim is missing ownership evidence.");
            string status = S(j,"Status");
            if (status != "Pending")
            {
                Staff reviewer = manager.StaffMembers.Single(s=>s.StaffId==S(j,"Reviewer"));
                string note = Guard.Text(S(j,"DecisionNote"),"Decision note");
                if (status == "Approved") claim.Approve(reviewer,note);
                else if (status == "Rejected") claim.Reject(reviewer,note);
                else throw new InvalidDataException("Invalid claim status.");
                claim.ResolvedAt = j.GetProperty("ResolvedAt").GetDateTime();
            }
            foreach (JsonElement n in j.GetProperty("Notifications").EnumerateArray())
            {
                Check(claim.Notifications.Count + 1,I(n,"NotificationId"));
                Notification notification = new Notification(I(n,"NotificationId"),S(n,"Message"));
                notification.CreatedAt = n.GetProperty("CreatedAt").GetDateTime();
                if (n.GetProperty("IsRead").GetBoolean()) notification.MarkAsRead();
                claim.AddNotification(notification);
            }
        }
        foreach (FoundItem item in manager.Items)
        {
            int approved = item.Claims.Count(c=>c.Status==ClaimStatus.Approved);
            if (approved > 1 || (approved == 1 && item.Claims.Any(c=>c.Status==ClaimStatus.Pending))) throw new InvalidDataException("Conflicting claims in saved data.");
            if (item.Claims.Where(c=>c.Status==ClaimStatus.Pending).GroupBy(c=>c.Student.StudentId).Any(g=>g.Count()>1)) throw new InvalidDataException("Duplicate pending claims in saved data.");
        }
        foreach (JsonElement j in root.GetProperty("Collections").EnumerateArray())
        {
            Check(manager.Collections.Count+1,I(j,"Id"));
            CollectionRecord record = new CollectionRecord(I(j,"Id"),manager.Claims.Single(c=>c.ClaimId==I(j,"Claim")), manager.StaffMembers.Single(s=>s.StaffId==S(j,"Staff")),S(j,"Remarks"));
            record.CollectedAt = j.GetProperty("CollectedAt").GetDateTime();
            manager.RestoreCollection(record);
        }
        return manager;
    }
    private static string S(JsonElement j, string key) { return j.GetProperty(key).GetString() ?? ""; }
    private static int I(JsonElement j, string key) { return j.GetProperty(key).GetInt32(); }
    private static void Check(int actual, int expected)
    {
        if (actual != expected) throw new InvalidDataException("Invalid record sequence.");
    }
}
