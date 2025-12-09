using NUnit.Framework;
using VillainLairManager.Models;
using VillainLairManager;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace VillainousTesting
{
    /// <summary>
    /// Integration tests for DatabaseHelper CRUD operations using actual SQLiteConnection
    /// Tests all CRUD operations for:
    /// - Minions
    /// - EvilSchemes
    /// - SecretBases
    /// - Equipment
    /// 
    /// Uses in-memory SQLite database for fast, isolated testing without affecting production database
    /// </summary>
    [TestFixture]
    public class CrudOperationsTests
    {
        private SQLiteConnection testConnection;
        private DatabaseHelper databaseHelper;

        [SetUp]
        public void Setup()
        {
            // Create in-memory SQLite database for testing
            testConnection = new SQLiteConnection("Data Source=:memory:;Version=3;");
            testConnection.Open();
            
            // Inject connection into DatabaseHelper
            databaseHelper = new DatabaseHelper(testConnection);
            databaseHelper.CreateSchemaIfNotExists();
            databaseHelper.Initialize();
        }

        [TearDown]
        public void Cleanup()
        {
            testConnection?.Close();
            testConnection?.Dispose();
        }

        // ===================== MINION CRUD OPERATIONS =====================

        [TestCase("Igor", 5, "Combat", Description = "Insert and retrieve minion")]
        [TestCase("Helga", 8, "Hacking", Description = "Insert skilled minion")]
        public void Minion_InsertAndRetrieve_Succeeds(string name, int skillLevel, string specialty)
        {
            var minion = new Minion
            {
                Name = name,
                SkillLevel = skillLevel,
                Specialty = specialty,
                LoyaltyScore = 50,
                SalaryDemand = 5000m,
                MoodStatus = "Happy",
                LastMoodUpdate = DateTime.Now
            };

            databaseHelper.InsertMinion(minion);
            var allMinions = databaseHelper.GetAllMinions();
            var retrieved = allMinions.FirstOrDefault(m => m.Name == name);

            Assert.That(retrieved, Is.Not.Null);
            Assert.That(retrieved.Name, Is.EqualTo(name));
            Assert.That(retrieved.SkillLevel, Is.EqualTo(skillLevel));
            Assert.That(retrieved.Specialty, Is.EqualTo(specialty));
        }

        [TestCase("Igor", 50, 75, Description = "Update minion loyalty")]
        public void Minion_UpdateMinion_Succeeds(string originalName, int originalLoyalty, int newLoyalty)
        {
            var minion = new Minion
            {
                Name = originalName,
                SkillLevel = 5,
                Specialty = "Combat",
                LoyaltyScore = originalLoyalty,
                SalaryDemand = 5000m,
                MoodStatus = "Happy",
                LastMoodUpdate = DateTime.Now
            };

            databaseHelper.InsertMinion(minion);
            var inserted = databaseHelper.GetAllMinions().FirstOrDefault(m => m.Name == originalName);
            
            Assert.That(inserted, Is.Not.Null);
            inserted!.LoyaltyScore = newLoyalty;
            databaseHelper.UpdateMinion(inserted);
            
            var retrieved = databaseHelper.GetMinionById(inserted.MinionId);

            Assert.That(retrieved.LoyaltyScore, Is.EqualTo(newLoyalty));
        }

        [TestCase("Igor", Description = "Delete minion")]
        public void Minion_DeleteMinion_Succeeds(string minionName)
        {
            var minion = new Minion
            {
                Name = minionName,
                SkillLevel = 5,
                Specialty = "Combat",
                LoyaltyScore = 50,
                SalaryDemand = 5000m,
                MoodStatus = "Happy",
                LastMoodUpdate = DateTime.Now
            };

            databaseHelper.InsertMinion(minion);
            var inserted = databaseHelper.GetAllMinions().FirstOrDefault(m => m.Name == minionName);
            
            Assert.That(inserted, Is.Not.Null);
            databaseHelper.DeleteMinion(inserted!.MinionId);
            
            var retrieved = databaseHelper.GetMinionById(inserted.MinionId);

            Assert.That(retrieved, Is.Null);
        }

        // ===================== EVIL SCHEME CRUD OPERATIONS =====================

        [TestCase("Steal the Moon", 8, Description = "Insert and retrieve scheme")]
        [TestCase("Freeze City", 6, Description = "Insert medium scheme")]
        public void Scheme_InsertAndRetrieve_Succeeds(string name, int requiredSkillLevel)
        {
            var scheme = new EvilScheme
            {
                Name = name,
                Description = "Test scheme",
                Budget = 500000m,
                CurrentSpending = 0m,
                RequiredSkillLevel = requiredSkillLevel,
                RequiredSpecialty = "Engineering",
                Status = "Planning",
                TargetCompletionDate = DateTime.Now.AddMonths(6),
                DiabolicalRating = 7,
                SuccessLikelihood = 50
            };

            databaseHelper.InsertScheme(scheme);
            var allSchemes = databaseHelper.GetAllSchemes();
            var retrieved = allSchemes.FirstOrDefault(s => s.Name == name);

            Assert.That(retrieved, Is.Not.Null);
            Assert.That(retrieved.Name, Is.EqualTo(name));
            Assert.That(retrieved.RequiredSkillLevel, Is.EqualTo(requiredSkillLevel));
        }

        [TestCase("Moon Heist", "Updated Scheme", 75, Description = "Update scheme success likelihood")]
        public void Scheme_UpdateScheme_Succeeds(string originalName, string newName, int newSuccess)
        {
            var scheme = new EvilScheme
            {
                Name = originalName,
                Description = "Test",
                Budget = 500000m,
                CurrentSpending = 0m,
                RequiredSkillLevel = 6,
                RequiredSpecialty = "Engineering",
                Status = "Planning",
                TargetCompletionDate = DateTime.Now.AddMonths(6),
                DiabolicalRating = 7,
                SuccessLikelihood = 50
            };

            databaseHelper.InsertScheme(scheme);
            var inserted = databaseHelper.GetAllSchemes().FirstOrDefault(s => s.Name == originalName);
            
            Assert.That(inserted, Is.Not.Null);
            inserted!.Name = newName;
            inserted.SuccessLikelihood = newSuccess;
            databaseHelper.UpdateScheme(inserted);
            
            var retrieved = databaseHelper.GetSchemeById(inserted.SchemeId);

            Assert.That(retrieved.Name, Is.EqualTo(newName));
            Assert.That(retrieved.SuccessLikelihood, Is.EqualTo(newSuccess));
        }

        [TestCase("World Domination", Description = "Delete scheme")]
        public void Scheme_DeleteScheme_Succeeds(string schemeName)
        {
            var scheme = new EvilScheme
            {
                Name = schemeName,
                Description = "Test",
                Budget = 500000m,
                CurrentSpending = 0m,
                RequiredSkillLevel = 6,
                RequiredSpecialty = "Engineering",
                Status = "Planning",
                TargetCompletionDate = DateTime.Now.AddMonths(6),
                DiabolicalRating = 7,
                SuccessLikelihood = 50
            };

            databaseHelper.InsertScheme(scheme);
            var inserted = databaseHelper.GetAllSchemes().FirstOrDefault(s => s.Name == schemeName);
            
            Assert.That(inserted, Is.Not.Null);
            databaseHelper.DeleteScheme(inserted!.SchemeId);
            
            var retrieved = databaseHelper.GetSchemeById(inserted.SchemeId);

            Assert.That(retrieved, Is.Null);
        }

        // ===================== SECRET BASE CRUD OPERATIONS =====================

        [TestCase("Volcano Fortress", 50, 9, Description = "Insert and retrieve base")]
        [TestCase("Arctic Hideout", 30, 7, Description = "Insert medium base")]
        public void Base_InsertAndRetrieve_Succeeds(string name, int capacity, int securityLevel)
        {
            var base_ = new SecretBase
            {
                Name = name,
                Location = "Test Location",
                Capacity = capacity,
                SecurityLevel = securityLevel,
                MonthlyMaintenanceCost = 50000m,
                HasDoomsdayDevice = true,
                IsDiscovered = false
            };

            databaseHelper.InsertBase(base_);
            var allBases = databaseHelper.GetAllBases();
            var retrieved = allBases.FirstOrDefault(b => b.Name == name);

            Assert.That(retrieved, Is.Not.Null);
            Assert.That(retrieved.Name, Is.EqualTo(name));
            Assert.That(retrieved.Capacity, Is.EqualTo(capacity));
        }

        [TestCase("Original Base", "Updated Base", 60, Description = "Update base capacity")]
        public void Base_UpdateBase_Succeeds(string originalName, string newName, int newCapacity)
        {
            var base_ = new SecretBase
            {
                Name = originalName,
                Location = "Test Location",
                Capacity = 50,
                SecurityLevel = 9,
                MonthlyMaintenanceCost = 50000m,
                HasDoomsdayDevice = true,
                IsDiscovered = false
            };

            databaseHelper.InsertBase(base_);
            var inserted = databaseHelper.GetAllBases().FirstOrDefault(b => b.Name == originalName);
            
            Assert.That(inserted, Is.Not.Null);
            inserted!.Name = newName;
            inserted.Capacity = newCapacity;
            databaseHelper.UpdateBase(inserted);
            
            var retrieved = databaseHelper.GetBaseById(inserted.BaseId);

            Assert.That(retrieved.Name, Is.EqualTo(newName));
            Assert.That(retrieved.Capacity, Is.EqualTo(newCapacity));
        }

        [TestCase("Base to Delete", Description = "Delete base")]
        public void Base_DeleteBase_Succeeds(string baseName)
        {
            var base_ = new SecretBase
            {
                Name = baseName,
                Location = "Test Location",
                Capacity = 50,
                SecurityLevel = 9,
                MonthlyMaintenanceCost = 50000m,
                HasDoomsdayDevice = true,
                IsDiscovered = false
            };

            databaseHelper.InsertBase(base_);
            var inserted = databaseHelper.GetAllBases().FirstOrDefault(b => b.Name == baseName);
            
            Assert.That(inserted, Is.Not.Null);
            databaseHelper.DeleteBase(inserted!.BaseId);
            
            var retrieved = databaseHelper.GetBaseById(inserted.BaseId);

            Assert.That(retrieved, Is.Null);
        }

        // ===================== EQUIPMENT CRUD OPERATIONS =====================

        [TestCase("Freeze Ray", "Weapon", 85, Description = "Insert and retrieve equipment")]
        [TestCase("Drill Tank", "Vehicle", 72, Description = "Insert vehicle equipment")]
        public void Equipment_InsertAndRetrieve_Succeeds(string name, string category, int condition)
        {
            var equipment = new Equipment
            {
                Name = name,
                Category = category,
                Condition = condition,
                PurchasePrice = 100000m,
                MaintenanceCost = 5000m,
                RequiresSpecialist = false
            };

            databaseHelper.InsertEquipment(equipment);
            var allEquipment = databaseHelper.GetAllEquipment();
            var retrieved = allEquipment.FirstOrDefault(e => e.Name == name);

            Assert.That(retrieved, Is.Not.Null);
            Assert.That(retrieved.Name, Is.EqualTo(name));
            Assert.That(retrieved.Category, Is.EqualTo(category));
            Assert.That(retrieved.Condition, Is.EqualTo(condition));
        }

        [TestCase("Freeze Ray", "Updated Freeze Ray", 80, Description = "Update equipment condition")]
        public void Equipment_UpdateEquipment_Succeeds(string originalName, string newName, int newCondition)
        {
            var equipment = new Equipment
            {
                Name = originalName,
                Category = "Weapon",
                Condition = 85,
                PurchasePrice = 100000m,
                MaintenanceCost = 5000m,
                RequiresSpecialist = false
            };

            databaseHelper.InsertEquipment(equipment);
            var inserted = databaseHelper.GetAllEquipment().FirstOrDefault(e => e.Name == originalName);
            
            Assert.That(inserted, Is.Not.Null);
            inserted!.Name = newName;
            inserted.Condition = newCondition;
            databaseHelper.UpdateEquipment(inserted);
            
            var retrieved = databaseHelper.GetEquipmentById(inserted.EquipmentId);

            Assert.That(retrieved.Name, Is.EqualTo(newName));
            Assert.That(retrieved.Condition, Is.EqualTo(newCondition));
        }

        [TestCase("Equipment to Delete", Description = "Delete equipment")]
        public void Equipment_DeleteEquipment_Succeeds(string equipmentName)
        {
            var equipment = new Equipment
            {
                Name = equipmentName,
                Category = "Weapon",
                Condition = 85,
                PurchasePrice = 100000m,
                MaintenanceCost = 5000m,
                RequiresSpecialist = false
            };

            databaseHelper.InsertEquipment(equipment);
            var inserted = databaseHelper.GetAllEquipment().FirstOrDefault(e => e.Name == equipmentName);
            
            Assert.That(inserted, Is.Not.Null);
            databaseHelper.DeleteEquipment(inserted!.EquipmentId);
            
            var retrieved = databaseHelper.GetEquipmentById(inserted.EquipmentId);

            Assert.That(retrieved, Is.Null);
        }
    }
}
