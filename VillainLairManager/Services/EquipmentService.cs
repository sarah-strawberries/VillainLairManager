using System;
using System.Collections.Generic;
using VillainLairManager.Models;
using VillainLairManager.Utils;

namespace VillainLairManager.Services
{
    public class EquipmentService(IRepository DatabaseHelper) : IEquipmentService
    {
        public Dictionary<int, Equipment> Equipment { get; set; }

        /// <summary>
        /// Degrades equipment condition based on usage and maintenance status
        /// </summary>
        public void DegradeCondition(int equipmentId)
        {
            var equipment = Equipment[equipmentId];

            if (equipment.AssignedToSchemeId.HasValue)
            {
                var scheme = DatabaseHelper.GetSchemeById(equipment.AssignedToSchemeId.Value);
                if (scheme != null && scheme.Status == ConfigManager.StatusActive)
                {
                    int monthsSinceMaintenance = 1; // Simplified - should calculate from LastMaintenanceDate
                    int degradation = monthsSinceMaintenance * ConfigManager.ConditionDegradationRate;
                    equipment.Condition -= degradation;

                    if (equipment.Condition < 0) equipment.Condition = 0;

                    DatabaseHelper.UpdateEquipment(equipment);
                }
            }
        }

        /// <summary>
        /// Performs maintenance on equipment and returns the cost
        /// </summary>
        public decimal PerformMaintenance(int equipmentId)
        {
            var equipment = Equipment[equipmentId];

            decimal cost;
            if (equipment.Category == "Doomsday Device")
            {
                cost = equipment.PurchasePrice * ConfigManager.DoomsdayMaintenanceCostPercentage;
            }
            else
            {
                cost = equipment.PurchasePrice * ConfigManager.MaintenanceCostPercentage;
            }

            equipment.Condition = 100;
            equipment.LastMaintenanceDate = DateTime.Now;

            DatabaseHelper.UpdateEquipment(equipment);

            return cost;
        }

        /// <summary>
        /// Checks if equipment is operational (condition meets minimum threshold)
        /// </summary>
        public bool IsOperational(int equipmentId)
        {
            var equipment = Equipment[equipmentId];
            return equipment.Condition >= ConfigManager.MinEquipmentCondition;
        }

        /// <summary>
        /// Checks if equipment is broken (condition below broken threshold)
        /// </summary>
        public bool IsBroken(int equipmentId)
        {
            var equipment = Equipment[equipmentId];
            return equipment.Condition < ConfigManager.BrokenEquipmentCondition;
        }
    }
}
