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
            var dbHelper = new DatabaseHelper();
            dbHelper.Initialize();

            // Create schema if needed
            dbHelper.CreateSchemaIfNotExists();

            // Seed data on first run - no check if already seeded (anti-pattern)
            dbHelper.SeedInitialData();

            Application.Run(new MainForm(dbHelper));
        }
    }
}
