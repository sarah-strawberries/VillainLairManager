namespace VillainLairManager.Services
{
    /// <summary>
    /// Interface for secret base service operations
    /// </summary>
    public interface ISecretBaseService
    {
        /// <summary>
        /// Gets the current occupancy of a base
        /// </summary>
        int GetCurrentOccupancy(int baseId);

        /// <summary>
        /// Gets the available capacity remaining in a base
        /// </summary>
        int GetAvailableCapacity(int baseId);

        /// <summary>
        /// Checks if a base can accommodate an additional minion
        /// </summary>
        bool CanAccommodateMinion(int baseId);

        /// <summary>
        /// Gets the occupancy percentage of a base
        /// </summary>
        decimal GetOccupancyPercentage(int baseId);

        /// <summary>
        /// Checks if a base is at full capacity
        /// </summary>
        bool IsAtFullCapacity(int baseId);
    }
}
