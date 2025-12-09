using VillainLairManager.Models;
using VillainLairManager.Services;
using NSubstitute;
using NUnit.Framework;
using VillainLairManager;
using System;

namespace VillainousTesting;

/// <summary>
/// Comprehensive tests for Minion Business Rules (BR-M-001 through BR-M-007)
/// These tests are designed to validate rule compliance and currently assert they FAIL
/// until the rules are properly implemented in MinionService.
/// </summary>
public class MinionRulesTests
{
    private IRepository mockRepository;

    [SetUp]
    public void Setup()
    {
        mockRepository = Substitute.For<IRepository>();
    }

    // ===================== RULE 1: Loyalty Decay and Growth (BR-M-001) =====================
    // Format: [initialLoyalty, salaryDemand, amountPaid, expectedLoyalty]

    [TestCase(70, 5000, 5000, 73)]   // Satisfied minion: +3
    [TestCase(70, 5000, 6000, 73)]   // Overpaid minion: +3 (same as satisfied)
    [TestCase(70, 5000, 4000, 65)]   // Underpaid minion: -5
    [TestCase(3, 3000, 2000, 0)]     // Minimum boundary: clamp at 0
    [TestCase(98, 3000, 4000, 100)]  // Maximum boundary: clamp at 100
    public void BR_M_001_LoyaltyDecayAndGrowth(int initialLoyalty, decimal salaryDemand, decimal amountPaid, int expectedLoyalty)
    {
        var minion = new Minion 
        { 
            MinionId = 1, 
            Name = "Test Minion", 
            LoyaltyScore = initialLoyalty, 
            SalaryDemand = salaryDemand 
        };
        var minionService = new MinionService(mockRepository);
        minionService.Minions = new Dictionary<int, Minion> { { 1, minion } };

        minionService.UpdateLoyalty(1, amountPaid);

        Assert.That(minionService.Minions[1].LoyaltyScore, Is.EqualTo(expectedLoyalty), 
            "BR-M-001: Loyalty calculation should match expected value based on payment");
    }

    // ===================== RULE 2: Mood Determination (BR-M-002) =====================
    // Format: [loyalty, daysAssigned, hasSchemeAssignment, expectedMood]

    [TestCase(85, -30, true, "Happy")]              // High loyalty, not overworked
    [TestCase(85, -70, true, "Exhausted")]          // Overworked (70 days > 60)
    [TestCase(55, -20, true, "Grumpy")]             // Medium loyalty
    [TestCase(25, -15, true, "Plotting Betrayal")]  // Low loyalty
    [TestCase(45, 0, false, "Grumpy")]              // Unassigned minion
    public void BR_M_002_MoodDetermination(int loyalty, int daysAssigned, bool hasSchemeAssignment, string expectedMood)
    {
        var minion = new Minion
        {
            MinionId = 1,
            Name = "Test Minion",
            LoyaltyScore = loyalty,
            SchemeAssignmentDate = hasSchemeAssignment ? (DateTime?)DateTime.Now.AddDays(daysAssigned) : null,
            CurrentSchemeId = hasSchemeAssignment ? 1 : (int?)null,
            LastMoodUpdate = DateTime.Now
        };
        var minionService = new MinionService(mockRepository);
        minionService.Minions = new Dictionary<int, Minion> { { 1, minion } };

        minionService.UpdateMood(1);

        Assert.That(minionService.Minions[1].MoodStatus, Is.EqualTo(expectedMood),
            "BR-M-002: Mood should match expected value based on loyalty and assignment duration");
    }
        // ============================================================================
    // BR-M-003: Scheme Assignment Validation Tests
    // ============================================================================
    // Tests validate that minions can only be assigned to schemes if they meet
    // qualification requirements and availability constraints.

