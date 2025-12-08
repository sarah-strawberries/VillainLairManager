namespace VillainLairManager.Services
{
    /// <summary>
    /// Interface for equipment service operations
    /// </summary>
    public interface IEquipmentService
    {
        /// <summary>
        /// Degrades equipment condition based on usage and maintenance status
        /// </summary>
        void DegradeCondition(int equipmentId);

        /// <summary>
        /// Performs maintenance on equipment and returns the cost
        /// </summary>
        decimal PerformMaintenance(int equipmentId);

        /// <summary>
        /// Checks if equipment is operational (condition meets minimum threshold)
        /// </summary>
        bool IsOperational(int equipmentId);

        /// <summary>
        /// Checks if equipment is broken (condition below broken threshold)
        /// </summary>
        bool IsBroken(int equipmentId);
    }
}
