using NUnit.Framework;
using NSubstitute;
using VillainLairManager.Models;
using VillainLairManager.Services;
using VillainLairManager.Utils;
using VillainLairManager;
using System;
using System.Collections.Generic;

namespace VillainousTesting
{
    [TestFixture]
    public class EquipmentRulesTesting
    {
        private IRepository _mockRepository;
        private EquipmentService _equipmentService;

        [SetUp]
        public void Setup()
        {
            _mockRepository = Substitute.For<IRepository>();
            _mockRepository.GetAllEquipment().Returns(new List<Equipment>());
            _equipmentService = new EquipmentService(_mockRepository);
            _equipmentService.Equipment = new Dictionary<int, Equipment>();
        }

        [TestCase(100, 1, true, "Active", 95, Description = "Standard degradation")]
        [TestCase(100, 5, true, "Active", 75, Description = "Extended use")]
        [TestCase(30, 8, true, "Active", 0, Description = "Severely degraded (clamped)")]
        [TestCase(80, 3, false, "Active", 80, Description = "No degradation when not assigned")]
        [TestCase(50, 2, true, "Completed", 50, Description = "No degradation for completed schemes")]
        public void Test_Rule1_ConditionDegradation(int initialCondition, int monthsSinceMaintenance, bool isAssigned, string schemeStatus, int expectedCondition)
        {
            // Arrange
            int equipmentId = 1;
            int schemeId = 101;

            var equipment = new Equipment
            {
                EquipmentId = equipmentId,
                Condition = initialCondition,
                LastMaintenanceDate = DateTime.Now.AddMonths(-monthsSinceMaintenance),
                AssignedToSchemeId = isAssigned ? schemeId : (int?)null
            };

            var scheme = new EvilScheme
            {
                SchemeId = schemeId,
                Status = schemeStatus
            };

            // Setup mocks
            _equipmentService.Equipment[equipmentId] = equipment;
            _mockRepository.GetSchemeById(schemeId).Returns(scheme);

            // Act
            _equipmentService.DegradeCondition(equipmentId);

            // Assert
            Assert.That(equipment.Condition, Is.EqualTo(expectedCondition), $"Failed for case: Initial={initialCondition}, Months={monthsSinceMaintenance}, Assigned={isAssigned}, Status={schemeStatus}");
            
            // Verify update was called if condition changed
            if (initialCondition != expectedCondition)
            {
                _mockRepository.Received().UpdateEquipment(equipment);
            }
        }

        [TestCase(50, 10000, 1500, 5000, true, 100, Description = "Success")]
        [TestCase(80, 20000, 3000, 2000, false, 80, Description = "Rejected - Insufficient funds")]
        [TestCase(100, 10000, 1500, 5000, false, 100, Description = "Rejected - Already perfect")]
        [TestCase(15, 50000, 7500, 10000, true, 100, Description = "Success - Expensive repair")]
        public void Test_Rule2_MaintenanceOperations(int initialCondition, decimal purchasePrice, decimal expectedCost, decimal availableFunds, bool expectSuccess, int expectedCondition)
        {
            // Arrange
            int equipmentId = 2;
            var equipment = new Equipment
            {
                EquipmentId = equipmentId,
                Condition = initialCondition,
                PurchasePrice = purchasePrice,
                LastMaintenanceDate = DateTime.Now.AddMonths(-5) // Old maintenance
            };
            _equipmentService.Equipment[equipmentId] = equipment;
            _mockRepository.GetEquipmentById(equipmentId).Returns(equipment);

            // Act & Assert
            if (expectSuccess)
            {
                decimal cost = _equipmentService.PerformMaintenance(equipmentId, availableFunds);
                
                Assert.That(cost, Is.EqualTo(expectedCost), "Maintenance cost mismatch");
                Assert.That(equipment.Condition, Is.EqualTo(expectedCondition), "Condition should be restored to 100");
                _mockRepository.Received().UpdateEquipment(equipment);
            }
            else
            {
                // Expect exception for rejection
                var ex = Assert.Throws<Exception>(() => _equipmentService.PerformMaintenance(equipmentId, availableFunds));
                
                // Verify state didn't change
                Assert.That(equipment.Condition, Is.EqualTo(expectedCondition), "Condition should not change on rejection");
                _mockRepository.DidNotReceive().UpdateEquipment(equipment);
            }
        }

