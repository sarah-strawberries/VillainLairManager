using System;
using System.Collections.Generic;
using VillainLairManager.Models;

namespace VillainLairManager.Services
{
    public class SecretBaseService(IRepository DatabaseHelper) : ISecretBaseService
    {
        public Dictionary<int, SecretBase> Bases { get; set; }

        /// <summary>
        /// Gets the current occupancy of a base
        /// </summary>
        public int GetCurrentOccupancy(int baseId)
        {
            return DatabaseHelper.GetBaseOccupancy(baseId);
        }

        /// <summary>
        /// Gets the available capacity remaining in a base
        /// </summary>
        public int GetAvailableCapacity(int baseId)
        {
            var secretBase = Bases[baseId];
            return secretBase.Capacity - GetCurrentOccupancy(baseId);
        }

        /// <summary>
        /// Checks if a base can accommodate an additional minion
        /// </summary>
        public bool CanAccommodateMinion(int baseId)
        {
            return GetCurrentOccupancy(baseId) < Bases[baseId].Capacity;
        }

        /// <summary>
        /// Gets the occupancy percentage of a base
        /// </summary>
        public decimal GetOccupancyPercentage(int baseId)
        {
            var secretBase = Bases[baseId];
            if (secretBase.Capacity == 0) return 0;
            return (GetCurrentOccupancy(baseId) / (decimal)secretBase.Capacity) * 100;
        }

        /// <summary>
        /// Checks if a base is at full capacity
        /// </summary>
        public bool IsAtFullCapacity(int baseId)
        {
            return GetCurrentOccupancy(baseId) >= Bases[baseId].Capacity;
        }
    }
}
