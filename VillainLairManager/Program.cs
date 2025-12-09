using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using VillainLairManager.Forms;
using VillainLairManager.Services;

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
            
            // Register core services
            services.AddSingleton<IRepository, DatabaseHelper>();
            services.AddSingleton<IMinionService, MinionService>();
            services.AddSingleton<IEvilSchemeService, EvilSchemeService>();
            services.AddSingleton<ISecretBaseService, SecretBaseService>();
            services.AddSingleton<IEquipmentService, EquipmentService>();
            
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
