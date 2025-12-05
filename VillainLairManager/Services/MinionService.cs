using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillainLairManager.Models;
using VillainLairManager.Utils;

namespace VillainLairManager.Services
{
    public class MinionService(IRepository DatabaseHelper)
    {
        public Dictionary<int, Minion> Minions { get; set; }

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