        [TestCase(80, true, false, false, 0, true, Description = "Valid assignment")]
        [TestCase(40, true, false, false, 0, false, Description = "Rejected - Condition too low")]
        [TestCase(80, false, false, false, 0, false, Description = "Rejected - Not stored at base")]
        [TestCase(80, true, true, false, 0, false, Description = "Rejected - Already assigned to active scheme")]
        [TestCase(80, true, false, true, 5, false, Description = "Rejected - Specialist required but not present")]
        [TestCase(80, true, false, true, 9, true, Description = "Valid - Specialist present")]
        [TestCase(80, true, true, false, 0, true, Description = "Valid - Reassign from completed scheme")]
        public void Test_Rule3_AssignmentValidation(int condition, bool isStored, bool isAssigned, bool requiresSpecialist, int maxMinionSkill, bool expectValid)
        {
            // Arrange
            int equipmentId = 3;
            int schemeId = 103;
            int baseId = 200;

            var equipment = new Equipment
            {
                EquipmentId = equipmentId,
                Condition = condition,
                StoredAtBaseId = isStored ? baseId : (int?)null,
                AssignedToSchemeId = isAssigned ? 999 : (int?)null, // 999 is "other scheme"
                RequiresSpecialist = requiresSpecialist
            };
            _equipmentService.Equipment[equipmentId] = equipment;

            var targetScheme = new EvilScheme { SchemeId = schemeId, Status = "Active", PrimaryBaseId = baseId };
            _mockRepository.GetSchemeById(schemeId).Returns(targetScheme);

            if (isAssigned)
            {
                string otherSchemeStatus = (expectValid && isAssigned) ? "Completed" : "Active";
                _mockRepository.GetSchemeById(999).Returns(new EvilScheme { SchemeId = 999, Status = otherSchemeStatus });
            }

            var allMinions = new List<Minion>();
            if (requiresSpecialist && maxMinionSkill > 0)
            {
                allMinions.Add(new Minion { MinionId = 1, SkillLevel = maxMinionSkill, CurrentSchemeId = schemeId });
            }
            _mockRepository.GetAllMinions().Returns(allMinions);

            // Act
            var result = _equipmentService.ValidateAssignment(equipmentId, schemeId);

            // Assert
            Assert.That(result.IsValid, Is.EqualTo(expectValid), $"Validation result mismatch. Message: {result.Message}");
        }

