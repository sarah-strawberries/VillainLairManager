using System;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace VillainLairManager.Utils
{
    /// <summary>
    /// Configuration manager that reads from appsettings.json
    /// </summary>
    public static class ConfigManager
    {
        private static IConfiguration _configuration;

        static ConfigManager()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }

        // Database Configuration
        public static string DatabasePath => _configuration["Database:Path"] ?? "villainlair.db";

        // Constraint Configuration
        public static int MaxMinionsPerScheme => int.Parse(_configuration["Constraints:MaxMinionsPerScheme"] ?? "10");
        public static int MaxEquipmentPerScheme => int.Parse(_configuration["Constraints:MaxEquipmentPerScheme"] ?? "5");

        // Salary Configuration
        public static decimal DefaultMinionSalary => decimal.Parse(_configuration["Salary:DefaultMinionSalary"] ?? "5000.0");

        // Loyalty Configuration
        public static int LoyaltyDecayRate => int.Parse(_configuration["Loyalty:DecayRate"] ?? "5");
        public static int LoyaltyGrowthRate => int.Parse(_configuration["Loyalty:GrowthRate"] ?? "3");
        public static int LowLoyaltyThreshold => int.Parse(_configuration["Loyalty:LowThreshold"] ?? "40");
        public static int HighLoyaltyThreshold => int.Parse(_configuration["Loyalty:HighThreshold"] ?? "70");

        // Equipment Configuration
        public static int ConditionDegradationRate => int.Parse(_configuration["Equipment:ConditionDegradationRate"] ?? "5");
        public static decimal MaintenanceCostPercentage => decimal.Parse(_configuration["Equipment:MaintenanceCostPercentage"] ?? "0.15");
        public static int MinEquipmentCondition => int.Parse(_configuration["Equipment:MinCondition"] ?? "50");
        public static int BrokenEquipmentCondition => int.Parse(_configuration["Equipment:BrokenCondition"] ?? "20");

        // Base Configuration
        public static decimal DoomsdayMaintenanceCostPercentage => decimal.Parse(_configuration["Base:DoomsdayMaintenanceCostPercentage"] ?? "0.30");

        // Mood Status Strings
        public static string MoodHappy => _configuration["Mood:Happy"] ?? "Happy";
        public static string MoodGrumpy => _configuration["Mood:Grumpy"] ?? "Grumpy";
        public static string MoodBetrayal => _configuration["Mood:Betrayal"] ?? "Plotting Betrayal";
        public static string MoodExhausted => _configuration["Mood:Exhausted"] ?? "Exhausted";

        // Scheme Status Strings
        public static string StatusPlanning => _configuration["Scheme:StatusPlanning"] ?? "Planning";
        public static string StatusActive => _configuration["Scheme:StatusActive"] ?? "Active";
        public static string StatusOnHold => _configuration["Scheme:StatusOnHold"] ?? "On Hold";
        public static string StatusCompleted => _configuration["Scheme:StatusCompleted"] ?? "Completed";
        public static string StatusFailed => _configuration["Scheme:StatusFailed"] ?? "Failed";

        // Valid Specialties
        public static string[] ValidSpecialties => _configuration.GetSection("Specialty:ValidSpecialties").Get<string[]>() ?? new[]
        {
            "Hacking",
            "Explosives",
            "Disguise",
            "Combat",
            "Engineering",
            "Piloting"
        };

        // Valid Equipment Categories
        public static string[] ValidCategories => _configuration.GetSection("Equipment:ValidCategories").Get<string[]>() ?? new[]
        {
            "Weapon",
            "Vehicle",
            "Gadget",
            "Doomsday Device"
        };

        // Business Rules
        public static int OverworkedDays => int.Parse(_configuration["Business:OverworkedDays"] ?? "60");
        public static int SpecialistSkillLevel => int.Parse(_configuration["Business:SpecialistSkillLevel"] ?? "8");
        public static int HighDiabolicalRating => int.Parse(_configuration["Business:HighDiabolicalRating"] ?? "8");
        public static int SuccessLikelihoodHighThreshold => int.Parse(_configuration["Business:SuccessLikelihoodHighThreshold"] ?? "70");
        public static int SuccessLikelihoodLowThreshold => int.Parse(_configuration["Business:SuccessLikelihoodLowThreshold"] ?? "30");
    }
}