    [TestCase(5, 6, "Hacking", "Hacking", null, true)]  // Skill too low
    [TestCase(8, 6, "Combat", "Hacking", null, true)]   // Specialty mismatch
    [TestCase(8, 6, "Hacking", "Hacking", 5, true)]     // Already assigned to active scheme
    [TestCase(8, 6, "Hacking", "Hacking", null, false)] // Valid assignment
    [TestCase(8, 6, "Hacking", "Hacking", 3, false)]    // Reassign from completed scheme
    public void BR_M_003_SchemeAssignmentValidation(
        int minionSkill, int schemeReqSkill, string minionSpecialty, 
        string schemeReqSpecialty, int? currentSchemeId, bool shouldThrow)
    {
        var minion = new Minion
        {
            MinionId = 1,
            Name = "Test Minion",
            SkillLevel = minionSkill,
            Specialty = minionSpecialty,
            LoyaltyScore = 50,
            SalaryDemand = 5000,
            CurrentSchemeId = currentSchemeId,
            LastMoodUpdate = DateTime.Now
        };

        var scheme = new EvilScheme
        {
            SchemeId = currentSchemeId == 5 ? 2 : 1,  // Always use different scheme ID to test "already assigned"
            Name = "Test Scheme",
            RequiredSkillLevel = schemeReqSkill,
            RequiredSpecialty = schemeReqSpecialty,
            Status = currentSchemeId == 5 ? "Active" : "Planning"
        };

        var minionService = new MinionService(mockRepository);
        minionService.Minions = new Dictionary<int, Minion> { { 1, minion } };

        if (shouldThrow)
        {
            Assert.That(() => minionService.AssignMinionToScheme(1, scheme),
                Throws.ArgumentException,
                "BR-M-003: Assignment should be rejected with invalid parameters");
        }
        else
        {
            Assert.That(() => minionService.AssignMinionToScheme(1, scheme),
                Throws.Nothing,
                "BR-M-003: Assignment should succeed with valid parameters");
        }
    }

    // ============================================================================
    // BR-M-004: Base Assignment Capacity Tests
    // ============================================================================
    // Tests validate that secret bases have limited capacity and cannot exceed it.

    [TestCase(50, 45, 1, false)]  // Below capacity
    [TestCase(50, 49, 1, false)]  // Exactly at capacity
    [TestCase(50, 50, 1, true)]   // Over capacity
    [TestCase(30, 28, 3, true)]   // Bulk assignment exceeds
    public void BR_M_004_BaseAssignmentCapacity(int capacity, int currentOccupancy, int newAssignments, bool shouldThrow)
    {
        var minions = new List<Minion>();
        for (int i = 1; i <= newAssignments; i++)
        {
            minions.Add(new Minion
            {
                MinionId = i,
                Name = $"Minion {i}",
                SkillLevel = 5,
                Specialty = "Combat",
                LoyaltyScore = 50,
                SalaryDemand = 3000,
                LastMoodUpdate = DateTime.Now
            });
        }

        var base_ = new SecretBase
        {
            BaseId = 1,
            Name = "Test Base",
            Capacity = capacity,
            CurrentOccupancy = currentOccupancy
        };

        var mockRepo = Substitute.For<IRepository>();
        var minionService = new MinionService(mockRepo);
        minionService.Minions = minions.ToDictionary(m => m.MinionId);

        if (newAssignments == 1)
        {
            if (shouldThrow)
            {
                Assert.That(() => minionService.AssignMinionToBase(1, base_),
                    Throws.ArgumentException.With.Message.Contains("at full capacity"),
                    "BR-M-004: Single assignment should be rejected if over capacity");
            }
            else
            {
                Assert.That(() => minionService.AssignMinionToBase(1, base_),
                    Throws.Nothing,
                    "BR-M-004: Single assignment should succeed if within capacity");
            }
        }
        else
        {
            var minionIds = Enumerable.Range(1, newAssignments).ToList();
            if (shouldThrow)
            {
                Assert.That(() => minionService.AssignMinionsToBase(minionIds, base_),
                    Throws.ArgumentException.With.Message.Contains("would exceed capacity"),
                    "BR-M-004: Bulk assignment should be rejected if would exceed capacity");
            }
            else
            {
                Assert.That(() => minionService.AssignMinionsToBase(minionIds, base_),
                    Throws.Nothing,
                    "BR-M-004: Bulk assignment should succeed if within capacity");
            }
        }
    }

