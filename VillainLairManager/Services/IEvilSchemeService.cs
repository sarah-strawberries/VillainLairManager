namespace VillainLairManager.Services
{
    /// <summary>
    /// Interface for evil scheme service operations
    /// </summary>
    public interface IEvilSchemeService
    {
        /// <summary>
        /// Calculates the success likelihood of a scheme based on assigned minions, equipment, and other factors
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
    }
}
