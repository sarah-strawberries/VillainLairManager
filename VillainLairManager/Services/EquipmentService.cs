using System;
using System.Collections.Generic;
using System.Linq;
using VillainLairManager.Models;
using VillainLairManager.Utils;

namespace VillainLairManager.Services
{
    public class EquipmentService : IEquipmentService
    {
        private readonly IRepository DatabaseHelper;
        public Dictionary<int, Equipment> Equipment { get; set; }

        public EquipmentService(IRepository databaseHelper)
        {
            DatabaseHelper = databaseHelper;
            // Initialize cache
            var allEquipment = DatabaseHelper.GetAllEquipment();
            Equipment = allEquipment.ToDictionary(e => e.EquipmentId);
        }

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
                    int monthsSinceMaintenance = 0;
                    if (equipment.LastMaintenanceDate.HasValue)
                    {
                        var now = DateTime.Now;
                        var last = equipment.LastMaintenanceDate.Value;
                        monthsSinceMaintenance = ((now.Year - last.Year) * 12) + now.Month - last.Month;
                        
                        // Ensure we don't get negative values if dates are weird
                        if (monthsSinceMaintenance < 0) monthsSinceMaintenance = 0;
                    }

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
        public decimal PerformMaintenance(int equipmentId, decimal availableFunds)
        {
            // Ensure we have the latest data
            var equipment = DatabaseHelper.GetEquipmentById(equipmentId);
            if (equipment == null) throw new Exception("Equipment not found");
            
            // Update cache
            if (Equipment == null) Equipment = new Dictionary<int, Equipment>();
            Equipment[equipmentId] = equipment;

            if (equipment.Condition >= 100)
            {
                throw new Exception("Equipment is already in perfect condition");
            }

            decimal cost;
            if (equipment.Category == "Doomsday Device")
            {
                cost = equipment.PurchasePrice * ConfigManager.DoomsdayMaintenanceCostPercentage;
            }
            else
            {
                cost = equipment.PurchasePrice * ConfigManager.MaintenanceCostPercentage;
            }

            if (availableFunds < cost)
            {
                throw new Exception("Insufficient funds for maintenance");
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

        public (bool IsValid, string Message) ValidateAssignment(int equipmentId, int schemeId)
        {
            var equipment = Equipment[equipmentId];
            var scheme = DatabaseHelper.GetSchemeById(schemeId);
            
            // Check 1: Condition Requirement
            if (equipment.Condition < ConfigManager.MinEquipmentCondition)
            {
                return (false, "Equipment condition too low for use");
            }

            // Check 2: Storage Location
            if (!equipment.StoredAtBaseId.HasValue)
            {
                return (false, "Equipment must be stored at a base first");
            }

            // Check 3: Not Already Assigned
            if (equipment.AssignedToSchemeId.HasValue)
            {
                var assignedScheme = DatabaseHelper.GetSchemeById(equipment.AssignedToSchemeId.Value);
                if (assignedScheme != null && assignedScheme.Status == ConfigManager.StatusActive)
                {
                    return (false, "Equipment already assigned to another active scheme");
                }
            }

            // Check 4: Specialist Requirement & Doomsday Rules
            int requiredSkill = ConfigManager.SpecialistSkillLevel; // 8
            bool requiresSpecialist = equipment.RequiresSpecialist;

            if (equipment.Category == "Doomsday Device")
            {
                requiresSpecialist = true;
                requiredSkill = 9; // Rule 7

                // Doomsday Storage Check (Warning only)
                var storedBase = DatabaseHelper.GetBaseById(equipment.StoredAtBaseId.Value);
                if (storedBase != null && !storedBase.HasDoomsdayDevice)
                {
                     // Warning: "Base not equipped to store doomsday devices" - but returns Valid per tests
                }

                // Doomsday Rating Check (Warning only)
                if (scheme.DiabolicalRating < 8)
                {
                    // Warning: "Doomsday device overkill for low-rated scheme" - but returns Valid per tests
                }
            }

            if (requiresSpecialist)
            {
                var schemeMinions = DatabaseHelper.GetAllMinions().Where(m => m.CurrentSchemeId == schemeId).ToList();
                
                bool hasSpecialist = schemeMinions.Any(m => m.SkillLevel >= requiredSkill);
                if (!hasSpecialist)
                {
                    return (false, $"Equipment requires a specialist minion (skill {requiredSkill}+)");
                }
            }

            // Check 5: Location Match (Warning only)
            if (scheme.PrimaryBaseId.HasValue && equipment.StoredAtBaseId != scheme.PrimaryBaseId)
            {
                // Warning
            }

            return (true, "Assignment Valid");
        }

        public (bool IsValid, string Message) ValidateEquipment(Equipment equipment)
        {
            // Rule 4: Category Validation
            if (string.IsNullOrEmpty(equipment.Category) || !ConfigManager.ValidCategories.Contains(equipment.Category))
            {
                return (false, "Invalid category");
            }

            // Rule 5: Condition Range Validation
            if (equipment.Condition < 0 || equipment.Condition > 100)
            {
                return (false, "Condition must be between 0 and 100");
            }

            // Rule 6: Cost Validation
            if (equipment.PurchasePrice <= 0)
            {
                return (false, "Purchase price must be greater than zero");
            }
            if (equipment.MaintenanceCost < 0)
            {
                return (false, "Maintenance cost cannot be negative");
            }
            // Warning if MaintenanceCost > PurchasePrice, but valid.

            return (true, "Equipment Valid");
        }

        public void AddEquipment(Equipment equipment)
        {
            var validation = ValidateEquipment(equipment);
            if (!validation.IsValid)
            {
                throw new Exception(validation.Message);
            }
            DatabaseHelper.InsertEquipment(equipment);
            
            // Update cache
            if (Equipment != null)
            {
                var allEquipment = DatabaseHelper.GetAllEquipment();
                Equipment = allEquipment.ToDictionary(e => e.EquipmentId);
            }
        }

        public void UpdateEquipment(Equipment equipment)
        {
            var validation = ValidateEquipment(equipment);
            if (!validation.IsValid)
            {
                throw new Exception(validation.Message);
            }
            DatabaseHelper.UpdateEquipment(equipment);
            
            // Update cache
            if (Equipment != null)
            {
                Equipment[equipment.EquipmentId] = equipment;
            }
        }

        public void DeleteEquipment(int equipmentId)
        {
            var equipment = DatabaseHelper.GetEquipmentById(equipmentId);
            if (equipment == null) return;

            // Handle side effects
            if (equipment.AssignedToSchemeId.HasValue)
            {
                var scheme = DatabaseHelper.GetSchemeById(equipment.AssignedToSchemeId.Value);
                if (scheme != null)
                {
                    // Recalculate success likelihood (-5%)
                    scheme.SuccessLikelihood -= 5;
                    if (scheme.SuccessLikelihood < 0) scheme.SuccessLikelihood = 0;
                    DatabaseHelper.UpdateScheme(scheme);
                }
            }

            DatabaseHelper.DeleteEquipment(equipmentId);
            
            // Update cache
            if (Equipment != null)
            {
                Equipment.Remove(equipmentId);
            }
        }
    }
}