        [TestCase("Weapon", true)]
        [TestCase("Doomsday Device", true)]
        [TestCase("weapon", false)]
        [TestCase("Tool", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void Test_Rule4_CategoryValidation(string? category, bool expectValid)
        {
            var equipment = new Equipment { Category = category!, Condition = 100, PurchasePrice = 100, MaintenanceCost = 10 };
            var result = _equipmentService.ValidateEquipment(equipment);
            Assert.That(result.IsValid, Is.EqualTo(expectValid), $"Category validation failed for '{category}'");
        }

        [TestCase(0, true)]
        [TestCase(100, true)]
        [TestCase(50, true)]
        [TestCase(-10, false)]
        [TestCase(150, false)]
        public void Test_Rule5_ConditionRangeValidation(int condition, bool expectValid)
        {
            var equipment = new Equipment { Category = "Weapon", Condition = condition, PurchasePrice = 100, MaintenanceCost = 10 };
            var result = _equipmentService.ValidateEquipment(equipment);
            Assert.That(result.IsValid, Is.EqualTo(expectValid), $"Condition validation failed for {condition}");
        }

        [TestCase(10000, 500, true)]
        [TestCase(0, 500, false)]
        [TestCase(10000, -100, false)]
        [TestCase(10000, 15000, true)] // Warning is still valid
        public void Test_Rule6_CostValidation(decimal price, decimal maintenance, bool expectValid)
        {
            var equipment = new Equipment { Category = "Weapon", Condition = 100, PurchasePrice = price, MaintenanceCost = maintenance };
            var result = _equipmentService.ValidateEquipment(equipment);
            Assert.That(result.IsValid, Is.EqualTo(expectValid), $"Cost validation failed for Price={price}, Maint={maintenance}");
        }

        [TestCase(true, 9, 10, true, Description = "Valid Doomsday Device")]
        [TestCase(false, 9, 10, true, Description = "Valid (Warning) - Base storage mismatch")]
        [TestCase(true, 5, 10, true, Description = "Valid (Warning) - Low rating")]
        [TestCase(true, 9, 7, false, Description = "Rejected - Skill too low")]
        public void Test_Rule7_DoomsdayDeviceRules(bool baseHasStorage, int schemeRating, int minionSkill, bool expectValid)
        {
            // Arrange
            int equipmentId = 7;
            int schemeId = 107;
            int baseId = 207;

            var equipment = new Equipment
            {
                EquipmentId = equipmentId,
                Category = "Doomsday Device",
                Condition = 100,
                StoredAtBaseId = baseId,
                RequiresSpecialist = true // Implicit for DD
            };
            _equipmentService.Equipment[equipmentId] = equipment;

            var scheme = new EvilScheme { SchemeId = schemeId, Status = "Active", DiabolicalRating = schemeRating, PrimaryBaseId = baseId };
            _mockRepository.GetSchemeById(schemeId).Returns(scheme);

            var baseObj = new SecretBase { BaseId = baseId, HasDoomsdayDevice = baseHasStorage };
            _mockRepository.GetBaseById(baseId).Returns(baseObj);

            var minions = new List<Minion> { new Minion { MinionId = 1, SkillLevel = minionSkill, CurrentSchemeId = schemeId } };
            _mockRepository.GetAllMinions().Returns(minions);

            // Act
            var result = _equipmentService.ValidateAssignment(equipmentId, schemeId);

            // Assert
            Assert.That(result.IsValid, Is.EqualTo(expectValid), $"Doomsday validation failed. Message: {result.Message}");
        }

        [Test]
        public void AddEquipment_ValidEquipment_InsertsIntoRepository()
        {
            var equipment = new Equipment { Category = "Weapon", Condition = 100, PurchasePrice = 100, MaintenanceCost = 10 };
            _mockRepository.GetAllEquipment().Returns(new List<Equipment>()); // For cache refresh

            _equipmentService.AddEquipment(equipment);

            _mockRepository.Received().InsertEquipment(equipment);
        }

        [Test]
        public void AddEquipment_InvalidEquipment_ThrowsException()
        {
            var equipment = new Equipment { Category = "Invalid", Condition = 100, PurchasePrice = 100, MaintenanceCost = 10 };

            var ex = Assert.Throws<Exception>(() => _equipmentService.AddEquipment(equipment));
            Assert.That(ex.Message, Does.Contain("Invalid category"));
            _mockRepository.DidNotReceive().InsertEquipment(equipment);
        }

        [Test]
        public void UpdateEquipment_ValidEquipment_UpdatesRepository()
        {
            var equipment = new Equipment { EquipmentId = 1, Category = "Weapon", Condition = 100, PurchasePrice = 100, MaintenanceCost = 10 };
            
            _equipmentService.UpdateEquipment(equipment);

            _mockRepository.Received().UpdateEquipment(equipment);
        }

        [Test]
        public void UpdateEquipment_InvalidEquipment_ThrowsException()
        {
            var equipment = new Equipment { EquipmentId = 1, Category = "Invalid", Condition = 100, PurchasePrice = 100, MaintenanceCost = 10 };

            var ex = Assert.Throws<Exception>(() => _equipmentService.UpdateEquipment(equipment));
            Assert.That(ex.Message, Does.Contain("Invalid category"));
            _mockRepository.DidNotReceive().UpdateEquipment(equipment);
        }

        [Test]
        public void DeleteEquipment_Unassigned_DeletesFromRepository()
        {
            int equipmentId = 1;
            var equipment = new Equipment { EquipmentId = equipmentId, AssignedToSchemeId = null };
            _mockRepository.GetEquipmentById(equipmentId).Returns(equipment);

            _equipmentService.DeleteEquipment(equipmentId);

            _mockRepository.Received().DeleteEquipment(equipmentId);
        }

        [Test]
        public void DeleteEquipment_Assigned_UpdatesSchemeAndDeletes()
        {
            int equipmentId = 1;
            int schemeId = 101;
            var equipment = new Equipment { EquipmentId = equipmentId, AssignedToSchemeId = schemeId };
            var scheme = new EvilScheme { SchemeId = schemeId, SuccessLikelihood = 50 };
            
            _mockRepository.GetEquipmentById(equipmentId).Returns(equipment);
            _mockRepository.GetSchemeById(schemeId).Returns(scheme);

            _equipmentService.DeleteEquipment(equipmentId);

            // Verify scheme update
            Assert.That(scheme.SuccessLikelihood, Is.EqualTo(45)); // 50 - 5
            _mockRepository.Received().UpdateScheme(scheme);
            
            // Verify deletion
            _mockRepository.Received().DeleteEquipment(equipmentId);
        }
    }
}
