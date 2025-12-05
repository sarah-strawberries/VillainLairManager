using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VillainLairManager
{
     public static class DIContainer
    {
        public static IRepository _repositoryInstance = new DatabaseHelper();
    }
}