    // ===================== RULE 5: Specialty Validation (BR-M-005) =====================
    // Format: [specialty, shouldSucceed]

    [TestCase("Hacking", true)]
    [TestCase("Explosives", true)]
    [TestCase("Disguise", true)]
    [TestCase("Combat", true)]
    [TestCase("Engineering", true)]
    [TestCase("Piloting", true)]
    [TestCase("Magic", false)]           // Invalid specialty
    [TestCase("hacking", false)]         // Case-sensitive - wrong case
    public void BR_M_005_SpecialtyValidation(string specialty, bool shouldSucceed)
    {
        var minion = new Minion
        {
            MinionId = 1,
            Name = "Test Specialist",
            Specialty = specialty,
            SkillLevel = 8,
            SalaryDemand = 5000
        };
        var minionService = new MinionService(mockRepository);
        minionService.Minions = new Dictionary<int, Minion>();

        if (shouldSucceed)
        {
            var result = minionService.CreateMinion(minion);
            Assert.That(result, Is.Not.Null,
                $"BR-M-005: Valid specialty '{specialty}' should be accepted");
        }
        else
        {
            Assert.Throws<ArgumentException>(() => minionService.CreateMinion(minion),
                $"BR-M-005: Specialty '{specialty}' should be rejected");
        }
    }

    // ===================== RULE 6: Skill Level Range Validation (BR-M-006) =====================
    // Format: [skillLevel, shouldSucceed]

    [TestCase(1, true)]
    [TestCase(5, true)]
    [TestCase(10, true)]
    [TestCase(0, false)]
    [TestCase(-5, false)]
    [TestCase(11, false)]
    [TestCase(100, false)]
    public void BR_M_006_SkillLevelValidation(int skillLevel, bool shouldSucceed)
    {
        var minion = new Minion
        {
            MinionId = 1,
            Name = "Test Minion",
            Specialty = "Hacking",
            SkillLevel = skillLevel,
            SalaryDemand = 5000
        };
        var minionService = new MinionService(mockRepository);
        minionService.Minions = new Dictionary<int, Minion>();

        if (shouldSucceed)
        {
            var result = minionService.CreateMinion(minion);
            Assert.That(result, Is.Not.Null,
                $"BR-M-006: Skill level {skillLevel} should be valid (1-10)");
        }
        else
        {
            Assert.Throws<ArgumentException>(() => minionService.CreateMinion(minion),
                $"BR-M-006: Skill level {skillLevel} is invalid (must be 1-10)");
        }
    }

    // ===================== RULE 7: Salary Demand Validation (BR-M-007) =====================
    // Format: [salary, shouldSucceed, description]

    [TestCase(5000, true)]           // Valid salary
    [TestCase(1500000, true)]        // Unusually high but allowed (with warning)
    [TestCase(0, false)]             // Invalid: zero
    [TestCase(-1000, false)]         // Invalid: negative
    public void BR_M_007_SalaryDemandValidation(decimal salary, bool shouldSucceed)
    {
        var minion = new Minion
        {
            MinionId = 1,
            Name = "Test Minion",
            Specialty = "Hacking",
            SkillLevel = 8,
            SalaryDemand = salary
        };
        var minionService = new MinionService(mockRepository);
        minionService.Minions = new Dictionary<int, Minion>();

        if (shouldSucceed)
        {
            var result = minionService.CreateMinion(minion);
            Assert.That(result, Is.Not.Null,
                $"BR-M-007: Salary demand {salary} should be allowed");
        }
        else
        {
            Assert.Throws<ArgumentException>(() => minionService.CreateMinion(minion),
                $"BR-M-007: Salary demand {salary} must be positive");
        }
    }
}
