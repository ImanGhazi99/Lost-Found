using System.Windows.Forms;
namespace CampusLostAndFound.Desktop;
internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        using var gate=new System.Threading.Mutex(true,"Local\\CampusLostAndFoundDesktop",out bool first);
        if(!first) {MessageBox.Show("Campus Lost and Found is already open.");return;}
        try {Application.Run(new MainWindow());}
        catch(Exception ex) {MessageBox.Show("Unable to open the workspace: "+ex.Message,"Campus Lost and Found",MessageBoxButtons.OK,MessageBoxIcon.Error);}
    }
}
