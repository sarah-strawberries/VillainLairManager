using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillainLairManager.Models;
using VillainLairManager.Utils;

namespace VillainLairManager.Services
{
    public class MinionService(IRepository DatabaseHelper) : IMinionService
    {
        public Dictionary<int, Minion> Minions { get; set; }

        public MinionService() : this(null)
        {
            // Parameterless constructor for testing
        }

        /// <summary>
        /// Initialize the service by loading all minions from the database
        /// </summary>
        public void Initialize()
        {
            var allMinions = DatabaseHelper.GetAllMinions();
            Minions = allMinions.ToDictionary(m => m.MinionId);
        }

        /// <summary>
        /// Factory method to create an initialized MinionService with all minions loaded from database
        /// </summary>
        public static MinionService CreateInitialized(IRepository repository)
        {
            var service = new MinionService(repository);
            var allMinions = repository.GetAllMinions();
            service.Minions = allMinions.ToDictionary(m => m.MinionId);
            return service;
        }

        /// <summary>
        /// Get a minion by ID
        /// </summary>
        public Minion GetMinionById(int minionId)
        {
            if (Minions.TryGetValue(minionId, out var minion))
            {
                return minion;
            }
            return null;
        }

        /// <summary>
        /// Get all minions
        /// </summary>
        public IEnumerable<Minion> GetAllMinions()
        {
            return Minions.Values.ToList();
        }

        /// <summary>
        /// Create a new minion
        /// </summary>
        public Minion CreateMinion(Minion minion)
        {
            DatabaseHelper.InsertMinion(minion);
            Minions[minion.MinionId] = minion;
            return minion;
        }

        /// <summary>
        /// Update an existing minion
        /// </summary>
        public void UpdateMinion(Minion minion)
        {
            if (Minions.ContainsKey(minion.MinionId))
            {
                DatabaseHelper.UpdateMinion(minion);
                Minions[minion.MinionId] = minion;
            }
        }

        /// <summary>
        /// Delete a minion by ID
        /// </summary>
        public void DeleteMinion(int minionId)
        {
            if (Minions.ContainsKey(minionId))
            {
                var minion = Minions[minionId];
                DatabaseHelper.DeleteMinion(minionId);
                Minions.Remove(minionId);
            }
        }

        public void UpdateMood(int minionid)
        {
            var minion = Minions[minionid];
            if (minion.LoyaltyScore > ConfigManager.HighLoyaltyThreshold)
                minion.MoodStatus = ConfigManager.MoodHappy;
            else if (minion.LoyaltyScore < ConfigManager.LowLoyaltyThreshold)
                minion.MoodStatus = ConfigManager.MoodBetrayal;
            else
                minion.MoodStatus = ConfigManager.MoodGrumpy;

            minion.LastMoodUpdate = DateTime.Now;

            DatabaseHelper.UpdateMinion(minion);
        }

        //public static bool IsValidSpecialty(string specialty)
        //{
        //    return ValidationHelper.IsValidSpecialty(specialty);
        //}

        public void UpdateLoyalty(int minionid, decimal actualSalaryPaid)
        {
            var minion = Minions[minionid];

            if (actualSalaryPaid >= minion.SalaryDemand)
            {
                minion.LoyaltyScore += ConfigManager.LoyaltyGrowthRate;
            }
            else
            {
                minion.LoyaltyScore -= ConfigManager.LoyaltyDecayRate;
            }

            // Clamp to valid range
            if (minion.LoyaltyScore > 100) minion.LoyaltyScore = 100;
            if (minion.LoyaltyScore < 0) minion.LoyaltyScore = 0;

            // Update mood based on new loyalty
            UpdateMood(minionid);
        }
    }

}