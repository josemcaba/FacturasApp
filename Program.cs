using FacturasApp.Services;
using FacturasApp.UI;

namespace FacturasApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>

        // The [STAThread] attribute indicates that the COM threading model
        // for the application is single-threaded apartment.
        // This is necessary for Windows Forms applications to ensure that
        // the UI components are accessed from the main thread.
        [STAThread]
        
        // The Main method is the entry point of the application.
        // It initializes the application configuration and starts the main form.
        static void Main() 
        {
            // The ApplicationConfiguration.Initialize() method is used
            // to set up the application configuration, such as high DPI settings and default font.
            ApplicationConfiguration.Initialize();
            
            // The Application.Run(new MainForm()) method starts
            // the application and opens the main form of the application.
            Application.Run(new MainForm());
            
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
        }
    }
}
