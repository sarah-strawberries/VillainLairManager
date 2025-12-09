using VillainLairManager.Models;
using VillainLairManager.Services;
using NSubstitute;
using NUnit.Framework;
using VillainLairManager;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VillainousTesting;

/// <summary>
/// Unit tests for Minion service operations
/// Tests basic service functionality with mocked repository
/// </summary>
public class MinionTests
{
    private IRepository mockRepository;
    [SetUp]
    public void Setup()
    {
        mockRepository = Substitute.For<IRepository>();
    }

    [TestCase(80, Description = "High loyalty minion mood update")]
    [TestCase(50, Description = "Medium loyalty minion mood update")]
    [TestCase(20, Description = "Low loyalty minion mood update")]
    public void TestUpdateMood(int loyaltyScore)
    {
        MinionService minionService = new(mockRepository);
        minionService.Minions = new Dictionary<int, Minion>
        {
            { 1, new Minion { MinionId = 1, Name = "Test Minion", LoyaltyScore = loyaltyScore, MoodStatus = "", LastMoodUpdate = DateTime.Now } }
        };
        
        minionService.UpdateMood(1);

        Assert.That(minionService.Minions[1].MoodStatus, Is.Not.Empty);
    }

    [TestCase(70, 5000, 5000, 73, Description = "Satisfied minion loyalty increase")]
    [TestCase(50, 5000, 4000, 45, Description = "Underpaid minion loyalty decrease")]
    [TestCase(95, 5000, 6000, 98, Description = "Overpaid minion clamped")]
    public void UpdateLoyaltyScore(int initialLoyalty, decimal salaryDemand, decimal amountPaid, int expectedLoyalty)
    {
        MinionService minionService = new(mockRepository);
        minionService.Minions = new Dictionary<int, Minion>
        {
            { 1, new Minion { MinionId = 1, Name = "Test Minion", LoyaltyScore = initialLoyalty, SalaryDemand = salaryDemand} }
        };
        
        minionService.UpdateLoyalty(1, amountPaid);
        
        Assert.That(minionService.Minions[1].LoyaltyScore, Is.EqualTo(expectedLoyalty));
    }

    [TestCase(1, "Test Minion", Description = "Get single minion")]
    [TestCase(2, "Another Minion", Description = "Get different minion")]
    public void GetMinionById_ReturnsMinion_WhenExists(int minionId, string expectedName)
    {
        var minion = new Minion { MinionId = minionId, Name = expectedName, LoyaltyScore = 50 };
        var minionService = new MinionService(mockRepository);
        minionService.Minions = new Dictionary<int, Minion> { { minionId, minion } };

        var result = minionService.GetMinionById(minionId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.MinionId, Is.EqualTo(minionId));
        Assert.That(result.Name, Is.EqualTo(expectedName));
    }

    [TestCase(999, Description = "Get non-existent minion")]
    [TestCase(0, Description = "Get with invalid ID")]
    public void GetMinionById_ReturnsNull_WhenNotExists(int minionId)
    {
        var minionService = new MinionService(mockRepository);
        minionService.Minions = new Dictionary<int, Minion>();

        var result = minionService.GetMinionById(minionId);

        Assert.That(result, Is.Null);
    }

    [TestCase(1, 3, Description = "Get all minions with 3 minions")]
    [TestCase(2, 5, Description = "Get all minions with 5 minions")]
    public void GetAllMinions_ReturnsAllMinions(int unused, int count)
    {
        var minions = new Dictionary<int, Minion>();
        for (int i = 1; i <= count; i++)
        {
            minions[i] = new Minion { MinionId = i, Name = $"Minion {i}", LoyaltyScore = 50 };
        }
        var minionService = new MinionService(mockRepository);
        minionService.Minions = minions;

        var result = minionService.GetAllMinions().ToList();

        Assert.That(result, Has.Count.EqualTo(count));
        for (int i = 1; i <= count; i++)
        {
            Assert.That(result.Any(m => m.MinionId == i), Is.True);
        }
    }

    [TestCase(5, "New Minion", "Hacking", Description = "Create hacking minion")]
    [TestCase(10, "Combat Expert", "Combat", Description = "Create combat minion")]
    public void CreateMinion_AddsToCache_AndCallsRepository(int minionId, string name, string specialty)
    {
        var newMinion = new Minion 
        { 
            MinionId = minionId, 
            Name = name, 
            LoyaltyScore = 50, 
            Specialty = specialty, 
            SkillLevel = 5, 
            SalaryDemand = 5000m,
            MoodStatus = "Happy",
            LastMoodUpdate = DateTime.Now
        };
        var minionService = new MinionService(mockRepository);
        minionService.Minions = new Dictionary<int, Minion>();

        var result = minionService.CreateMinion(newMinion);

        Assert.That(result, Is.EqualTo(newMinion));
        Assert.That(minionService.Minions.ContainsKey(minionId), Is.True);
        mockRepository.Received(1).InsertMinion(newMinion);
    }


    [TestCase(1, "Updated Igor", 75, Description = "Update name and loyalty")]
    [TestCase(2, "Updated Helga", 60, Description = "Update different minion")]
    public void UpdateMinion_UpdatesCache_AndCallsRepository(int minionId, string newName, int newLoyalty)
    {
        var minion = new Minion 
        { 
            MinionId = minionId, 
            Name = "Original", 
            LoyaltyScore = 50, 
            Specialty = "Combat", 
            SkillLevel = 5, 
            SalaryDemand = 5000m,
            MoodStatus = "Happy",
            LastMoodUpdate = DateTime.Now
        };
        var minionService = new MinionService(mockRepository);
        minionService.Minions = new Dictionary<int, Minion> { { minionId, minion } };

        minion.Name = newName;
        minion.LoyaltyScore = newLoyalty;
        minionService.UpdateMinion(minion);

        Assert.That(minionService.Minions[minionId].Name, Is.EqualTo(newName));
        Assert.That(minionService.Minions[minionId].LoyaltyScore, Is.EqualTo(newLoyalty));
        mockRepository.Received(1).UpdateMinion(minion);
    }

    [TestCase(999, Description = "Update non-existent minion")]
    public void UpdateMinion_DoesNotUpdate_WhenMinionNotExists(int minionId)
    {
        var minion = new Minion { MinionId = minionId, Name = "Nonexistent", LoyaltyScore = 50 };
        var minionService = new MinionService(mockRepository);
        minionService.Minions = new Dictionary<int, Minion>();

        minionService.UpdateMinion(minion);

        mockRepository.DidNotReceive().UpdateMinion(Arg.Any<Minion>());
    }

    [TestCase(1, Description = "Delete first minion")]
    [TestCase(2, Description = "Delete second minion")]
    public void DeleteMinion_RemovesFromCache_AndCallsRepository(int minionId)
    {
        var minion = new Minion { MinionId = minionId, Name = "To Delete", LoyaltyScore = 50 };
        var minionService = new MinionService(mockRepository);
        minionService.Minions = new Dictionary<int, Minion> { { minionId, minion } };

        minionService.DeleteMinion(minionId);

        Assert.That(minionService.Minions.ContainsKey(minionId), Is.False);
        mockRepository.Received(1).DeleteMinion(minionId);
    }

    [TestCase(999, Description = "Delete non-existent minion")]
    public void DeleteMinion_DoesNotDelete_WhenMinionNotExists(int minionId)
    {
        var minionService = new MinionService(mockRepository);
        minionService.Minions = new Dictionary<int, Minion>();

        minionService.DeleteMinion(minionId);

        mockRepository.DidNotReceive().DeleteMinion(Arg.Any<int>());
    }
}

