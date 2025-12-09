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

        /// <summary>
        /// Validates the budget status of a scheme and determines if new assignments are allowed
        /// BR-S-002: Budget Enforcement
        /// </summary>
        /// <returns>Tuple containing status and whether new assignments are allowed</returns>
        public (string status, bool allowNewAssignments) ValidateBudgetStatus(int schemeId)
        {
            var scheme = Schemes[schemeId];

            if (scheme.CurrentSpending > scheme.Budget)
            {
                scheme.AllowNewAssignments = false;
                return ("Over Budget - Action Required", false);
            }
            else if (scheme.CurrentSpending > scheme.Budget * 0.9m && scheme.CurrentSpending < scheme.Budget)
            {
                scheme.AllowNewAssignments = true;
                return ("Approaching Budget Limit", true);
            }
            else
            {
                scheme.AllowNewAssignments = true;
                return ("Within Budget", true);
            }
        }

        /// <summary>
        /// Calculates estimated spending if a minion is assigned to the scheme
        /// BR-S-002: Budget Enforcement - Spending Calculation
        /// </summary>
        /// <returns>Tuple with estimated additional spending, new total spending, and whether it would exceed budget</returns>
        public (decimal estimatedAmount, decimal newTotalSpending, bool wouldExceedBudget) CalculateEstimatedSpending(int schemeId, Minion minionToAssign)
        {
            var scheme = Schemes[schemeId];

            // Calculate months remaining until deadline
            int estimatedMonthsRemaining = (int)Math.Ceiling((scheme.TargetCompletionDate - DateTime.Now).TotalDays / 30.0);
            if (estimatedMonthsRemaining < 1)
                estimatedMonthsRemaining = 1;

            decimal additionalSpending = minionToAssign.SalaryDemand * estimatedMonthsRemaining;
            decimal newTotalSpending = scheme.CurrentSpending + additionalSpending;
            bool wouldExceed = newTotalSpending > scheme.Budget;

            return (additionalSpending, newTotalSpending, wouldExceed);
        }

        /// <summary>
        /// Validates if a scheme can transition to a target status
        /// BR-S-003: Status Transition Rules
        /// </summary>
        /// <returns>Tuple with whether transition is valid and any validation errors</returns>
        public (bool canTransition, List<string> validationErrors) CanTransitionToStatus(int schemeId, string targetStatus)
        {
            var scheme = Schemes[schemeId];
            var errors = new List<string>();
            string currentStatus = scheme.Status ?? "Planning";

            // Validate Planning -> Active
            if (currentStatus == "Planning" && targetStatus == "Active")
            {
                if (!scheme.StartDate.HasValue)
                    errors.Add("StartDate must be set before activating");

                var assignedMinions = DatabaseHelper.GetAllMinions();
                int totalAssigned = 0;
                int matchingSpecialty = 0;

                foreach (var minion in assignedMinions)
                {
                    if (minion.CurrentSchemeId == scheme.SchemeId)
                    {
                        totalAssigned++;
                        if (minion.Specialty == scheme.RequiredSpecialty)
                            matchingSpecialty++;
                    }
                }

                if (totalAssigned < 2)
                    errors.Add("At least 2 minions must be assigned");
                if (matchingSpecialty < 1)
                    errors.Add("At least 1 minion with required specialty must be assigned");

                if (scheme.CurrentSpending > scheme.Budget)
                    errors.Add("Scheme cannot be over budget when activating");

                return (errors.Count == 0, errors);
            }

            // Validate Active -> Completed
            if (currentStatus == "Active" && targetStatus == "Completed")
            {
                if (scheme.SuccessLikelihood < 70)
                    errors.Add("Success likelihood must be at least 70% to complete");
                if (DateTime.Now < scheme.TargetCompletionDate)
                    errors.Add("Target completion date must have passed");

                return (errors.Count == 0, errors);
            }

            // Validate Active -> On Hold (always allowed)
            if (currentStatus == "Active" && targetStatus == "On Hold")
            {
                return (true, errors);
            }

            // Validate On Hold -> Active (same as Planning -> Active)
            if (currentStatus == "On Hold" && targetStatus == "Active")
            {
                var assignedMinions = DatabaseHelper.GetAllMinions();
                int totalAssigned = 0;
                int matchingSpecialty = 0;

                foreach (var minion in assignedMinions)
                {
                    if (minion.CurrentSchemeId == scheme.SchemeId)
                    {
                        totalAssigned++;
                        if (minion.Specialty == scheme.RequiredSpecialty)
                            matchingSpecialty++;
                    }
                }

                if (totalAssigned < 2)
                    errors.Add("At least 2 minions must be assigned");
                if (matchingSpecialty < 1)
                    errors.Add("At least 1 minion with required specialty must be assigned");

                return (errors.Count == 0, errors);
            }

            // Any status -> Planning (always allowed, resets assignments)
            if (targetStatus == "Planning")
            {
                return (true, errors);
            }

            // Active -> Failed (always allowed)
            if (currentStatus == "Active" && targetStatus == "Failed")
            {
                return (true, errors);
            }

            // Invalid transition
            errors.Add($"Cannot transition from {currentStatus} to {targetStatus}");
            return (false, errors);
        }

        /// <summary>
        /// Gets resource requirements based on diabolical rating
        /// BR-S-004: Resource Assignment Requirements
        /// </summary>
        /// <returns>Tuple with minimum minions, minimum equipment, and whether doomsday device is required</returns>
        public (int minMinions, int minEquipment, bool requiresDoomsdayDevice) GetResourceRequirements(int diabolicalRating)
        {
            if (diabolicalRating >= 8)
                return (3, 2, true);
            else if (diabolicalRating >= 5)
                return (2, 1, false);
            else
                return (1, 0, false);
        }

        /// <summary>
        /// Validates if scheme meets resource requirements based on diabolical rating
        /// BR-S-004: Resource Assignment Requirements
        /// </summary>
        /// <returns>Tuple with whether requirements are met and any warnings</returns>
        public (bool isMet, List<string> warnings) ValidateResourceRequirements(int schemeId, int assignedMinionCount, int assignedEquipmentCount, bool hasDoomsdayDevice)
        {
            var scheme = Schemes[schemeId];
            var warnings = new List<string>();

            var (minMinions, minEquipment, requiresDoomsdayDevice) = GetResourceRequirements(scheme.DiabolicalRating);

            if (requiresDoomsdayDevice && !hasDoomsdayDevice)
                warnings.Add("Highly diabolical schemes require a doomsday device");

            if (assignedEquipmentCount < minEquipment)
                warnings.Add($"Scheme requires at least {minEquipment} equipment, but only {assignedEquipmentCount} assigned");

            if (assignedMinionCount < minMinions)
                warnings.Add($"Scheme requires at least {minMinions} minions, but only {assignedMinionCount} assigned");

            bool isMet = assignedEquipmentCount >= minEquipment && 
                        assignedMinionCount >= minMinions && 
                        (!requiresDoomsdayDevice || hasDoomsdayDevice);

            return (isMet, warnings);
        }

        /// <summary>
        /// Gets the deadline status of a scheme
        /// BR-S-005: Deadline Management
        /// </summary>
        /// <returns>Status indicator: OnTrack, DueSoon, Urgent, or Overdue</returns>
        public string GetDeadlineStatus(int schemeId)
        {
            var scheme = Schemes[schemeId];
            int daysUntilDeadline = (int)(scheme.TargetCompletionDate - DateTime.Now).TotalDays;

            if (daysUntilDeadline < 0)
                return "Overdue";
            else if (daysUntilDeadline <= 7)
                return "Urgent";
            else if (daysUntilDeadline <= 30)
                return "Due soon";
            else
                return "On track";
        }

        /// <summary>
        /// Validates specialty matching for scheme
        /// BR-S-006: Specialty Matching
        /// </summary>
        /// <returns>Tuple with whether required specialty is met, count of matching minions, and warnings</returns>
        public (bool hasRequiredSpecialty, int matchingMinionCount, List<string> warnings) ValidateSpecialtyMatching(int schemeId)
        {
            var scheme = Schemes[schemeId];
            var warnings = new List<string>();

            var assignedMinions = DatabaseHelper.GetAllMinions();
            int matchingMinions = 0;

            foreach (var minion in assignedMinions)
            {
                if (minion.CurrentSchemeId == scheme.SchemeId && minion.Specialty == scheme.RequiredSpecialty)
                    matchingMinions++;
            }

            bool hasRequired = matchingMinions > 0;

            if (matchingMinions == 0 && (scheme.Status == "Active" || scheme.Status == "Attempting to Activate"))
                warnings.Add("No minions with required specialty assigned");
            else if (matchingMinions == 1)
                warnings.Add("Only one minion with required specialty - risky!");

            return (hasRequired, matchingMinions, warnings);
        }

        /// <summary>
        /// Validates budget values for a scheme
        /// BR-S-007: Scheme Budget Validation
        /// </summary>
        /// <returns>Tuple with whether budget is valid and any warnings</returns>
        public (bool isValid, List<string> warnings) ValidateBudgetValues(decimal budget, decimal estimatedCost)
        {
            var warnings = new List<string>();
            const decimal MinimumBudget = 10000m;
            const decimal MaximumBudget = 10000000m;

            if (budget < MinimumBudget)
                return (false, new List<string> { "Budget too low - minimum is 10,000 evil dollars" });

            if (budget > MaximumBudget)
                warnings.Add("Budget seems unrealistic - are you sure?");

            if (budget < estimatedCost)
                warnings.Add("Budget may be insufficient for resource requirements");

            return (true, warnings);
        }

        /// <summary>
        /// Applies automatic transitions when deadline passes
        /// BR-S-003: Status Transition Rules, BR-S-005: Deadline Management
        /// </summary>
        public void ApplyAutoTransitions(int schemeId)
        {
            var scheme = Schemes[schemeId];

            // Only check auto-transitions if scheme is Active
            if (scheme.Status != "Active")
                return;

            int daysUntilDeadline = (int)(scheme.TargetCompletionDate - DateTime.Now).TotalDays;

            // Check if deadline has passed
            if (daysUntilDeadline < 0)
            {
                // Auto-complete if success is 70% or higher
                if (scheme.SuccessLikelihood >= 70)
                {
                    scheme.Status = "Completed";
                    DatabaseHelper.UpdateScheme(scheme);
                }
                // Auto-fail if success is below 30%
                else if (scheme.SuccessLikelihood < 30)
                {
                    scheme.Status = "Failed";
                    DatabaseHelper.UpdateScheme(scheme);
                }
            }
        }
    }
}
