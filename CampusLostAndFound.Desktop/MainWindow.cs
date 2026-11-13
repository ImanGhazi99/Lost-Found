using System.IO;
using System.Windows.Forms;
using System.Drawing;
using CampusLostAndFound.Library;
namespace CampusLostAndFound.Desktop;
public sealed class MainWindow : System.Windows.Forms.Form
{
    private readonly string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CampusLostAndFound");
    private readonly LostAndFoundManager manager;
    private string page = "Overview";
    private int peopleTab;
    private readonly Panel PageContent = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
    private readonly Label StatusLine = new Label { Dock = DockStyle.Bottom, Height = 30, Padding = new Padding(16,5,0,0) };
    private readonly Label PageTitle = new Label { Dock = DockStyle.Top, Height = 65, Font = new Font("Segoe UI",22,FontStyle.Bold), Padding = new Padding(20,12,0,0) };
    private readonly FlowLayoutPanel Navigation = new FlowLayoutPanel { Dock = DockStyle.Left, Width = 205, FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = Color.FromArgb(18,45,60), Padding = new Padding(12,20,12,0) };
    public MainWindow()
    {
        Text = "Campus Lost and Found | Windows Forms"; Width=1380; Height=880; MinimumSize=new Size(1000,650);
        StartPosition=FormStartPosition.CenterScreen; Font=new Font("Segoe UI",10); BackColor=Color.FromArgb(244,247,249); AutoScaleMode=AutoScaleMode.Dpi;
        Controls.Add(PageContent); Controls.Add(PageTitle); Controls.Add(StatusLine); Controls.Add(Navigation);
        Navigation.Controls.Add(new Label { Text="FOUND /", ForeColor=Color.White, Font=new Font("Segoe UI",24,FontStyle.Bold), Height=70, Width=180 });
        manager=WorkspaceStore.Load(Path.Combine(folder,"workspace.json")); manager.EnsureDefaultOptions(); Save();
        foreach(string name in new[]{"Overview","Found items","Lost reports","Claims","Collections","People & places","Inbox"}) {
            var button=ActionButton(name,()=>ShowPage(name)); button.Width=175; button.Height=44; button.AutoSize=false; Navigation.Controls.Add(button);
        }
        FormClosing+=(_,e)=> { try { Save(); } catch(Exception ex) { MessageBox.Show(this,ex.Message,"Unable to save"); e.Cancel=true; } };
        ShowPage(page);
    }
    private void SetPage(Control control) {
        foreach(Control old in PageContent.Controls.Cast<Control>().ToArray()) old.Dispose();
        PageContent.Controls.Clear(); control.Dock=DockStyle.Fill; PageContent.Controls.Add(control);
    }

