using VillainLairManager.Models;
using VillainLairManager.Services;
using NSubstitute;
using NUnit.Framework;
using VillainLairManager;
using System;
using System.Collections.Generic;

namespace VillainousTesting;

/// <summary>
/// Integration tests for EvilSchemeService
/// Tests the interaction between service methods, business rules, and database operations
/// </summary>
public class EvilSchemeServiceTests
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

    // ===================== CRUD Operations =====================

    [Test]
    public void UpdateSuccessLikelihood_CalculatesAndPersists()
    {
        var minions = new List<Minion>
        {
            new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Hacking" },
            new Minion { MinionId = 2, CurrentSchemeId = 1, Specialty = "Hacking" }
        };
        mockRepository.GetAllMinions().Returns(minions);
        mockRepository.GetAllEquipment().Returns(new List<Equipment>());

        schemeService.UpdateSuccessLikelihood(1);

        // Base 50 + (2 * 10) bonus + 0 equipment = 70 (no resource penalty since we have 2 minions and 1+ matching)
        Assert.That(testScheme.SuccessLikelihood, Is.EqualTo(70),
            "UpdateSuccessLikelihood should calculate and store success in scheme");
        mockRepository.Received(1).UpdateScheme(testScheme);
    }

    [Test]
    public void IsOverBudget_ReturnsCorrectStatus()
    {
        testScheme.CurrentSpending = 50000m;
        testScheme.Budget = 100000m;

        bool result = schemeService.IsOverBudget(1);

        Assert.That(result, Is.False);

        testScheme.CurrentSpending = 100001m;
        result = schemeService.IsOverBudget(1);

        Assert.That(result, Is.True);
    }

    [Test]
    public void GetRemainingBudget_CalculatesCorrectly()
    {
        testScheme.CurrentSpending = 30000m;
        testScheme.Budget = 100000m;

        decimal remaining = schemeService.GetRemainingBudget(1);

        Assert.That(remaining, Is.EqualTo(70000m));
    }

    [Test]
    public void CanAfford_ValidatesAffordability()
    {
        testScheme.CurrentSpending = 80000m;
        testScheme.Budget = 100000m;
        // Remaining budget: 20000

        // >= 20001 is false (20000 is not >= 20001)
        Assert.That(schemeService.CanAfford(1, 20001m), Is.False);
        // >= 20000 is true (20000 is >= 20000)  
        Assert.That(schemeService.CanAfford(1, 20000m), Is.True);
        // >= 19999 is true
        Assert.That(schemeService.CanAfford(1, 19999m), Is.True);
    }

    // ===================== Budget Status Integration =====================

    [Test]
    public void ValidateBudgetStatus_UpdatesAllowNewAssignments()
    {
        testScheme.CurrentSpending = 150000m;
        testScheme.Budget = 100000m;

        schemeService.ValidateBudgetStatus(1);

        Assert.That(testScheme.AllowNewAssignments, Is.False);
    }

    [Test]
    public void CalculateEstimatedSpending_RejectsIfOverBudget()
    {
        testScheme.CurrentSpending = 95000m;
        testScheme.Budget = 100000m;
        var minion = new Minion { SalaryDemand = 10000m };
        testScheme.TargetCompletionDate = DateTime.Now.AddMonths(1);

        var (estimated, newTotal, wouldExceed) = schemeService.CalculateEstimatedSpending(1, minion);

        Assert.That(wouldExceed, Is.True);
    }

    // ===================== Status Transitions with Side Effects =====================

    [Test]
    public void CanTransitionToStatus_ValidatesBeforeTransition()
    {
        testScheme.Status = "Planning";
        testScheme.StartDate = null;
        mockRepository.GetAllMinions().Returns(new List<Minion>());

        var (canTransition, errors) = schemeService.CanTransitionToStatus(1, "Active");

        Assert.That(canTransition, Is.False);
        Assert.That(errors.Count, Is.GreaterThan(0));
    }

    [Test]
    public void CanTransitionToStatus_MultipleErrorsReturned()
    {
        testScheme.Status = "Planning";
        testScheme.StartDate = null;
        testScheme.CurrentSpending = 150000m;
        testScheme.Budget = 100000m;

        mockRepository.GetAllMinions().Returns(new List<Minion>());

        var (canTransition, errors) = schemeService.CanTransitionToStatus(1, "Active");

        Assert.That(canTransition, Is.False);
        Assert.That(errors.Count, Is.GreaterThanOrEqualTo(3), 
            "Should have errors for StartDate, minions, and budget");
    }

    // ===================== Resource Requirements Integration =====================

    [Test]
    public void GetResourceRequirements_ConsistentWithDiabolicalRating()
    {
        var (minMin5, minEquip5, doomsday5) = schemeService.GetResourceRequirements(5);
        var (minMin8, minEquip8, doomsday8) = schemeService.GetResourceRequirements(8);

        Assert.That(minMin5, Is.LessThan(minMin8), "Higher rating should require more minions");
        Assert.That(minEquip5, Is.LessThan(minEquip8), "Higher rating should require more equipment");
        Assert.That(doomsday5, Is.False);
        Assert.That(doomsday8, Is.True);
    }

    [Test]
    public void ValidateResourceRequirements_WarningsForInsufficient()
    {
        testScheme.DiabolicalRating = 8;

        var (isMet, warnings) = schemeService.ValidateResourceRequirements(1, 2, 1, false);

        Assert.That(isMet, Is.False);
        Assert.That(warnings.Count, Is.GreaterThan(0));
    }

    // ===================== Deadline Integration =====================

    [Test]
    public void GetDeadlineStatus_ChangesWithTime()
    {
        testScheme.TargetCompletionDate = DateTime.Now.AddDays(60);
        Assert.That(schemeService.GetDeadlineStatus(1), Is.EqualTo("On track"));

        testScheme.TargetCompletionDate = DateTime.Now.AddDays(15);
        Assert.That(schemeService.GetDeadlineStatus(1), Is.EqualTo("Due soon"));

        testScheme.TargetCompletionDate = DateTime.Now.AddDays(5);
        Assert.That(schemeService.GetDeadlineStatus(1), Is.EqualTo("Urgent"));

        testScheme.TargetCompletionDate = DateTime.Now.AddDays(-5);
        Assert.That(schemeService.GetDeadlineStatus(1), Is.EqualTo("Overdue"));
    }

    // ===================== Specialty Matching Integration =====================

    [Test]
    public void ValidateSpecialtyMatching_WarnsOnlyOne()
    {
        var minions = new List<Minion>
        {
            new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Hacking" }
        };
        mockRepository.GetAllMinions().Returns(minions);

        var (hasRequired, count, warnings) = schemeService.ValidateSpecialtyMatching(1);

        Assert.That(hasRequired, Is.True);
        Assert.That(count, Is.EqualTo(1));
        Assert.That(warnings, Contains.Item("Only one minion with required specialty - risky!"));
    }

    [Test]
    public void ValidateSpecialtyMatching_NoWarningsForMultiple()
    {
        var minions = new List<Minion>
        {
            new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Hacking" },
            new Minion { MinionId = 2, CurrentSchemeId = 1, Specialty = "Hacking" }
        };
        mockRepository.GetAllMinions().Returns(minions);

        var (hasRequired, count, warnings) = schemeService.ValidateSpecialtyMatching(1);

        Assert.That(warnings, Is.Empty);
    }

    // ===================== Auto Transitions with Database Persistence =====================

    [Test]
    public void ApplyAutoTransitions_PersistsChanges()
    {
        testScheme.Status = "Active";
        testScheme.SuccessLikelihood = 75;
        testScheme.TargetCompletionDate = DateTime.Now.AddDays(-5);

        schemeService.ApplyAutoTransitions(1);

        Assert.That(testScheme.Status, Is.EqualTo("Completed"));
        mockRepository.Received(1).UpdateScheme(testScheme);
    }

    [Test]
    public void ApplyAutoTransitions_FailsAndPersists()
    {
        testScheme.Status = "Active";
        testScheme.SuccessLikelihood = 20;
        testScheme.TargetCompletionDate = DateTime.Now.AddDays(-5);

        schemeService.ApplyAutoTransitions(1);

        Assert.That(testScheme.Status, Is.EqualTo("Failed"));
        mockRepository.Received(1).UpdateScheme(testScheme);
    }

    [Test]
    public void ApplyAutoTransitions_NoChangeIfNotOverdue()
    {
        testScheme.Status = "Active";
        testScheme.SuccessLikelihood = 20;
        testScheme.TargetCompletionDate = DateTime.Now.AddDays(30);

        schemeService.ApplyAutoTransitions(1);

        Assert.That(testScheme.Status, Is.EqualTo("Active"));
        mockRepository.DidNotReceive().UpdateScheme(testScheme);
    }

    // ===================== Complex Scenarios =====================

    [Test]
    public void CompleteWorkflow_NewScheme()
    {
        // Scenario: Create a new scheme, add resources, calculate success, activate, auto-complete
        var newScheme = new EvilScheme
        {
            SchemeId = 2,
            Name = "New Scheme",
            Budget = 50000m,
            CurrentSpending = 0m,
            RequiredSpecialty = "Combat",
            Status = "Planning",
            TargetCompletionDate = DateTime.Now.AddDays(-1),
            DiabolicalRating = 3,
            SuccessLikelihood = 0
        };
        schemeService.Schemes[2] = newScheme;

        // Add minions and equipment
        var minions = new List<Minion>
        {
            new Minion { MinionId = 1, CurrentSchemeId = 2, Specialty = "Combat" },
            new Minion { MinionId = 2, CurrentSchemeId = 2, Specialty = "Combat" }
        };
        mockRepository.GetAllMinions().Returns(minions);
        mockRepository.GetAllEquipment().Returns(new List<Equipment>());

        // Calculate success
        schemeService.UpdateSuccessLikelihood(2);
        Assert.That(newScheme.SuccessLikelihood, Is.GreaterThan(0));

        // Check if can activate (should fail due to no StartDate)
        var (canActivate, errors) = schemeService.CanTransitionToStatus(2, "Active");
        Assert.That(canActivate, Is.False);

        // Set start date and try again
        newScheme.StartDate = DateTime.Now;
        var (canActivate2, errors2) = schemeService.CanTransitionToStatus(2, "Active");
        Assert.That(canActivate2, Is.True);

        // Activate and auto-transition
        newScheme.Status = "Active";
        newScheme.SuccessLikelihood = 75;
        schemeService.ApplyAutoTransitions(2);

        Assert.That(newScheme.Status, Is.EqualTo("Completed"));
    }

    [Test]
    public void BudgetMonitoring_Workflow()
    {
        // Scenario: Monitor budget as minions are added
        testScheme.CurrentSpending = 0m;
        testScheme.Budget = 50000m;

        var (status1, allow1) = schemeService.ValidateBudgetStatus(1);
        Assert.That(allow1, Is.True);
        Assert.That(status1, Is.EqualTo("Within Budget"));

        // Exactly at 90% threshold: 45000 > 45000? NO, so still Within Budget
        testScheme.CurrentSpending = 45000m;
        var (status2, allow2) = schemeService.ValidateBudgetStatus(1);
        Assert.That(status2, Is.EqualTo("Within Budget"));

        // Above 90% threshold: 45001 > 45000? YES, so Approaching
        testScheme.CurrentSpending = 45001m;
        var (status2b, allow2b) = schemeService.ValidateBudgetStatus(1);
        Assert.That(status2b, Is.EqualTo("Approaching Budget Limit"));

        // Over budget: 51000 > 50000? YES
        testScheme.CurrentSpending = 51000m;
        var (status3, allow3) = schemeService.ValidateBudgetStatus(1);
        Assert.That(status3, Is.EqualTo("Over Budget - Action Required"));
        Assert.That(allow3, Is.False);
    }

    [Test]
    public void SuccessLikelihood_AffectedByMultipleFactors()
    {
        // Start with base scenario
        var minions = new List<Minion>
        {
            new Minion { MinionId = 1, CurrentSchemeId = 1, Specialty = "Hacking" },
            new Minion { MinionId = 2, CurrentSchemeId = 1, Specialty = "Hacking" }
        };
        var equipment = new List<Equipment>
        {
            new Equipment { EquipmentId = 1, AssignedToSchemeId = 1, Condition = 100 }
        };

        mockRepository.GetAllMinions().Returns(minions);
        mockRepository.GetAllEquipment().Returns(equipment);

        schemeService.UpdateSuccessLikelihood(1);
        int baseSuccess = testScheme.SuccessLikelihood;

        // Add another minion
        minions.Add(new Minion { MinionId = 3, CurrentSchemeId = 1, Specialty = "Hacking" });
        schemeService.UpdateSuccessLikelihood(1);
        int withExtraMinion = testScheme.SuccessLikelihood;

        Assert.That(withExtraMinion, Is.GreaterThan(baseSuccess));

        // Go over budget
        testScheme.CurrentSpending = 150000m;
        schemeService.UpdateSuccessLikelihood(1);
        int overBudget = testScheme.SuccessLikelihood;

        Assert.That(overBudget, Is.LessThan(withExtraMinion));
    }
}
