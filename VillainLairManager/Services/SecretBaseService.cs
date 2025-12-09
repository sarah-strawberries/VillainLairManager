using System;
using System.Collections.Generic;
using System.Linq;
using VillainLairManager.Models;

namespace VillainLairManager.Services
{
    public class SecretBaseService(IRepository DatabaseHelper) : ISecretBaseService
    {
        public Dictionary<int, SecretBase> Bases { get; set; } = new Dictionary<int, SecretBase>();

        /// <summary>
        /// Initializes the Bases cache from the database
        /// </summary>
        public void InitializeBases()
        {
            Bases.Clear();
            var bases = DatabaseHelper.GetAllBases();
            foreach (var base_ in bases)
            {
                Bases[base_.BaseId] = base_;
            }
        }

        /// <summary>
        /// Gets the current number of minions stationed at a base
        /// BR-B-001: Cannot exceed base capacity
        /// </summary>
        public int GetCurrentOccupancy(int baseId)
        {
            var minions = DatabaseHelper.GetAllMinions();
            return minions.Count(m => m.CurrentBaseId == baseId);
        }

        /// <summary>
        /// Gets the available capacity at a base
        /// </summary>
        public int GetAvailableCapacity(int baseId)
        {
            if (!Bases.ContainsKey(baseId))
                return 0;

            var base_ = Bases[baseId];
            int occupancy = GetCurrentOccupancy(baseId);
            return base_.Capacity - occupancy;
        }

