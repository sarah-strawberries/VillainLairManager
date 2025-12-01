using System;

namespace VillainLairManager.Utils
{
    /// <summary>
    /// Static configuration manager with hardcoded values (anti-pattern)
    /// </summary>
    public static class ConfigManager
    {
        // Hardcoded database path - no configuration file (anti-pattern)
        public const string DatabasePath = "villainlair.db";

        // Magic numbers scattered as constants
        public const int MaxMinionsPerScheme = 10;
        public const int MaxEquipmentPerScheme = 5;
        public const decimal DefaultMinionSalary = 5000.0m;
        public const int LoyaltyDecayRate = 5;
        public const int LoyaltyGrowthRate = 3;
        public const int ConditionDegradationRate = 5;
        public const decimal MaintenanceCostPercentage = 0.15m;
        public const decimal DoomsdayMaintenanceCostPercentage = 0.30m;

        // Magic strings for mood status
        public const string MoodHappy = "Happy";
        public const string MoodGrumpy = "Grumpy";
        public const string MoodBetrayal = "Plotting Betrayal";
        public const string MoodExhausted = "Exhausted";

        // Magic strings for scheme status
        public const string StatusPlanning = "Planning";
        public const string StatusActive = "Active";
        public const string StatusOnHold = "On Hold";
        public const string StatusCompleted = "Completed";
        public const string StatusFailed = "Failed";

        // Hardcoded specialty list (duplicated elsewhere too - anti-pattern)
        public static readonly string[] ValidSpecialties = new string[]
        {
            "Hacking",
            "Explosives",
            "Disguise",
            "Combat",
            "Engineering",
            "Piloting"
        };

        // Hardcoded equipment categories (duplicated elsewhere too)
        public static readonly string[] ValidCategories = new string[]
        {
            "Weapon",
            "Vehicle",
            "Gadget",
            "Doomsday Device"
        };

        // Business rule thresholds
        public const int LowLoyaltyThreshold = 40;
        public const int HighLoyaltyThreshold = 70;
        public const int OverworkedDays = 60;
        public const int SpecialistSkillLevel = 8;
        public const int MinEquipmentCondition = 50;
        public const int BrokenEquipmentCondition = 20;
        public const int HighDiabolicalRating = 8;
        public const int SuccessLikelihoodHighThreshold = 70;
        public const int SuccessLikelihoodLowThreshold = 30;
    }
}
