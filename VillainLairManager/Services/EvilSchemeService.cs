using System;
using System.Collections.Generic;
using VillainLairManager.Models;
using VillainLairManager.Utils;

namespace VillainLairManager.Services
{
    public class EvilSchemeService(IRepository DatabaseHelper) : IEvilSchemeService
    {
        public Dictionary<int, EvilScheme> Schemes { get; set; }

        /// <summary>
        /// Calculates the success likelihood of a scheme based on assigned minions, equipment, and other factors
        /// </summary>
        public int CalculateSuccessLikelihood(int schemeId)
        {
            var scheme = Schemes[schemeId];
            int baseSuccess = 50;

            // Get assigned minions from database
            var assignedMinions = DatabaseHelper.GetAllMinions();
            int matchingMinions = 0;
            int totalMinions = 0;

            foreach (var minion in assignedMinions)
            {
                if (minion.CurrentSchemeId == scheme.SchemeId)
                {
                    totalMinions++;
                    if (minion.Specialty == scheme.RequiredSpecialty)
                    {
                        matchingMinions++;
                    }
                }
            }

            int minionBonus = matchingMinions * 10;

            // Get assigned equipment
            var assignedEquipment = DatabaseHelper.GetAllEquipment();
            int workingEquipmentCount = 0;

            foreach (var equipment in assignedEquipment)
            {
                if (equipment.AssignedToSchemeId == scheme.SchemeId &&
                    equipment.Condition >= ConfigManager.MinEquipmentCondition)
                {
                    workingEquipmentCount++;
                }
            }

            int equipmentBonus = workingEquipmentCount * 5;

            // Calculate penalties
            int budgetPenalty = (scheme.CurrentSpending > scheme.Budget) ? -20 : 0;
            int resourcePenalty = (totalMinions >= 2 && matchingMinions >= 1) ? 0 : -15;
            int timelinePenalty = (DateTime.Now > scheme.TargetCompletionDate) ? -25 : 0;

            // Calculate final success
            int success = baseSuccess + minionBonus + equipmentBonus + budgetPenalty + resourcePenalty + timelinePenalty;

            // Clamp to 0-100 range
            if (success < 0) success = 0;
            if (success > 100) success = 100;

            return success;
        }

        /// <summary>
        /// Updates the success likelihood and stores it in the scheme
        /// </summary>
        public void UpdateSuccessLikelihood(int schemeId)
        {
            var scheme = Schemes[schemeId];
            scheme.SuccessLikelihood = CalculateSuccessLikelihood(schemeId);
            DatabaseHelper.UpdateScheme(scheme);
        }

        /// <summary>
        /// Checks if scheme is over budget
        /// </summary>
        public bool IsOverBudget(int schemeId)
        {
            var scheme = Schemes[schemeId];
            return scheme.CurrentSpending > scheme.Budget;
        }

        /// <summary>
        /// Gets the remaining budget for a scheme
        /// </summary>
        public decimal GetRemainingBudget(int schemeId)
        {
            var scheme = Schemes[schemeId];
            return scheme.Budget - scheme.CurrentSpending;
        }

        /// <summary>
        /// Checks if scheme can afford additional spending
        /// </summary>
        public bool CanAfford(int schemeId, decimal amount)
        {
            return GetRemainingBudget(schemeId) >= amount;
        }
    }
}
