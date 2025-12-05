using System;

namespace VillainLairManager.Models
{
    /// <summary>
    /// Secret Base model
    /// </summary>
    public class SecretBase
    {
        private readonly IRepository _databaseHelper;

        public SecretBase(IRepository databaseHelper)
        {
            _databaseHelper = databaseHelper;
        }

        public int BaseId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public int Capacity { get; set; }
        public int SecurityLevel { get; set; }
        public decimal MonthlyMaintenanceCost { get; set; }
        public bool HasDoomsdayDevice { get; set; }
        public bool IsDiscovered { get; set; }
        public DateTime? LastInspectionDate { get; set; }

        // Business logic in model
        public int GetCurrentOccupancy()
        {
            // Directly calls database (anti-pattern)
            return _databaseHelper.GetBaseOccupancy(this.BaseId);
        }

        public int GetAvailableCapacity()
        {
            return Capacity - GetCurrentOccupancy();
        }

        public bool CanAccommodateMinion()
        {
            return GetCurrentOccupancy() < Capacity;
        }

        // ToString for display
        public override string ToString()
        {
            return $"{Name} ({Location}, Capacity: {Capacity})";
        }
    }
}
