using System;
using VillainLairManager.Utils;

namespace VillainLairManager.Models
{
    /// <summary>
    /// Equipment model with business logic
    /// </summary>
    public class Equipment
    {
        private readonly IRepository _databaseHelper;

        public Equipment(IRepository databaseHelper)
        {
            _databaseHelper = databaseHelper;
        }
        public int EquipmentId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public int Condition { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal MaintenanceCost { get; set; }
        public int? AssignedToSchemeId { get; set; }
        public int? StoredAtBaseId { get; set; }
        public bool RequiresSpecialist { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }

        // Business logic: condition degradation
        public void DegradeCondition()
        {
            if (AssignedToSchemeId.HasValue)
            {
                // Check if scheme is active
                var scheme = _databaseHelper.GetSchemeById(AssignedToSchemeId.Value);
                if (scheme != null && scheme.Status == ConfigManager.StatusActive)
                {
                    int monthsSinceMaintenance = 1; // Simplified - should calculate from LastMaintenanceDate
                    int degradation = monthsSinceMaintenance * ConfigManager.ConditionDegradationRate;
                    Condition -= degradation;

                    if (Condition < 0) Condition = 0;

                    _databaseHelper.UpdateEquipment(this);
                }
            }
        }

        // Perform maintenance
        public decimal PerformMaintenance()
        {
            decimal cost;
            if (Category == "Doomsday Device")
            {
                cost = PurchasePrice * ConfigManager.DoomsdayMaintenanceCostPercentage;
            }
            else
            {
                cost = PurchasePrice * ConfigManager.MaintenanceCostPercentage;
            }

            Condition = 100;
            LastMaintenanceDate = DateTime.Now;

            _databaseHelper.UpdateEquipment(this);

            return cost;
        }

        // Check if operational
        public bool IsOperational()
        {
            return Condition >= ConfigManager.MinEquipmentCondition;
        }

        public bool IsBroken()
        {
            return Condition < ConfigManager.BrokenEquipmentCondition;
        }

        // ToString for display
        public override string ToString()
        {
            return $"{Name} ({Category}, Condition: {Condition}%)";
        }
    }
}
