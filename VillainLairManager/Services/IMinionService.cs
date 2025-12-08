namespace VillainLairManager.Services
{
    /// <summary>
    /// Interface for minion service operations
    /// </summary>
    public interface IMinionService
    {
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
