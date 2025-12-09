using System;
using System.Collections.Generic;
using VillainLairManager.Models;

namespace VillainLairManager.Services
{
    /// <summary>
    /// Interface for evil scheme service operations
    /// </summary>
    public interface IEvilSchemeService
    {
        /// <summary>
        /// Calculates the success likelihood of a scheme based on assigned minions, equipment, and other factors
        /// BR-S-001: Success Likelihood Calculation
        /// </summary>
        int CalculateSuccessLikelihood(int schemeId);

        /// <summary>
        /// Updates the success likelihood and stores it in the scheme
        /// </summary>
        void UpdateSuccessLikelihood(int schemeId);

        /// <summary>
        /// Checks if scheme is over budget
        /// </summary>
        bool IsOverBudget(int schemeId);

        /// <summary>
        /// Gets the remaining budget for a scheme
        /// </summary>
        decimal GetRemainingBudget(int schemeId);

        /// <summary>
        /// Checks if scheme can afford additional spending
        /// </summary>
        bool CanAfford(int schemeId, decimal amount);

        /// <summary>
        /// Validates the budget status of a scheme and determines if new assignments are allowed
        /// BR-S-002: Budget Enforcement
        /// </summary>
        (string status, bool allowNewAssignments) ValidateBudgetStatus(int schemeId);

        /// <summary>
        /// Calculates estimated spending if a minion is assigned to the scheme
        /// BR-S-002: Budget Enforcement - Spending Calculation
        /// </summary>
        (decimal estimatedAmount, decimal newTotalSpending, bool wouldExceedBudget) CalculateEstimatedSpending(int schemeId, Minion minionToAssign);

        /// <summary>
        /// Validates if a scheme can transition to a target status
        /// BR-S-003: Status Transition Rules
        /// </summary>
        (bool canTransition, List<string> validationErrors) CanTransitionToStatus(int schemeId, string targetStatus);

        /// <summary>
        /// Gets resource requirements based on diabolical rating
        /// BR-S-004: Resource Assignment Requirements
        /// </summary>
        (int minMinions, int minEquipment, bool requiresDoomsdayDevice) GetResourceRequirements(int diabolicalRating);

        /// <summary>
        /// Validates if scheme meets resource requirements based on diabolical rating
        /// BR-S-004: Resource Assignment Requirements
        /// </summary>
        (bool isMet, List<string> warnings) ValidateResourceRequirements(int schemeId, int assignedMinionCount, int assignedEquipmentCount, bool hasDoomsdayDevice);

        /// <summary>
        /// Gets the deadline status of a scheme
        /// BR-S-005: Deadline Management
        /// </summary>
        string GetDeadlineStatus(int schemeId);

        /// <summary>
        /// Validates specialty matching for scheme
        /// BR-S-006: Specialty Matching
        /// </summary>
        (bool hasRequiredSpecialty, int matchingMinionCount, List<string> warnings) ValidateSpecialtyMatching(int schemeId);

        /// <summary>
        /// Validates budget values for a scheme
        /// BR-S-007: Scheme Budget Validation
        /// </summary>
        (bool isValid, List<string> warnings) ValidateBudgetValues(decimal budget, decimal estimatedCost);

        /// <summary>
        /// Applies automatic transitions when deadline passes
        /// BR-S-003: Status Transition Rules, BR-S-005: Deadline Management
        /// </summary>
        void ApplyAutoTransitions(int schemeId);
    }
}
