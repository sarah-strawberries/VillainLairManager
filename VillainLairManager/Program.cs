using System;
using System.Data.SQLite;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using VillainLairManager.Forms;
using VillainLairManager.Services;
using VillainLairManager.Utils;

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

            // Setup Dependency Injection
            var services = new ServiceCollection();
            
            // Register database connection
            var dbConnection = new SQLiteConnection($"Data Source={ConfigManager.DatabasePath};Version=3;");
            services.AddSingleton(dbConnection);
            
            // Register core services
            services.AddSingleton<IRepository>(provider => 
                new DatabaseHelper(provider.GetRequiredService<SQLiteConnection>()));
            services.AddSingleton<IEvilSchemeService, EvilSchemeService>();
            services.AddSingleton<ISecretBaseService, SecretBaseService>();
            services.AddSingleton<IEquipmentService, EquipmentService>();
            services.AddSingleton<IMinionService>(provider => 
                MinionService.CreateInitialized(provider.GetRequiredService<IRepository>()));
            
            // Register Forms
            services.AddTransient<MainForm>();
            services.AddTransient<MinionManagementForm>();
            services.AddTransient<SchemeManagementForm>();
            services.AddTransient<BaseManagementForm>();
            services.AddTransient<EquipmentInventoryForm>();
            
            var serviceProvider = services.BuildServiceProvider();
            
            // Initialize database
            var dbHelper = serviceProvider.GetRequiredService<IRepository>();
            dbHelper.Initialize();
            dbHelper.CreateSchemaIfNotExists();
            dbHelper.SeedInitialData();

            // Run the application
            Application.Run(serviceProvider.GetRequiredService<MainForm>());
        }
    }
}
