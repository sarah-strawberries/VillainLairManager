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
}
