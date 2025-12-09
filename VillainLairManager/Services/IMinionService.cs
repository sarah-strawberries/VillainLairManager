using VillainLairManager.Models;

namespace VillainLairManager.Services
{
    /// <summary>
    /// Interface for minion service operations
    /// </summary>
    public interface IMinionService
    {
        /// <summary>
        /// Get a minion by ID
        /// </summary>
        Minion GetMinionById(int minionId);

        /// <summary>
        /// Get all minions
        /// </summary>
        IEnumerable<Minion> GetAllMinions();

        /// <summary>
        /// Create a new minion
        /// </summary>
        Minion CreateMinion(Minion minion);

        /// <summary>
        /// Update an existing minion
        /// </summary>
        void UpdateMinion(Minion minion);

        /// <summary>
        /// Delete a minion by ID
        /// </summary>
        void DeleteMinion(int minionId);

        /// <summary>
        /// Updates the mood status of a minion based on their loyalty score
        /// </summary>
        void UpdateMood(int minionId);

        /// <summary>
        /// Updates a minion's loyalty score based on salary payment and updates their mood
        /// </summary>
        void UpdateLoyalty(int minionId, decimal actualSalaryPaid);

        /// <summary>
        /// Assign a minion to a scheme with validation (BR-M-003)
        /// </summary>
        void AssignMinionToScheme(int minionId, EvilScheme scheme);

        /// <summary>
        /// Assign a minion to a base with capacity validation (BR-M-004)
        /// </summary>
        void AssignMinionToBase(int minionId, SecretBase base_);

        /// <summary>
        /// Bulk assign multiple minions to a base with capacity validation (BR-M-004)
        /// </summary>
        void AssignMinionsToBase(List<int> minionIds, SecretBase base_);
    }
}
