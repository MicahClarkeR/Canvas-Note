using System.Configuration;
using System.Data;
using System.Windows;

namespace Canvas_Note_Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                string filePath = e.Args[0];
                // Open the file
                MainWindow mainWindow = new MainWindow();
                mainWindow.LoadFile(filePath); // Implement the OpenFile method in MainWindow
                mainWindow.Show();
            }
            else
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
        }
    }

}
