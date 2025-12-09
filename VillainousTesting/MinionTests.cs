using VillainLairManager.Models;
using VillainLairManager.Services;
using NSubstitute;
using NUnit.Framework;
using VillainLairManager;
namespace VillainousTesting;

public class MinionTests
{
    private IRepository mockRepository;
    [SetUp]
    public void Setup()
    {
        mockRepository = Substitute.For<IRepository>();
    }

    [Test]
    public void TestUpdateMood()
    {
        MinionService minionService = new(mockRepository);
        minionService.Minions = new Dictionary<int, Minion>
        {
            { 1, new Minion { MinionId = 1, Name = "Test Minion", LoyaltyScore = 80, MoodStatus = "" } }
        };
        minionService.UpdateMood(1);

    }

    [Test]
    public void updateLoyaltyScoreSatisfiedMinion()
    {
        MinionService minionService = new(mockRepository);
        minionService.Minions = new Dictionary<int, Minion>
        {
            { 1, new Minion { MinionId = 1, Name = "Test Satisfied Minion", LoyaltyScore = 70, SalaryDemand=5000} }
        };
        minionService.UpdateLoyalty(1, 5000);
        Assert.That(minionService.Minions[1].LoyaltyScore, Is.EqualTo(73));
    }

    // CRUD Operation Tests

    [Test]
    public void GetMinionById_ReturnsMinion_WhenExists()
    {
        var minion = new Minion { MinionId = 1, Name = "Test Minion", LoyaltyScore = 50 };
        var minionService = new MinionService(mockRepository);
        minionService.Minions = new Dictionary<int, Minion> { { 1, minion } };

        var result = minionService.GetMinionById(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.MinionId, Is.EqualTo(1));
        Assert.That(result.Name, Is.EqualTo("Test Minion"));
    }

    [Test]
    public void GetMinionById_ReturnsNull_WhenNotExists()
    {
        var minionService = new MinionService(mockRepository);
        minionService.Minions = new Dictionary<int, Minion>();

        var result = minionService.GetMinionById(999);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetAllMinions_ReturnsAllMinions()
    {
        var minions = new Dictionary<int, Minion>
        {
            { 1, new Minion { MinionId = 1, Name = "Minion 1", LoyaltyScore = 50 } },
            { 2, new Minion { MinionId = 2, Name = "Minion 2", LoyaltyScore = 70 } },
            { 3, new Minion { MinionId = 3, Name = "Minion 3", LoyaltyScore = 30 } }
        };
        var minionService = new MinionService(mockRepository);
        minionService.Minions = minions;

        var result = minionService.GetAllMinions().ToList();

        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result.Any(m => m.MinionId == 1), Is.True);
        Assert.That(result.Any(m => m.MinionId == 2), Is.True);
        Assert.That(result.Any(m => m.MinionId == 3), Is.True);
    }

    [Test]
    public void CreateMinion_AddsToCache_AndCallsRepository()
    {
        var newMinion = new Minion { MinionId = 5, Name = "New Minion", LoyaltyScore = 50 };
        var minionService = new MinionService(mockRepository);
        minionService.Minions = new Dictionary<int, Minion>();

        var result = minionService.CreateMinion(newMinion);

        Assert.That(result, Is.EqualTo(newMinion));
        Assert.That(minionService.Minions.ContainsKey(5), Is.True);
        mockRepository.Received(1).InsertMinion(newMinion);
    }

    [Test]
    public void UpdateMinion_UpdatesCache_AndCallsRepository()
    {
        var minion = new Minion { MinionId = 1, Name = "Original", LoyaltyScore = 50 };
        var minionService = new MinionService(mockRepository);
        minionService.Minions = new Dictionary<int, Minion> { { 1, minion } };

        minion.Name = "Updated";
        minion.LoyaltyScore = 75;
        minionService.UpdateMinion(minion);

        Assert.That(minionService.Minions[1].Name, Is.EqualTo("Updated"));
        Assert.That(minionService.Minions[1].LoyaltyScore, Is.EqualTo(75));
        mockRepository.Received(1).UpdateMinion(minion);
    }

    [Test]
    public void UpdateMinion_DoesNotUpdate_WhenMinionNotExists()
    {
        var minion = new Minion { MinionId = 999, Name = "Nonexistent", LoyaltyScore = 50 };
        var minionService = new MinionService(mockRepository);
        minionService.Minions = new Dictionary<int, Minion>();

        minionService.UpdateMinion(minion);

        mockRepository.DidNotReceive().UpdateMinion(Arg.Any<Minion>());
    }

    [Test]
    public void DeleteMinion_RemovesFromCache_AndCallsRepository()
    {
        var minion = new Minion { MinionId = 1, Name = "To Delete", LoyaltyScore = 50 };
        var minionService = new MinionService(mockRepository);
        minionService.Minions = new Dictionary<int, Minion> { { 1, minion } };

        minionService.DeleteMinion(1);

        Assert.That(minionService.Minions.ContainsKey(1), Is.False);
        mockRepository.Received(1).DeleteMinion(1);
    }

    [Test]
    public void DeleteMinion_DoesNotDelete_WhenMinionNotExists()
    {
        var minionService = new MinionService(mockRepository);
        minionService.Minions = new Dictionary<int, Minion>();

        minionService.DeleteMinion(999);

        mockRepository.DidNotReceive().DeleteMinion(Arg.Any<int>());
    }
}


