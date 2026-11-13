using System.Windows.Forms;
using System.Drawing;
namespace CampusLostAndFound.Desktop;
public sealed class EntryDialog : System.Windows.Forms.Form
{
    private readonly TableLayoutPanel fields=new() { Dock=DockStyle.Top,AutoSize=true,ColumnCount=1,Padding=new Padding(20) };
    private readonly Dictionary<string,Control> inputs=new();
    private readonly Label error=new() { AutoSize=true,ForeColor=Color.Firebrick,MaximumSize=new Size(500,0) };
    private readonly Button save=new() { Text="Save record",AutoSize=true,Padding=new Padding(12,6,12,6),BackColor=Color.Teal,ForeColor=Color.White };
    public EntryDialog(System.Windows.Forms.Form owner,string title)
    {
        Owner=owner;base.Text=title;Width=610;Height=730;MinimumSize=new Size(550,450);StartPosition=FormStartPosition.CenterParent;ShowInTaskbar=false;Font=new Font("Segoe UI",10);AutoScaleMode=AutoScaleMode.Dpi;
        var scroll=new Panel {Dock=DockStyle.Fill,AutoScroll=true};scroll.Controls.Add(fields);
        var footer=new FlowLayoutPanel {Dock=DockStyle.Bottom,Height=105,Padding=new Padding(20),FlowDirection=FlowDirection.TopDown,WrapContents=false};
        footer.Controls.Add(error);var buttons=new FlowLayoutPanel {AutoSize=true};
        var cancel=new Button {Text="Cancel",AutoSize=true,Padding=new Padding(12,6,12,6),DialogResult=DialogResult.Cancel};buttons.Controls.Add(cancel);buttons.Controls.Add(save);footer.Controls.Add(buttons);
        Controls.Add(scroll);Controls.Add(footer);CancelButton=cancel;
    }
    public new TextBox Text(string key,string label,bool multiline=false,string value="")
    {
        var input=new TextBox {Text=value,Multiline=multiline,AcceptsReturn=multiline,Height=multiline?85:30,ScrollBars=multiline?ScrollBars.Vertical:ScrollBars.None};Add(key,label,input);return input;
    }
    public ComboBox Choice(string key,string label,IEnumerable<object> options,bool optional=false)
    {
        var box=new ComboBox {DropDownStyle=ComboBoxStyle.DropDownList,DropDownWidth=500};if(optional)box.Items.Add("None");foreach(var option in options)box.Items.Add(option);if(box.Items.Count>0)box.SelectedIndex=0;Add(key,label,box);return box;
    }
    public void Date(string key,string label) {Add(key,label,new DateTimePicker {Format=DateTimePickerFormat.Short,Value=DateTime.Today,MaxDate=DateTime.Today});}
    public void File(string key,string label,bool image)
    {
        var box=Text(key,label);box.ReadOnly=true;InlineAction("Choose file…",()=> {using var dialog=new OpenFileDialog {Filter=image?"Images|*.jpg;*.jpeg;*.png;*.bmp":"Supporting files|*.pdf;*.jpg;*.jpeg;*.png;*.txt"};if(dialog.ShowDialog(this)==DialogResult.OK)box.Text=dialog.FileName;});
    }
    public void InlineAction(string label,Action action) {var b=new Button {Text=label,AutoSize=true,Margin=new Padding(0,0,0,15)};b.Click+=(_,_)=> {try{action();}catch(Exception ex){error.Text=ex.Message;}};fields.Controls.Add(b);}
    public void Hint(string text) {fields.Controls.Add(new Label {Text=text,AutoSize=true,MaximumSize=new Size(500,0),ForeColor=Color.SlateGray,Margin=new Padding(0,0,0,15)});}
    private void Add(string key,string label,Control input) {fields.Controls.Add(new Label {Text=label,AutoSize=true,Margin=new Padding(0,0,0,5)});input.Width=500;input.Margin=new Padding(0,0,0,15);fields.Controls.Add(input);inputs[key]=input;}
    public string Value(string key)=>((TextBox)inputs[key]).Text.Trim();
    public T Pick<T>(string key) where T:class=>((ComboBox)inputs[key]).SelectedItem as T??throw new InvalidOperationException("Select "+key+" first. Add a record in People & places if the list is empty.");
    public T? Optional<T>(string key) where T:class=>((ComboBox)inputs[key]).SelectedItem as T;
    public DateTime Day(string key)=>((DateTimePicker)inputs[key]).Value.Date;
    public void OnSave(Action action,string label="Save record") {save.Text=label;save.Click+=(_,_)=> {try {action();DialogResult=DialogResult.OK;}catch(Exception ex){error.Text=ex.Message;}};}
}
