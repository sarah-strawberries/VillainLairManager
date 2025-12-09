using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillainLairManager.Utils;

namespace VillainLairManager
{
     public static class DIContainer
    {
        public static IRepository _repositoryInstance = new DatabaseHelper(
            new SQLiteConnection($"Data Source={ConfigManager.DatabasePath};Version=3;")
        );
    }
}
