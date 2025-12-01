using System;
using System.Windows.Forms;
using VillainLairManager.Forms;

namespace VillainLairManager
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize database - no error handling (anti-pattern)
            DatabaseHelper.Initialize();

            // Create schema if needed
            DatabaseHelper.CreateSchemaIfNotExists();

            // Seed data on first run - no check if already seeded (anti-pattern)
            DatabaseHelper.SeedInitialData();

            Application.Run(new MainForm());
        }
    }
}
