using System;
using System.Collections.Generic;

namespace VillainLairManager.Models
{
    /// <summary>
    /// Evil Scheme model - represents a villainous plan
    /// </summary>
    public class EvilScheme
    {
        public int SchemeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Budget { get; set; }
        public decimal CurrentSpending { get; set; }
        public int RequiredSkillLevel { get; set; }
        public string RequiredSpecialty { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime TargetCompletionDate { get; set; }
        public int DiabolicalRating { get; set; }
        public int SuccessLikelihood { get; set; }
        
        // Tracking assigned resources
        /// <summary>
        /// List of minion IDs assigned to this scheme
        /// </summary>
        public IList<int> AssignedMinionIds { get; set; } = new List<int>();
        
        /// <summary>
        /// List of equipment IDs assigned to this scheme
        /// </summary>
        public IList<int> AssignedEquipmentIds { get; set; } = new List<int>();
        
        // Metadata
        /// <summary>
        /// Date this scheme was created
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Controls whether new minion/equipment assignments are allowed based on budget status
        /// </summary>
        public bool AllowNewAssignments { get; set; } = true;

        // ToString for display
        public override string ToString()
        {
            return $"{Name} ({Status}, {SuccessLikelihood}% success)";
        }
    }
}
