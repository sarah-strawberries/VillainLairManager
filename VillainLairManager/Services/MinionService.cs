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
        /// Create a new minion with full validation (BR-M-005, BR-M-006, BR-M-007)
        /// </summary>
        public Minion CreateMinion(Minion minion)
        {
            ValidateMinionData(minion);
            DatabaseHelper.InsertMinion(minion);
            
            // Reload minions from database to get the auto-generated ID
            var allMinions = DatabaseHelper.GetAllMinions();
            if (allMinions != null && allMinions.Count > 0)
            {
                Minions = allMinions.ToDictionary(m => m.MinionId);
                
                // Return the minion with the correct ID (find it by name and other fields)
                var createdMinion = allMinions.FirstOrDefault(m => 
                    m.Name == minion.Name && 
                    m.Specialty == minion.Specialty &&
                    m.SkillLevel == minion.SkillLevel &&
                    m.SalaryDemand == minion.SalaryDemand);
                
                return createdMinion ?? minion;
            }
            
            return minion;
        }

        /// <summary>
        /// Update an existing minion with validation
        /// </summary>
        public void UpdateMinion(Minion minion)
        {
            if (Minions.ContainsKey(minion.MinionId))
            {
                ValidateMinionData(minion);
                DatabaseHelper.UpdateMinion(minion);
                Minions[minion.MinionId] = minion;
            }
        }

        /// <summary>
        /// Validate minion data against business rules BR-M-005, BR-M-006, BR-M-007
        /// </summary>
        private void ValidateMinionData(Minion minion)
        {
            // BR-M-005: Specialty Validation
            if (string.IsNullOrEmpty(minion.Specialty))
            {
                throw new ArgumentException("Specialty is required (BR-M-005)");
            }

            if (!ValidationHelper.IsValidSpecialty(minion.Specialty))
            {
                throw new ArgumentException(
                    $"Invalid specialty '{minion.Specialty}'. Must be one of: {string.Join(", ", ConfigManager.ValidSpecialties)} (BR-M-005)",
                    nameof(minion.Specialty));
            }

            // BR-M-006: Skill Level Validation
            if (!ValidationHelper.IsValidSkillLevel(minion.SkillLevel))
            {
                throw new ArgumentException(
                    $"Skill level must be between 1 and 10, but got {minion.SkillLevel} (BR-M-006)",
                    nameof(minion.SkillLevel));
            }

            // BR-M-007: Salary Demand Validation
            if (minion.SalaryDemand <= 0)
            {
                throw new ArgumentException(
                    $"Salary demand must be greater than zero, but got {minion.SalaryDemand} (BR-M-007)",
                    nameof(minion.SalaryDemand));
            }

            // Optional warning for unusually high salary (soft validation)
            if (minion.SalaryDemand > 1000000)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Warning: Salary demand {minion.SalaryDemand} seems unusually high (BR-M-007)");
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
            
            // BR-M-002: Check for overwork first (exhaustion overrides other moods)
            if (minion.SchemeAssignmentDate.HasValue)
            {
                var daysAssigned = (DateTime.Now - minion.SchemeAssignmentDate.Value).Days;
                if (daysAssigned > ConfigManager.OverworkedDays)
                {
                    minion.MoodStatus = ConfigManager.MoodExhausted;
                    minion.LastMoodUpdate = DateTime.Now;
                    DatabaseHelper.UpdateMinion(minion);
                    return;
                }
            }

            // Standard mood based on loyalty
            if (minion.LoyaltyScore > ConfigManager.HighLoyaltyThreshold)
                minion.MoodStatus = ConfigManager.MoodHappy;
            else if (minion.LoyaltyScore < ConfigManager.LowLoyaltyThreshold)
                minion.MoodStatus = ConfigManager.MoodBetrayal;
            else
                minion.MoodStatus = ConfigManager.MoodGrumpy;

            minion.LastMoodUpdate = DateTime.Now;

            DatabaseHelper.UpdateMinion(minion);
        }


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

            // Clamp to valid range using ValidationHelper
            if (!ValidationHelper.IsValidLoyalty(minion.LoyaltyScore))
            {
                minion.LoyaltyScore = minion.LoyaltyScore > 100 ? 100 : 0;
            }

            // Update mood based on new loyalty
            UpdateMood(minionid);
        }

        /// <summary>
        /// Assign a minion to a scheme with validation (BR-M-003)
        /// </summary>
        public void AssignMinionToScheme(int minionId, EvilScheme scheme)
        {
            // Check minion exists
            if (!Minions.TryGetValue(minionId, out var minion))
            {
                throw new ArgumentException($"Minion with ID {minionId} not found (BR-M-003)", nameof(minionId));
            }

            // BR-M-003: Check if already assigned to active scheme (before checking other validations)
            if (minion.CurrentSchemeId.HasValue && minion.CurrentSchemeId.Value != scheme.SchemeId)
            {
                // Minion is assigned to a different scheme - reject if we're trying to assign to "Active" status
                if (scheme.Status == "Active")
                {
                    throw new ArgumentException(
                        $"Minion is already assigned to active scheme {minion.CurrentSchemeId} (BR-M-003)",
                        nameof(minion.CurrentSchemeId));
                }
            }

            // BR-M-003: Validate skill level
            if (minion.SkillLevel < scheme.RequiredSkillLevel)
            {
                throw new ArgumentException(
                    $"Minion skill level ({minion.SkillLevel}) is below scheme requirement ({scheme.RequiredSkillLevel}) (BR-M-003)",
                    nameof(minion.SkillLevel));
            }

            // BR-M-003: Validate specialty
            if (minion.Specialty != scheme.RequiredSpecialty)
            {
                throw new ArgumentException(
                    $"Minion specialty ({minion.Specialty}) doesn't match scheme requirement ({scheme.RequiredSpecialty}) (BR-M-003)",
                    nameof(minion.Specialty));
            }

            // Assignment is valid - update minion's current scheme
            minion.CurrentSchemeId = scheme.SchemeId;
            minion.SchemeAssignmentDate = DateTime.Now;
            DatabaseHelper.UpdateMinion(minion);
        }

        /// <summary>
        /// Assign a minion to a base with capacity validation (BR-M-004)
        /// </summary>
        public void AssignMinionToBase(int minionId, SecretBase base_)
        {
            // Check minion exists
            if (!Minions.TryGetValue(minionId, out var minion))
            {
                throw new ArgumentException($"Minion with ID {minionId} not found (BR-M-004)", nameof(minionId));
            }

            // BR-M-004: Validate base capacity
            if (base_.CurrentOccupancy >= base_.Capacity)
            {
                throw new ArgumentException(
                    $"Base is at full capacity ({base_.CurrentOccupancy}/{base_.Capacity}) (BR-M-004)",
                    nameof(base_));
            }

            // Assignment is valid - update minion's current base and increment occupancy
            minion.CurrentBaseId = base_.BaseId;
            base_.CurrentOccupancy++;
            DatabaseHelper.UpdateMinion(minion);
        }

        /// <summary>
        /// Bulk assign multiple minions to a base with capacity validation (BR-M-004)
        /// </summary>
        public void AssignMinionsToBase(List<int> minionIds, SecretBase base_)
        {
            // BR-M-004: Check if bulk assignment would exceed capacity
            if (base_.CurrentOccupancy + minionIds.Count > base_.Capacity)
            {
                throw new ArgumentException(
                    $"Bulk assignment would exceed base capacity. Current: {base_.CurrentOccupancy}, Requested: {minionIds.Count}, Capacity: {base_.Capacity} would exceed capacity (BR-M-004)",
                    nameof(minionIds));
            }

            // All minions exist and assignment is valid
            foreach (var minionId in minionIds)
            {
                if (!Minions.TryGetValue(minionId, out var minion))
                {
                    throw new ArgumentException($"Minion with ID {minionId} not found (BR-M-004)", nameof(minionIds));
                }

                // Assign each minion
                minion.CurrentBaseId = base_.BaseId;
                base_.CurrentOccupancy++;
                DatabaseHelper.UpdateMinion(minion);
            }
        }
    }

}