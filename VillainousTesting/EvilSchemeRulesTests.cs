using VillainLairManager.Models;
using VillainLairManager.Services;
using NSubstitute;
using NUnit.Framework;
using VillainLairManager;
using System;
using System.Collections.Generic;

namespace VillainousTesting;

/// <summary>
/// Comprehensive tests for Evil Scheme Business Rules (BR-S-001 through BR-S-007)
/// These tests validate rule compliance and expected behavior.
/// </summary>
public class EvilSchemeRulesTests
{
    private IRepository mockRepository;
    private EvilSchemeService schemeService;
    private EvilScheme testScheme;

    [SetUp]
    public void Setup()
    {
        mockRepository = Substitute.For<IRepository>();
        schemeService = new EvilSchemeService(mockRepository);
        schemeService.Schemes = new Dictionary<int, EvilScheme>();

        // Create a base test scheme
        testScheme = new EvilScheme
        {
            SchemeId = 1,
            Name = "Test Scheme",
            Description = "A test scheme",
            Budget = 100000m,
            CurrentSpending = 0m,
            RequiredSkillLevel = 5,
            RequiredSpecialty = "Hacking",
            Status = "Planning",
            StartDate = null,
            TargetCompletionDate = DateTime.Now.AddMonths(1),
            DiabolicalRating = 5,
            SuccessLikelihood = 50
        };

        schemeService.Schemes[1] = testScheme;
    }

    // ===================== RULE 1: Success Likelihood Calculation (BR-S-001) =====================

    [Test]
    public void BR_S_001_SuccessLikelihood_BaseCase_NoResources()
    {
        // No minions or equipment assigned, no penalties
        mockRepository.GetAllMinions().Returns(new List<Minion>());
        mockRepository.GetAllEquipment().Returns(new List<Equipment>());

        int result = schemeService.CalculateSuccessLikelihood(1);

        // Base 50 - 15 resource penalty = 35
        Assert.That(result, Is.EqualTo(35), 
            "BR-S-001: Base success 50 minus resource penalty 15 should equal 35");
    }

    [Test]
    public void BR_S_001_SuccessLikelihood_WellResourced()
    {
        // 3 minions with matching specialty, 4 equipment with good condition
        var minions = new List<Minion>
        {
            new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Hacking", SkillLevel = 5 },
            new Minion { MinionId = 2, CurrentSchemeId = 1, Specialty = "Hacking", SkillLevel = 5 },
            new Minion { MinionId = 3, CurrentSchemeId = 1, Specialty = "Hacking", SkillLevel = 5 }
        };
        var equipment = new List<Equipment>
        {
            new Equipment { EquipmentId = 1, AssignedToSchemeId = 1, Condition = 100 },
            new Equipment { EquipmentId = 2, AssignedToSchemeId = 1, Condition = 90 },
            new Equipment { EquipmentId = 3, AssignedToSchemeId = 1, Condition = 80 },
            new Equipment { EquipmentId = 4, AssignedToSchemeId = 1, Condition = 70 }
        };

        mockRepository.GetAllMinions().Returns(minions);
        mockRepository.GetAllEquipment().Returns(equipment);

        int result = schemeService.CalculateSuccessLikelihood(1);

        // Base 50 + (3 * 10) + (4 * 5) = 50 + 30 + 20 = 100 (clamped)
        Assert.That(result, Is.EqualTo(100), 
            "BR-S-001: Well resourced scheme should achieve 100% success likelihood");
    }

    [Test]
    public void BR_S_001_SuccessLikelihood_OverBudget()
    {
        testScheme.CurrentSpending = 100001m; // Over budget
        var minions = new List<Minion>
        {
            new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Hacking", SkillLevel = 5 },
            new Minion { MinionId = 2, CurrentSchemeId = 1, Specialty = "Hacking", SkillLevel = 5 }
        };
        var equipment = new List<Equipment>
        {
            new Equipment { EquipmentId = 1, AssignedToSchemeId = 1, Condition = 100 },
            new Equipment { EquipmentId = 2, AssignedToSchemeId = 1, Condition = 90 }
        };

        mockRepository.GetAllMinions().Returns(minions);
        mockRepository.GetAllEquipment().Returns(equipment);

        int result = schemeService.CalculateSuccessLikelihood(1);

        // Base 50 + (2 * 10) + (2 * 5) - 20 budget penalty = 50 + 20 + 10 - 20 = 60
        Assert.That(result, Is.EqualTo(60), 
            "BR-S-001: Over budget scheme should have -20 penalty");
    }