    private void Save() { WorkspaceStore.Save(manager,Path.Combine(folder,"workspace.json")); StatusLine.Text = "Saved locally · " + DateTime.Now.ToString("dd MMM yyyy HH:mm"); }
    private void Run(Action action)
    {
        try { action(); } catch (Exception ex) { MessageBox.Show(this,ex.Message,"Please check",MessageBoxButtons.OK,MessageBoxIcon.Information); }
    }
    private void Form(EntryDialog dialog, Action action)
    {
        using(dialog) {
            dialog.OnSave(action);
            if (dialog.ShowDialog(this) == DialogResult.OK) { Run(Save); ShowPage(page); }
        }
    }
    private Button ActionButton(string label,Action action)
    {
        Button button=new Button { Text=label, AutoSize=true, Height=38, Padding=new Padding(10,5,10,5), FlatStyle=FlatStyle.Flat, BackColor=Color.FromArgb(0,115,112), ForeColor=Color.White, Margin=new Padding(0,0,8,8) };
        button.FlatAppearance.BorderSize=0; button.Click+=(_,_)=>Run(action);return button;
    }
    private TableLayoutPanel Panel(string hint,params Button[] buttons)
    {
        var panel=new TableLayoutPanel { Dock=DockStyle.Fill, ColumnCount=1, RowCount=0 };
        var label=new Label { Text=hint, AutoSize=true, MaximumSize=new Size(1050,0), ForeColor=Color.SlateGray, Margin=new Padding(0,0,0,12) };
        panel.Controls.Add(label); var actions=new FlowLayoutPanel { AutoSize=true, Dock=DockStyle.Fill, WrapContents=true };
        actions.Controls.AddRange(buttons); panel.Controls.Add(actions); return panel;
    }
    private void Fill(TableLayoutPanel panel,Control grid) {
        int index=panel.Controls.Count;panel.RowCount=index+1;
        while(panel.RowStyles.Count<index) panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent,100)); grid.Dock=DockStyle.Fill;panel.Controls.Add(grid,0,index);
    }
    private DataGridView Table(string[] headings,IEnumerable<GridRow> rows)
    {
        var grid=new DataGridView { Dock=DockStyle.Fill, ReadOnly=true, AllowUserToAddRows=false, AllowUserToDeleteRows=false, AutoGenerateColumns=false, MultiSelect=false, SelectionMode=DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible=false, BackgroundColor=Color.White, BorderStyle=BorderStyle.None, AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill, EnableHeadersVisualStyles=false };
        grid.ColumnHeadersDefaultCellStyle.BackColor=Color.FromArgb(224,235,239); grid.ColumnHeadersDefaultCellStyle.ForeColor=Color.FromArgb(18,45,60);grid.ColumnHeadersHeight=40;
        grid.DefaultCellStyle.SelectionBackColor=Color.FromArgb(213,237,233);grid.DefaultCellStyle.SelectionForeColor=Color.Black;grid.RowTemplate.Height=35;
        foreach(string heading in headings)grid.Columns.Add(heading,heading);SetRows(grid,rows);return grid;
    }
    private void SetRows(DataGridView grid,IEnumerable<GridRow> rows) { grid.Rows.Clear();foreach(var row in rows)grid.Rows[grid.Rows.Add(row.Cells.Cast<object>().ToArray())].Tag=row;grid.ClearSelection(); }
    private T Selected<T>(DataGridView grid) where T:class { return (grid.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault()?.Tag as GridRow)?.Value as T ?? throw new InvalidOperationException("Select a row first."); }

    private static GridRow Row(object value,params string[] cells) { return new GridRow(value,cells); }
    private void ShowPage(string name)
    {
        page=name;
        foreach(Button nav in Navigation.Controls.OfType<Button>()) nav.BackColor=nav.Text==name ? Color.FromArgb(32,77,91):Color.FromArgb(0,115,112);
        PageTitle.Text=name;

        switch(name)
        {
            case "Overview": Overview(); break;
            case "Found items": Items(); break;
            case "Lost reports": Reports(); break;
            case "Claims": Claims(); break;
            case "Collections": Collections(); break;
            case "People & places": People(); break;
            case "Inbox": Inbox(); break;
        }
    }
    private void Overview()
    {
        var panel=Panel("Register people, record items, verify ownership and complete collection.",ActionButton("+ Record found item",AddItem),ActionButton("+ Lost report",AddReport),ActionButton("Review claims",()=>ShowPage("Claims")));
        var cards=new FlowLayoutPanel { AutoSize=true, Dock=DockStyle.Fill };
        string[] labels={"Found items","Open reports","Pending claims","Returned"};
        int[] counts={manager.Items.Count,manager.Reports.Count(r=>!r.IsClosed),manager.Claims.Count(c=>c.Status==ClaimStatus.Pending),manager.Collections.Count};
        for(int i=0;i<4;i++)cards.Controls.Add(new Label { Text=counts[i]+"\n"+labels[i], Width=185,Height=100,BackColor=Color.White,ForeColor=Color.FromArgb(0,115,112),Font=new Font("Segoe UI",17,FontStyle.Bold),Padding=new Padding(15),Margin=new Padding(0,10,12,20) });
        panel.Controls.Add(cards);
        if(manager.Students.Count==0 && manager.StaffMembers.Count==0 && manager.Items.Count==0)panel.Controls.Add(ActionButton("Load sample workspace",()=> {manager.LoadDemoData();Save();ShowPage(page);}));
        Fill(panel,Table(new[]{"ID","Recent found items","Location","Status"},manager.Items.Reverse().Take(8).Select(i=>Row(i,"FI-"+i.ItemId.ToString("D3"),i.Description,i.FoundAt.Name,i.Status.ToString())))); SetPage(panel);
    }

    private void Items()
    {
        DataGridView grid=Table(new[]{"ID","Description","Category","Location","Status"},Array.Empty<GridRow>());
        TextBox search=new TextBox { Width=300, PlaceholderText="Search description, category or location" };
        ComboBox status=new ComboBox { Width=200, DropDownStyle=ComboBoxStyle.DropDownList };status.Items.AddRange(new[]{"All statuses","Unclaimed","ClaimPending","AwaitingCollection","Returned"});status.SelectedIndex=0;
        void Filter() { SetRows(grid,manager.SearchItems(search.Text).Where(i=>status.SelectedIndex==0 || i.Status.ToString()==status.SelectedItem?.ToString()).Select(i=>Row(i,"FI-"+i.ItemId.ToString("D3"),i.Description,i.Category.Name,i.FoundAt.Name,i.Status.ToString()))); }
        search.TextChanged+=(_,_)=>Filter();status.SelectedIndexChanged+=(_,_)=>Filter();

        TableLayoutPanel panel=Panel("Search by description, category or location. Select a row to view its details.",ActionButton("+ Record item",AddItem),ActionButton("View details",()=>ItemDetails(Selected<FoundItem>(grid))),ActionButton("Add photo",()=>AddPhoto(Selected<FoundItem>(grid))),ActionButton("Start claim",()=>AddClaim(Selected<FoundItem>(grid))));
        var filters=new FlowLayoutPanel { AutoSize=true, Dock=DockStyle.Fill };filters.Controls.Add(search);filters.Controls.Add(status);panel.Controls.Add(filters);Fill(panel,grid); Filter();
        grid.MouseDoubleClick+=(_,_)=>Run(()=>ItemDetails(Selected<FoundItem>(grid))); SetPage(panel);
    }
    private void AddItem()
    {
        EntryDialog d=new EntryDialog(this,"Record a found item");
        d.Hint("Categories and general campus locations are ready to use. Add an exact building or room below when needed.");
        d.Text("description","Item description",true);
        AddLocationChoice(d,"location","Found at");
        ComboBox categories=d.Choice("category","Category",manager.Categories);

        d.Date("date","Date found");d.Text("storage","Storage location / note");
        Form(d,()=>manager.AddItem(d.Value("description"),d.Pick<Location>("location"),d.Pick<ItemCategory>("category"),d.Day("date"),d.Value("storage")));
    }
    private void AddLocationChoice(EntryDialog parent,string key,string label)
    {
        ComboBox choice=parent.Choice(key,label,manager.Locations);
        parent.InlineAction("+ Add an exact location",()=>
        {
            EntryDialog entry=new EntryDialog(parent,"Add campus location");
            entry.Text("name","Location name (for example, Block 17, Room 02-13)");
            entry.Text("building","Building / campus area");
            Location? added=null;
            entry.OnSave(()=>added=manager.AddLocation(entry.Value("name"),entry.Value("building")));
            if(entry.ShowDialog()==DialogResult.OK && added!=null)
            {
                choice.Items.Add(added);choice.SelectedItem=added;Save();
            }
        });
    }

    private string ImportFile(string path,bool image=false)
    {
        if(string.IsNullOrWhiteSpace(path)) return "";
        if(new FileInfo(path).Length>10*1024*1024) throw new ArgumentException("Choose a file smaller than 10 MB.");
        if(image) { using var bitmap=Image.FromFile(path); }
        string relative=Path.Combine("attachments",Guid.NewGuid().ToString("N")+Path.GetExtension(path));Directory.CreateDirectory(Path.Combine(folder,"attachments"));File.Copy(path,Path.Combine(folder,relative));return relative;
    }
    private void AddPhoto(FoundItem item)
    {
        EntryDialog d=new EntryDialog(this,"Add item photo");d.File("file","Photo (maximum 10 MB)",true);d.Text("caption","Caption");
        Form(d,()=> { if(d.Value("file")=="" || d.Value("caption")=="") throw new ArgumentException("Select a photo and enter its caption.");manager.AddPhoto(item,ImportFile(d.Value("file"),true),d.Value("caption")); });
    }
    private void Details(string title,string text,IEnumerable<ItemPhoto>? photos=null)
    {
        using var window=new Form { Text=title, Width=720,Height=680,StartPosition=FormStartPosition.CenterParent,Font=Font };
        var body=new FlowLayoutPanel { Dock=DockStyle.Fill, AutoScroll=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(20) };
        body.Controls.Add(new TextBox { Text=text.Replace("\n",Environment.NewLine), Multiline=true,ReadOnly=true,Width=630,Height=260,ScrollBars=ScrollBars.Vertical,BackColor=Color.White });
        if(photos!=null)foreach(var photo in photos) {
            body.Controls.Add(new Label { Text=photo.Caption,AutoSize=true });
            try { using var original=Image.FromFile(Path.GetFullPath(Path.Combine(folder,photo.FilePath)));var picture=new PictureBox { Image=new Bitmap(original),Width=630,Height=260,SizeMode=PictureBoxSizeMode.Zoom };picture.Disposed+=(_,_)=>picture.Image?.Dispose();body.Controls.Add(picture); }
            catch { body.Controls.Add(new Label {Text="Photo unavailable",AutoSize=true}); }
        }
        window.Controls.Add(body);window.ShowDialog(this);
    }

    private void ItemDetails(FoundItem item) { Details(item.ToString(),$"{item.Description}\n\nCategory: {item.Category.Name}\nFound at: {item.FoundAt}\nDate: {item.DateFound:dd MMM yyyy}\nStorage: {item.StorageNote}\nStatus: {item.Status}\nClaims: {item.Claims.Count}",item.Photos); }
    private void Reports()
    {
        DataGridView grid=Table(new[]{"ID","Description","Student","Last seen","Status"},manager.Reports.Select(r=>Row(r,"LR-"+r.ReportId.ToString("D3"),r.Description,r.Owner.Name,r.LastSeenAt.Name,r.IsClosed ? "Closed":"Open")));
        TableLayoutPanel panel=Panel("Lost reports record what students are looking for. A linked report closes when its approved item is collected.",ActionButton("+ Lost report",AddReport),ActionButton("View report",()=> { LostReport r=Selected<LostReport>(grid);Details(r.ToString(),$"{r.Description}\n\nStudent: {r.Owner}\nLast seen: {r.LastSeenAt}\nDate lost: {r.DateLost:dd MMM yyyy}\nClosed: {r.IsClosed}"); }),ActionButton("Close report",()=> { LostReport r=Selected<LostReport>(grid);if(MessageBox.Show(this,"Close this lost report?","Confirm",MessageBoxButtons.YesNo)==DialogResult.Yes) { manager.CloseReport(r);Save();ShowPage(page); } }));Fill(panel,grid);SetPage(panel);
    }
    private void AddReport()
    {
        EntryDialog d=new EntryDialog(this,"Submit a lost report");d.Choice("student","Student",manager.Students);d.Text("description","Description",true);AddLocationChoice(d,"location","Last seen at");d.Date("date","Date lost");Form(d,()=>manager.SubmitLostReport(d.Pick<Student>("student"),d.Value("description"),d.Pick<Location>("location"),d.Day("date")));
    }
    private void Claims()
    {
        DataGridView grid=Table(new[]{"ID","Item","Student","Decision","Item status"},manager.Claims.Select(c=>Row(c,"CL-"+c.ClaimId.ToString("D3"),c.Item.Description,c.Student.Name,c.Status.ToString(),c.Item.Status.ToString())));
        TableLayoutPanel panel=Panel("Approval rejects competing pending claims. An approved claim stays awaiting collection until staff record the handover.",ActionButton("+ Submit claim",()=>AddClaim()),ActionButton("View evidence",()=> { Claim c=Selected<Claim>(grid);Details(c.ToString(),$"Item: {c.Item}\nStudent: {c.Student}\nStatus: {c.Status}\nReviewer: {c.ReviewedBy}\nDecision: {c.DecisionNote}\n\nEvidence:\n"+string.Join("\n\n",c.Evidence.Select(e=>e.Description+(e.FilePath=="" ? "":"\nAttachment: "+Path.Combine(folder,e.FilePath))))); }),ActionButton("Add evidence",()=> { Claim c=Selected<Claim>(grid);EntryDialog d=new EntryDialog(this,"Add ownership evidence");d.Text("proof","Ownership details",true);d.File("file","Optional attachment",false);Form(d,()=>manager.AddEvidence(c,d.Value("proof"),ImportFile(d.Value("file")))); }),ActionButton("Approve",()=>Review(Selected<Claim>(grid),true)),ActionButton("Reject",()=>Review(Selected<Claim>(grid),false)),ActionButton("Collect",()=>Collect(Selected<Claim>(grid))));Fill(panel,grid);SetPage(panel);
    }
    private void AddClaim(FoundItem? item=null)
    {
        EntryDialog d=new EntryDialog(this,"Submit an ownership claim");ComboBox items=d.Choice("item","Found item",manager.Items.Where(i=>i.Status==ItemStatus.Unclaimed || i.Status==ItemStatus.ClaimPending));if(item!=null)items.SelectedItem=item;
        ComboBox student=d.Choice("student","Claimant",manager.Students);ComboBox report=d.Choice("report","Linked lost report (optional)",Array.Empty<object>(),true);
        void ReportsForStudent() { report.Items.Clear();report.Items.Add("None");foreach(LostReport r in manager.Reports.Where(r=>!r.IsClosed && r.Owner==student.SelectedItem))report.Items.Add(r);report.SelectedIndex=0; }
        student.SelectedIndexChanged+=(_,_)=>ReportsForStudent();ReportsForStudent();d.Text("proof","Proof of ownership",true);d.File("file","Optional supporting file",false);
        Form(d,()=>manager.SubmitClaim(d.Pick<FoundItem>("item"),d.Pick<Student>("student"),d.Value("proof"),ImportFile(d.Value("file")),d.Optional<LostReport>("report")));
    }
    private void Review(Claim claim,bool approve)
    {
        EntryDialog d=new EntryDialog(this,approve ? "Approve claim":"Reject claim");d.Choice("staff","Reviewing staff",manager.StaffMembers);d.Text("note",approve ? "Verification / decision note":"Reason for rejection",true);Form(d,()=> { if(approve)manager.ApproveClaim(claim,d.Pick<Staff>("staff"),d.Value("note"));else manager.RejectClaim(claim,d.Pick<Staff>("staff"),d.Value("note")); });
    }
    private void Collect(Claim claim)
    {
        EntryDialog d=new EntryDialog(this,"Record collection");d.Choice("staff","Handling staff",manager.StaffMembers);d.Text("note","Handover verification (for example, student ID checked)",true);Form(d,()=>manager.RecordCollection(claim,d.Pick<Staff>("staff"),d.Value("note")));
    }
    private void Collections()
    {
        DataGridView grid=Table(new[]{"Receipt","Item","Collected by","Staff","Date"},manager.Collections.Select(c=>Row(c,"CR-"+c.CollectionId.ToString("D3"),c.ApprovedClaim.Item.Description,c.ApprovedClaim.Student.Name,c.HandledBy.Name,c.CollectedAt.ToString("dd MMM yyyy"))));
        TableLayoutPanel panel=Panel("Completed handovers are kept as collection records. Receipts can be viewed and exported.",ActionButton("View receipt",()=> { CollectionRecord c=Selected<CollectionRecord>(grid);Details("Collection receipt",c.ReceiptText()); }),ActionButton("Export receipt",()=> { CollectionRecord c=Selected<CollectionRecord>(grid);SaveFileDialog save=new SaveFileDialog { Filter="Text receipt|*.txt",FileName="Receipt-"+c.CollectionId+".txt" };if(save.ShowDialog(this)==DialogResult.OK)File.WriteAllText(save.FileName,c.ReceiptText()); }));Fill(panel,grid);SetPage(panel);
    }
    private void People()
    {
        TabControl tabs=new TabControl { Dock=DockStyle.Fill };
        void Tab(string title,string[] headings,IEnumerable<GridRow> rows,Action add)
        {
            DataGridView grid=Table(headings,rows);
            List<Button> actions=new List<Button> { ActionButton("+ Add "+title.TrimEnd('s'),add) };
            if(title=="Students" || title=="Staff")
            {
                Button remove=ActionButton("Remove selected",()=>
                {
                    object person=(grid.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault()?.Tag as GridRow)?.Value ?? throw new InvalidOperationException("Select a name first.");
                    if(MessageBox.Show(this,"Remove "+person+"? This permanently removes an unused person record. Linked history is protected.","Confirm removal",MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button2)!=DialogResult.Yes) return;
                    if(person is Student student)manager.RemoveStudent(student);
                    else if(person is Staff member)manager.RemoveStaff(member);
                    Run(Save);ShowPage(page);
                });
                remove.Enabled=false;
                remove.BackColor=Color.FromArgb(167,48,55);
                grid.SelectionChanged+=(_,_)=>remove.Enabled=grid.SelectedRows.Count>0;
                actions.Add(remove);
            }
            TableLayoutPanel p=Panel(title=="Students" || title=="Staff" ? "Select a name to remove it. Names linked to reports, claims or receipts are protected." : "Add records used throughout the workspace.",actions.ToArray());
            p.Padding=new Padding(15);Fill(p,grid);var tab=new TabPage(title);tab.Controls.Add(p);tabs.TabPages.Add(tab);
        }
        Tab("Students",new[]{"ID","Name","Email"},manager.Students.Select(s=>Row(s,s.StudentId,s.Name,s.Email)),()=>AddPerson(false));
        Tab("Staff",new[]{"ID","Name","Email"},manager.StaffMembers.Select(s=>Row(s,s.StaffId,s.Name,s.Email)),()=>AddPerson(true));
        Tab("Locations",new[]{"ID","Name","Building"},manager.Locations.Select(l=>Row(l,l.LocationId.ToString(),l.Name,l.Building)),()=> { EntryDialog d=new EntryDialog(this,"Add location");d.Text("name","Location name");d.Text("building","Building");Form(d,()=>manager.AddLocation(d.Value("name"),d.Value("building"))); });
        Tab("Categories",new[]{"ID","Name","Description"},manager.Categories.Select(c=>Row(c,c.CategoryId.ToString(),c.Name,c.Description)),()=> { EntryDialog d=new EntryDialog(this,"Add category");d.Text("name","Category name");d.Text("description","Description",true);Form(d,()=>manager.AddCategory(d.Value("name"),d.Value("description"))); });
        tabs.SelectedIndex=peopleTab;
        tabs.SelectedIndexChanged+=(_,_)=> { if(tabs.SelectedIndex>=0)peopleTab=tabs.SelectedIndex; };
        SetPage(tabs);
    }
    private void AddPerson(bool staff)
    {
        EntryDialog d=new EntryDialog(this,staff ? "Add staff member":"Add student");d.Text("id",staff ? "Staff ID":"Student ID");d.Text("name","Full name");d.Text("email","Email address");Form(d,()=> { if(staff)manager.RegisterStaff(d.Value("id"),d.Value("name"),d.Value("email"));else manager.RegisterStudent(d.Value("id"),d.Value("name"),d.Value("email")); });
    }
    private void Inbox()
    {
        List<GridRow> rows=new List<GridRow>();foreach(Claim c in manager.Claims)foreach(Notification n in c.Notifications)rows.Add(Row(n,c.Student.Name,n.Message,n.CreatedAt.ToString("dd MMM HH:mm"),n.IsRead ? "Read":"Unread"));
        DataGridView grid=Table(new[]{"Student","Message","Date","Status"},rows);
        TableLayoutPanel panel=Panel("In-app notifications record claim updates. No email is sent.",ActionButton("Read notification",()=> { Notification n=Selected<Notification>(grid);Claim c=manager.Claims.First(c=>c.Notifications.Contains(n));Details("Notification",n.Message);manager.MarkNotificationRead(c,n);Save();ShowPage(page); }),ActionButton("Mark all read",()=> { foreach(Claim c in manager.Claims)foreach(Notification n in c.Notifications)manager.MarkNotificationRead(c,n);Save();ShowPage(page); }));Fill(panel,grid);SetPage(panel);
    }
}
public sealed class GridRow
{
    public object Value { get; }
    public string[] Cells { get; }
    public GridRow(object value,string[] cells) { Value=value;Cells=cells; }
}
