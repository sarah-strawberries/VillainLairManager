using System;
using System.Linq;

namespace VillainLairManager.Utils
{
    /// <summary>
    /// Static validation helper with duplicated logic (anti-pattern)
    /// </summary>
    public static class ValidationHelper
    {
        // Specialty validation using ConfigManager
        public static bool IsValidSpecialty(string specialty)
        {
            return ConfigManager.ValidSpecialties.Contains(specialty);
        }

        // Category validation using ConfigManager
        public static bool IsValidCategory(string category)
        {
            return ConfigManager.ValidCategories.Contains(category);
        }

        // Skill level validation (duplicated in forms)
        public static bool IsValidSkillLevel(int skillLevel)
        {
            return skillLevel >= 1 && skillLevel <= 10;
        }

        // Loyalty validation (duplicated in forms)
        public static bool IsValidLoyalty(int loyalty)
        {
            return loyalty >= 0 && loyalty <= 100;
        }

        // Condition validation (duplicated in forms)
        public static bool IsValidCondition(int condition)
        {
            return condition >= 0 && condition <= 100;
        }

        // Diabolical rating validation
        public static bool IsValidDiabolicalRating(int rating)
        {
            return rating >= 1 && rating <= 10;
        }

        // Security level validation
        public static bool IsValidSecurityLevel(int level)
        {
            return level >= 1 && level <= 10;
        }
    }
}