    [Test]
    public void BR_S_001_SuccessLikelihood_DeadlinePassed()
    {
        testScheme.TargetCompletionDate = DateTime.Now.AddDays(-5); // Deadline passed
        var minions = new List<Minion>
        {
            new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Hacking", SkillLevel = 5 },
            new Minion { MinionId = 2, CurrentSchemeId = 1, Specialty = "Hacking", SkillLevel = 5 }
        };
        var equipment = new List<Equipment>
        {
            new Equipment { EquipmentId = 1, AssignedToSchemeId = 1, Condition = 100 },
            new Equipment { EquipmentId = 2, AssignedToSchemeId = 1, Condition = 90 }
        };

        mockRepository.GetAllMinions().Returns(minions);
        mockRepository.GetAllEquipment().Returns(equipment);

        int result = schemeService.CalculateSuccessLikelihood(1);

        // Base 50 + (2 * 10) + (2 * 5) - 25 timeline penalty = 50 + 20 + 10 - 25 = 55
        Assert.That(result, Is.EqualTo(55), 
            "BR-S-001: Overdue scheme should have -25 timeline penalty");
    }

    [Test]
    public void BR_S_001_SuccessLikelihood_CompleteFailure()
    {
        testScheme.CurrentSpending = 150000m; // Over budget
        testScheme.TargetCompletionDate = DateTime.Now.AddDays(-5); // Deadline passed
        mockRepository.GetAllMinions().Returns(new List<Minion>());
        mockRepository.GetAllEquipment().Returns(new List<Equipment>());

        int result = schemeService.CalculateSuccessLikelihood(1);

        // Base 50 - 20 budget - 15 resource - 25 timeline = -10, clamped to 0
        Assert.That(result, Is.EqualTo(0), 
            "BR-S-001: Complete failure scenario should be clamped at 0%");
    }

    [Test]
    public void BR_S_001_SuccessLikelihood_EquipmentConditionBelow50()
    {
        var minions = new List<Minion>
        {
            new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Hacking", SkillLevel = 5 }
        };
        var equipment = new List<Equipment>
        {
            new Equipment { EquipmentId = 1, AssignedToSchemeId = 1, Condition = 30 }, // Below 50
            new Equipment { EquipmentId = 2, AssignedToSchemeId = 1, Condition = 60 }  // Above 50
        };

        mockRepository.GetAllMinions().Returns(minions);
        mockRepository.GetAllEquipment().Returns(equipment);

        int result = schemeService.CalculateSuccessLikelihood(1);

        // Only equipment with condition >= 50 counts: Base 50 + 10 + 5 - 15 = 50
        Assert.That(result, Is.EqualTo(50), 
            "BR-S-001: Equipment with condition < 50 should not count");
    }

    [Test]
    public void BR_S_001_SuccessLikelihood_MinionSpecialtyMismatch()
    {
        var minions = new List<Minion>
        {
            new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Combat", SkillLevel = 5 }, // Wrong specialty
            new Minion { MinionId = 2, CurrentSchemeId = 1, Specialty = "Hacking", SkillLevel = 5 }  // Correct
        };
        mockRepository.GetAllMinions().Returns(minions);
        mockRepository.GetAllEquipment().Returns(new List<Equipment>());

        int result = schemeService.CalculateSuccessLikelihood(1);

        // Base 50 + (1 * 10) bonus for matching. 2 minions >= 2 AND 1 matching >= 1, so NO resource penalty = 60
        Assert.That(result, Is.EqualTo(60), 
            "BR-S-001: Only matching minions provide bonus, but total minions count toward resource requirements");
    }

    // ===================== RULE 2: Budget Enforcement (BR-S-002) =====================

    [Test]
    public void BR_S_002_ValidateBudgetStatus_WithinBudget()
    {
        testScheme.CurrentSpending = 50000m;
        testScheme.Budget = 100000m;

        var (status, allowNew) = schemeService.ValidateBudgetStatus(1);

        Assert.That(status, Is.EqualTo("Within Budget"));
        Assert.That(allowNew, Is.True);
    }