        /// <summary>
        /// Checks if a minion can be assigned to a base
        /// BR-B-001: Cannot exceed base capacity
        /// </summary>
        public (bool canAssign, List<string> errors) CanAssignMinion(int baseId, int minionId)
        {
            var errors = new List<string>();
            
            if (!Bases.ContainsKey(baseId))
            {
                errors.Add("Base not found");
                return (false, errors);
            }

            var minions = DatabaseHelper.GetAllMinions();
            var minion = minions.FirstOrDefault(m => m.MinionId == minionId);

            if (minion == null)
            {
                errors.Add("Minion not found");
                return (false, errors);
            }

            var base_ = Bases[baseId];

            if (minion.CurrentBaseId == baseId)
                errors.Add("Minion is already at this base");

            int occupancy = GetCurrentOccupancy(baseId);
            if (occupancy >= base_.Capacity)
                errors.Add($"Base is at full capacity ({base_.Capacity} minions)");

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// Checks if a base can accommodate an additional minion
        /// </summary>
        public bool CanAccommodateMinion(int baseId)
        {
            return GetAvailableCapacity(baseId) > 0;
        }

        /// <summary>
        /// Gets the occupancy percentage of a base
        /// </summary>
        public decimal GetOccupancyPercentage(int baseId)
        {
            if (!Bases.ContainsKey(baseId))
                return 0;

            var base_ = Bases[baseId];
            if (base_.Capacity == 0)
                return 0;

            int occupancy = GetCurrentOccupancy(baseId);
            return (occupancy / (decimal)base_.Capacity) * 100;
        }

        /// <summary>
        /// Checks if a base is at full capacity
        /// </summary>
        public bool IsAtFullCapacity(int baseId)
        {
            return GetAvailableCapacity(baseId) <= 0;
        }

        /// <summary>
        /// Marks a base as discovered
        /// BR-B-002: Discovered bases need evacuation alerts
        /// </summary>
        public void MarkDiscovered(int baseId, DateTime discoveryDate)
        {
            if (!Bases.ContainsKey(baseId))
                return;

            var base_ = Bases[baseId];
            base_.IsDiscovered = true;
            base_.LastInspectionDate = discoveryDate;
            DatabaseHelper.UpdateBase(base_);
        }

        /// <summary>
        /// Marks a base as safe (cleared from discovery)
        /// </summary>
        public void MarkSafe(int baseId)
        {
            if (!Bases.ContainsKey(baseId))
                return;

            var base_ = Bases[baseId];
            base_.IsDiscovered = false;
            DatabaseHelper.UpdateBase(base_);
        }

        /// <summary>
        /// Gets the security status of a base
        /// </summary>
        public string GetSecurityStatus(int baseId)
        {
            if (!Bases.ContainsKey(baseId))
                return "Unknown";

            var base_ = Bases[baseId];

            if (!base_.IsDiscovered)
                return "Safe";

            if (base_.LastInspectionDate.HasValue)
            {
                int daysSinceDiscovery = (int)(DateTime.Now - base_.LastInspectionDate.Value).TotalDays;
                if (daysSinceDiscovery < 7)
                    return "Recently Discovered - Urgent Evacuation";
            }

            return "Discovered";
        }

        /// <summary>
        /// Checks if equipment can be stored at a base
        /// BR-B-003: Equipment can only be stored once
        /// </summary>
        public (bool canStore, List<string> errors) CanStoreEquipment(int baseId, int equipmentId)
        {
            var errors = new List<string>();
            
            if (!Bases.ContainsKey(baseId))
            {
                errors.Add("Base not found");
                return (false, errors);
            }

            var equipment = DatabaseHelper.GetAllEquipment().FirstOrDefault(e => e.EquipmentId == equipmentId);

            if (equipment == null)
            {
                errors.Add("Equipment not found");
                return (false, errors);
            }

            if (equipment.Condition < 50)
                errors.Add($"Equipment condition too low ({equipment.Condition}%) - must be at least 50%");

            if (equipment.StoredAtBaseId == baseId)
                errors.Add("Equipment is already stored at this base");

            if (equipment.StoredAtBaseId.HasValue && equipment.StoredAtBaseId != baseId)
                errors.Add("Equipment is already stored at another base");

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// Gets all equipment currently stored at a base
        /// </summary>
        public List<Equipment> GetStoredEquipment(int baseId)
        {
            var equipment = DatabaseHelper.GetAllEquipment();
            return equipment.Where(e => e.StoredAtBaseId == baseId).ToList();
        }

        /// <summary>
        /// Gets approximate storage space availability
        /// </summary>
        public int GetAvailableStorageSpace(int baseId)
        {
            if (!Bases.ContainsKey(baseId))
                return 0;

            // Arbitrary storage metric: 10 items per capacity unit
            var base_ = Bases[baseId];
            int stored = GetStoredEquipment(baseId).Count;
            int maxStorage = base_.Capacity * 2;
            return Math.Max(0, maxStorage - stored);
        }

        /// <summary>
        /// Calculates monthly costs for a base (maintenance + minion salaries)
        /// BR-B-006: Monthly costs calculated properly
        /// </summary>
        public decimal CalculateMonthlyCosts(int baseId)
        {
            if (!Bases.ContainsKey(baseId))
                return 0;

            var base_ = Bases[baseId];
            decimal maintenanceCost = base_.MonthlyMaintenanceCost;

            // Add minion salaries
            var minions = DatabaseHelper.GetAllMinions();
            var basedMinions = minions.Where(m => m.CurrentBaseId == baseId).ToList();
            decimal minionCosts = (decimal)basedMinions.Sum(m => m.SalaryDemand);

            return maintenanceCost + minionCosts;
        }

        /// <summary>
        /// Gets cost trend information
        /// </summary>
        public string GetCostTrend(int baseId)
        {
            if (!Bases.ContainsKey(baseId))
                return "Unknown";

            var base_ = Bases[baseId];
            var minions = DatabaseHelper.GetAllMinions();
            int minionCount = minions.Count(m => m.CurrentBaseId == baseId);

            // Simple trend: if minions are high and loyalty is low, costs increasing
            var basedMinions = minions.Where(m => m.CurrentBaseId == baseId).ToList();
            decimal avgLoyalty = basedMinions.Count > 0 ? (decimal)basedMinions.Average(m => m.LoyaltyScore) : 100;

            if (minionCount >= base_.Capacity * 0.8 && avgLoyalty < 50)
                return "Increasing (High occupancy + Low morale)";

            if (minionCount == 0)
                return "Minimal";

            return "Stable";
        }

        /// <summary>
        /// Gets a detailed summary of a base
        /// </summary>
        public string GetBaseSummary(int baseId)
        {
            if (!Bases.ContainsKey(baseId))
                return "Base not found";

            var base_ = Bases[baseId];
            int occupancy = GetCurrentOccupancy(baseId);
            int availableSpace = GetAvailableCapacity(baseId);
            decimal monthlyCost = CalculateMonthlyCosts(baseId);
            string securityStatus = GetSecurityStatus(baseId);
            var storedEquipment = GetStoredEquipment(baseId);

            return $@"
Base: {base_.Name}
Location: {base_.Location}
Security Level: {base_.SecurityLevel}/10
Occupancy: {occupancy}/{base_.Capacity} ({availableSpace} available)
Doomsday Device: {(base_.HasDoomsdayDevice ? "YES" : "No")}
Discovery Status: {securityStatus}
Monthly Costs: ${monthlyCost:N2}
Stored Equipment: {storedEquipment.Count} items
            ".Trim();
        }
    }
}
