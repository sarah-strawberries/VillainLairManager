using System;
using VillainLairManager.Utils;

namespace VillainLairManager.Models
{
    /// <summary>
    /// Minion model with business logic mixed in (anti-pattern)
    /// </summary>
    public class Minion
    {
        public int MinionId { get; set; }
        public string Name { get; set; }
        public int SkillLevel { get; set; }
        public string Specialty { get; set; }
        public int LoyaltyScore { get; set; }
        public decimal SalaryDemand { get; set; }
        public int? CurrentBaseId { get; set; }
        public int? CurrentSchemeId { get; set; }
        public string MoodStatus { get; set; }
        public DateTime LastMoodUpdate { get; set; }
        public DateTime? SchemeAssignmentDate { get; set; }

        //// Business logic mixed into model (anti-pattern)
        //public void UpdateMood()
        //{
        //    // Business rules embedded in model
        //    if (this.LoyaltyScore > ConfigManager.HighLoyaltyThreshold)
        //        this.MoodStatus = ConfigManager.MoodHappy;
        //    else if (this.LoyaltyScore < ConfigManager.LowLoyaltyThreshold)
        //        this.MoodStatus = ConfigManager.MoodBetrayal;
        //    else
        //        this.MoodStatus = ConfigManager.MoodGrumpy;

        //    this.LastMoodUpdate = DateTime.Now;

        //    // Directly accesses database (anti-pattern)
        //    DatabaseHelper.UpdateMinion(this);
        //}

        //// Static utility method in model (anti-pattern)
        //public static bool IsValidSpecialty(string specialty)
        //{
        //    // Hardcoded list (duplicated from ValidationHelper)
        //    return specialty == "Hacking" || specialty == "Explosives" ||
        //           specialty == "Disguise" || specialty == "Combat" ||
        //           specialty == "Engineering" || specialty == "Piloting";
        //}

        //// Business logic for loyalty calculation
        //public void UpdateLoyalty(decimal actualSalaryPaid)
        //{
        //    if (actualSalaryPaid >= this.SalaryDemand)
        //    {
        //        this.LoyaltyScore += ConfigManager.LoyaltyGrowthRate;
        //    }
        //    else
        //    {
        //        this.LoyaltyScore -= ConfigManager.LoyaltyDecayRate;
        //    }

        //    // Clamp to valid range
        //    if (this.LoyaltyScore > 100) this.LoyaltyScore = 100;
        //    if (this.LoyaltyScore < 0) this.LoyaltyScore = 0;

        //    // Update mood based on new loyalty
        //    UpdateMood();
        //}

        // ToString for ComboBox display
        public override string ToString()
        {
            return $"{Name} ({Specialty}, Skill: {SkillLevel})";
        }
    }
}