    [Test]
    public void BR_S_002_ValidateBudgetStatus_ApproachingLimit()
    {
        testScheme.CurrentSpending = 91000m;
        testScheme.Budget = 100000m;

        var (status, allowNew) = schemeService.ValidateBudgetStatus(1);

        Assert.That(status, Is.EqualTo("Approaching Budget Limit"));
        Assert.That(allowNew, Is.True);
    }

    [Test]
    public void BR_S_002_ValidateBudgetStatus_OverBudget()
    {
        testScheme.CurrentSpending = 100001m;
        testScheme.Budget = 100000m;

        var (status, allowNew) = schemeService.ValidateBudgetStatus(1);

        Assert.That(status, Is.EqualTo("Over Budget - Action Required"));
        Assert.That(allowNew, Is.False);
    }

    [Test]
    public void BR_S_002_ValidateBudgetStatus_ExactlyAtBudget()
    {
        testScheme.CurrentSpending = 100000m;
        testScheme.Budget = 100000m;

        var (status, allowNew) = schemeService.ValidateBudgetStatus(1);

        // Per spec: exactly at budget should be "Within Budget" (not "Approaching")
        // "Approaching" is only for values > 90% AND < 100%
        Assert.That(status, Is.EqualTo("Within Budget"));
        Assert.That(allowNew, Is.True);
    }

    [Test]
    public void BR_S_002_CalculateEstimatedSpending_OneMonth()
    {
        testScheme.TargetCompletionDate = DateTime.Now.AddDays(15); // 15 days ≈ 0.5 months, rounds to 1
        var minion = new Minion { SalaryDemand = 5000m };

        var (estimated, newTotal, wouldExceed) = schemeService.CalculateEstimatedSpending(1, minion);

        Assert.That(estimated, Is.EqualTo(5000m), "At minimum, should estimate 1 month");
        Assert.That(newTotal, Is.EqualTo(5000m));
        Assert.That(wouldExceed, Is.False);
    }

    [Test]
    public void BR_S_002_CalculateEstimatedSpending_MultipleMonths()
    {
        testScheme.TargetCompletionDate = DateTime.Now.AddDays(90); // ~3 months
        testScheme.CurrentSpending = 10000m;
        var minion = new Minion { SalaryDemand = 5000m };

        var (estimated, newTotal, wouldExceed) = schemeService.CalculateEstimatedSpending(1, minion);

        // Should be approximately 3 months * 5000
        Assert.That(estimated, Is.GreaterThan(12000m).And.LessThan(16000m));
        Assert.That(newTotal, Is.GreaterThan(22000m).And.LessThan(26000m));
        Assert.That(wouldExceed, Is.False);
    }

    [Test]
    public void BR_S_002_CalculateEstimatedSpending_WouldExceedBudget()
    {
        testScheme.TargetCompletionDate = DateTime.Now.AddMonths(1);
        testScheme.CurrentSpending = 95000m;
        testScheme.Budget = 100000m;
        var minion = new Minion { SalaryDemand = 10000m };

        var (estimated, newTotal, wouldExceed) = schemeService.CalculateEstimatedSpending(1, minion);

        Assert.That(wouldExceed, Is.True, "Should detect spending would exceed budget");
        Assert.That(newTotal, Is.GreaterThan(testScheme.Budget));
    }

    // ===================== RULE 3: Status Transitions (BR-S-003) =====================

