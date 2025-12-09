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
    }
}
