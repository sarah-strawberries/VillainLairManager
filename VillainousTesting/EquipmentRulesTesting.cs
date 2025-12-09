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
    }
}