    [Test]
    public void BR_S_003_CanTransitionToStatus_PlanningToActive_Valid()
    {
        testScheme.Status = "Planning";
        testScheme.StartDate = DateTime.Now;
        testScheme.CurrentSpending = 50000m;

        var minions = new List<Minion>
        {
            new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Hacking" },
            new Minion { MinionId = 2, CurrentSchemeId = 1, Specialty = "Hacking" }
        };
        mockRepository.GetAllMinions().Returns(minions);

        var (canTransition, errors) = schemeService.CanTransitionToStatus(1, "Active");

        Assert.That(canTransition, Is.True);
        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void BR_S_003_CanTransitionToStatus_PlanningToActive_NoStartDate()
    {
        testScheme.Status = "Planning";
        testScheme.StartDate = null; // Missing start date
        var minions = new List<Minion>
        {
            new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Hacking" },
            new Minion { MinionId = 2, CurrentSchemeId = 1, Specialty = "Hacking" }
        };
        mockRepository.GetAllMinions().Returns(minions);

        var (canTransition, errors) = schemeService.CanTransitionToStatus(1, "Active");

        Assert.That(canTransition, Is.False);
        Assert.That(errors, Contains.Item("StartDate must be set before activating"));
    }

    [Test]
    public void BR_S_003_CanTransitionToStatus_PlanningToActive_InsufficientMinions()
    {
        testScheme.Status = "Planning";
        testScheme.StartDate = DateTime.Now;
        var minions = new List<Minion>
        {
            new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Hacking" }
        };
        mockRepository.GetAllMinions().Returns(minions);

        var (canTransition, errors) = schemeService.CanTransitionToStatus(1, "Active");

        Assert.That(canTransition, Is.False);
        Assert.That(errors, Contains.Item("At least 2 minions must be assigned"));
    }

    [Test]
    public void BR_S_003_CanTransitionToStatus_PlanningToActive_NoMatchingSpecialty()
    {
        testScheme.Status = "Planning";
        testScheme.StartDate = DateTime.Now;
        var minions = new List<Minion>
        {
            new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Combat" },
            new Minion { MinionId = 2, CurrentSchemeId = 1, Specialty = "Combat" }
        };
        mockRepository.GetAllMinions().Returns(minions);

        var (canTransition, errors) = schemeService.CanTransitionToStatus(1, "Active");

        Assert.That(canTransition, Is.False);
        Assert.That(errors, Contains.Item("At least 1 minion with required specialty must be assigned"));
    }

    [Test]
    public void BR_S_003_CanTransitionToStatus_ActiveToCompleted_Valid()
    {
        testScheme.Status = "Active";
        testScheme.SuccessLikelihood = 75;
        testScheme.TargetCompletionDate = DateTime.Now.AddDays(-1);

        var (canTransition, errors) = schemeService.CanTransitionToStatus(1, "Completed");

        Assert.That(canTransition, Is.True);
        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void BR_S_003_CanTransitionToStatus_ActiveToCompleted_LowSuccess()
    {
        testScheme.Status = "Active";
        testScheme.SuccessLikelihood = 60;
        testScheme.TargetCompletionDate = DateTime.Now.AddDays(-1);

        var (canTransition, errors) = schemeService.CanTransitionToStatus(1, "Completed");

        Assert.That(canTransition, Is.False);
        Assert.That(errors, Contains.Item("Success likelihood must be at least 70% to complete"));
    }

    [Test]
    public void BR_S_003_CanTransitionToStatus_ActiveToOnHold_Always()
    {
        testScheme.Status = "Active";

        var (canTransition, errors) = schemeService.CanTransitionToStatus(1, "On Hold");

        Assert.That(canTransition, Is.True);
    }

    [Test]
    public void BR_S_003_CanTransitionToStatus_AnyStatusToPlanning_Always()
    {
        testScheme.Status = "Active";

        var (canTransition, errors) = schemeService.CanTransitionToStatus(1, "Planning");

        Assert.That(canTransition, Is.True);
    }

    [Test]
    public void BR_S_003_CanTransitionToStatus_ActiveToFailed_Always()
    {
        testScheme.Status = "Active";

        var (canTransition, errors) = schemeService.CanTransitionToStatus(1, "Failed");

        Assert.That(canTransition, Is.True);
    }

    // ===================== RULE 4: Resource Requirements (BR-S-004) =====================

    [Test]
    public void BR_S_004_GetResourceRequirements_HighDiabolical()
    {
        var (minMinions, minEquipment, requiresDoomsday) = schemeService.GetResourceRequirements(9);

        Assert.That(minMinions, Is.EqualTo(3));
        Assert.That(minEquipment, Is.EqualTo(2));
        Assert.That(requiresDoomsday, Is.True);
    }

    [Test]
    public void BR_S_004_GetResourceRequirements_MediumDiabolical()
    {
        var (minMinions, minEquipment, requiresDoomsday) = schemeService.GetResourceRequirements(6);

        Assert.That(minMinions, Is.EqualTo(2));
        Assert.That(minEquipment, Is.EqualTo(1));
        Assert.That(requiresDoomsday, Is.False);
    }

    [Test]
    public void BR_S_004_GetResourceRequirements_LowDiabolical()
    {
        var (minMinions, minEquipment, requiresDoomsday) = schemeService.GetResourceRequirements(3);

        Assert.That(minMinions, Is.EqualTo(1));
        Assert.That(minEquipment, Is.EqualTo(0));
        Assert.That(requiresDoomsday, Is.False);
    }

    [Test]
    public void BR_S_004_ValidateResourceRequirements_HighDiabolical_WithDoomsday()
    {
        testScheme.DiabolicalRating = 9;

        var (isMet, warnings) = schemeService.ValidateResourceRequirements(1, 3, 2, true);

        Assert.That(isMet, Is.True);
        Assert.That(warnings, Is.Empty);
    }

    [Test]
    public void BR_S_004_ValidateResourceRequirements_HighDiabolical_NoDoomsday()
    {
        testScheme.DiabolicalRating = 9;

        var (isMet, warnings) = schemeService.ValidateResourceRequirements(1, 3, 2, false);

        Assert.That(isMet, Is.False);
        Assert.That(warnings, Contains.Item("Highly diabolical schemes require a doomsday device"));
    }

    [Test]
    public void BR_S_004_ValidateResourceRequirements_InsufficientMinions()
    {
        testScheme.DiabolicalRating = 5;

        var (isMet, warnings) = schemeService.ValidateResourceRequirements(1, 1, 1, false);

        Assert.That(isMet, Is.False);
        Assert.That(warnings, Is.Not.Empty);
    }

    // ===================== RULE 5: Deadline Management (BR-S-005) =====================

    [Test]
    public void BR_S_005_GetDeadlineStatus_OnTrack()
    {
        testScheme.TargetCompletionDate = DateTime.Now.AddDays(60);

        string status = schemeService.GetDeadlineStatus(1);

        Assert.That(status, Is.EqualTo("On track"));
    }

    [Test]
    public void BR_S_005_GetDeadlineStatus_DueSoon()
    {
        testScheme.TargetCompletionDate = DateTime.Now.AddDays(15);

        string status = schemeService.GetDeadlineStatus(1);

        Assert.That(status, Is.EqualTo("Due soon"));
    }

    [Test]
    public void BR_S_005_GetDeadlineStatus_Urgent()
    {
        testScheme.TargetCompletionDate = DateTime.Now.AddDays(5);

        string status = schemeService.GetDeadlineStatus(1);

        Assert.That(status, Is.EqualTo("Urgent"));
    }

    [Test]
    public void BR_S_005_GetDeadlineStatus_Overdue()
    {
        testScheme.TargetCompletionDate = DateTime.Now.AddDays(-5);

        string status = schemeService.GetDeadlineStatus(1);

        Assert.That(status, Is.EqualTo("Overdue"));
    }

    // ===================== RULE 6: Specialty Matching (BR-S-006) =====================

    [Test]
    public void BR_S_006_ValidateSpecialtyMatching_HasRequired()
    {
        var minions = new List<Minion>
        {
            new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Hacking" }
        };
        mockRepository.GetAllMinions().Returns(minions);

        var (hasRequired, count, warnings) = schemeService.ValidateSpecialtyMatching(1);

        Assert.That(hasRequired, Is.True);
        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public void BR_S_006_ValidateSpecialtyMatching_NoRequired()
    {
        var minions = new List<Minion>
        {
            new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Combat" }
        };
        mockRepository.GetAllMinions().Returns(minions);

        var (hasRequired, count, warnings) = schemeService.ValidateSpecialtyMatching(1);

        Assert.That(hasRequired, Is.False);
        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public void BR_S_006_ValidateSpecialtyMatching_MultipleMatching()
    {
        var minions = new List<Minion>
        {
            new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Hacking" },
            new Minion { MinionId = 2, CurrentSchemeId = 1, Specialty = "Hacking" }
        };
        mockRepository.GetAllMinions().Returns(minions);

        var (hasRequired, count, warnings) = schemeService.ValidateSpecialtyMatching(1);

        Assert.That(hasRequired, Is.True);
        Assert.That(count, Is.EqualTo(2));
        Assert.That(warnings, Is.Empty);
    }

    [Test]
    public void BR_S_006_ValidateSpecialtyMatching_OnlyOneWarning()
    {
        var minions = new List<Minion>
        {
            new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Hacking" }
        };
        mockRepository.GetAllMinions().Returns(minions);

        var (hasRequired, count, warnings) = schemeService.ValidateSpecialtyMatching(1);

        Assert.That(hasRequired, Is.True);
        Assert.That(warnings, Contains.Item("Only one minion with required specialty - risky!"));
    }

    // ===================== RULE 7: Budget Validation (BR-S-007) =====================

    [Test]
    public void BR_S_007_ValidateBudgetValues_Valid()
    {
        var (isValid, warnings) = schemeService.ValidateBudgetValues(50000m, 30000m);

        Assert.That(isValid, Is.True);
        Assert.That(warnings, Is.Empty);
    }

    [Test]
    public void BR_S_007_ValidateBudgetValues_TooLow()
    {
        var (isValid, warnings) = schemeService.ValidateBudgetValues(5000m, 30000m);

        Assert.That(isValid, Is.False);
        Assert.That(warnings, Contains.Item("Budget too low - minimum is 10,000 evil dollars"));
    }

    [Test]
    public void BR_S_007_ValidateBudgetValues_Insufficient()
    {
        var (isValid, warnings) = schemeService.ValidateBudgetValues(30000m, 45000m);

        Assert.That(isValid, Is.True);
        Assert.That(warnings, Contains.Item("Budget may be insufficient for resource requirements"));
    }

    [Test]
    public void BR_S_007_ValidateBudgetValues_Unrealistic()
    {
        var (isValid, warnings) = schemeService.ValidateBudgetValues(15000000m, 50000m);

        Assert.That(isValid, Is.True);
        Assert.That(warnings, Contains.Item("Budget seems unrealistic - are you sure?"));
    }

    // ===================== INTEGRATION: Auto Transitions (BR-S-003 & BR-S-005) =====================

    [Test]
    public void BR_S_003_S_005_ApplyAutoTransitions_OverdueWithHighSuccess()
    {
        testScheme.Status = "Active";
        testScheme.SuccessLikelihood = 75;
        testScheme.TargetCompletionDate = DateTime.Now.AddDays(-5);

        schemeService.ApplyAutoTransitions(1);

        Assert.That(testScheme.Status, Is.EqualTo("Completed"),
            "Overdue scheme with success >= 70% should auto-complete");
    }

    [Test]
    public void BR_S_003_S_005_ApplyAutoTransitions_OverdueWithLowSuccess()
    {
        testScheme.Status = "Active";
        testScheme.SuccessLikelihood = 25;
        testScheme.TargetCompletionDate = DateTime.Now.AddDays(-5);

        schemeService.ApplyAutoTransitions(1);

        Assert.That(testScheme.Status, Is.EqualTo("Failed"),
            "Overdue scheme with success < 30% should auto-fail");
    }

    [Test]
    public void BR_S_003_S_005_ApplyAutoTransitions_OverdueWithMediumSuccess()
    {
        testScheme.Status = "Active";
        testScheme.SuccessLikelihood = 50;
        testScheme.TargetCompletionDate = DateTime.Now.AddDays(-5);

        schemeService.ApplyAutoTransitions(1);

        Assert.That(testScheme.Status, Is.EqualTo("Active"),
            "Overdue scheme with 30-70% success should remain Active");
    }

    [Test]
    public void BR_S_003_S_005_ApplyAutoTransitions_NotOverdue()
    {
        testScheme.Status = "Active";
        testScheme.SuccessLikelihood = 25;
        testScheme.TargetCompletionDate = DateTime.Now.AddDays(30);

        schemeService.ApplyAutoTransitions(1);

        Assert.That(testScheme.Status, Is.EqualTo("Active"),
            "Non-overdue scheme should not auto-transition");
    }

    [Test]
    public void BR_S_003_S_005_ApplyAutoTransitions_PlanningStatus()
    {
        testScheme.Status = "Planning";
        testScheme.SuccessLikelihood = 25;
        testScheme.TargetCompletionDate = DateTime.Now.AddDays(-5);

        schemeService.ApplyAutoTransitions(1);

        Assert.That(testScheme.Status, Is.EqualTo("Planning"),
            "Planning status should not trigger auto-transitions");
    }
}
