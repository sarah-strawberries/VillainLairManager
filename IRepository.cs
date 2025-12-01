using VillainLairManager.Models;

namespace VillainLairManager
{
    public interface IRepository
    {
         abstract void CreateSchemaIfNotExists();
         abstract void DeleteBase(int baseId);
         abstract void DeleteEquipment(int equipmentId);
         abstract void DeleteMinion(int minionId);
         abstract void DeleteScheme(int schemeId);
         abstract List<SecretBase> GetAllBases();
         abstract List<Equipment> GetAllEquipment();
         abstract List<Minion> GetAllMinions();
         abstract List<EvilScheme> GetAllSchemes();
         abstract SecretBase GetBaseById(int baseId);
         abstract int GetBaseOccupancy(int baseId);
         abstract Equipment GetEquipmentById(int equipmentId);
         abstract Minion GetMinionById(int minionId);
         abstract int GetSchemeAssignedEquipmentCount(int schemeId);
         abstract int GetSchemeAssignedMinionsCount(int schemeId);
         abstract EvilScheme GetSchemeById(int schemeId);
         abstract void Initialize();
         abstract void InsertBase(SecretBase baseObj);
         abstract void InsertEquipment(Equipment equipment);
         abstract void InsertMinion(Minion minion);
         abstract void InsertScheme(EvilScheme scheme);
         abstract void SeedInitialData();
         abstract void UpdateBase(SecretBase baseObj);
         abstract void UpdateEquipment(Equipment equipment);
         abstract void UpdateMinion(Minion minion);
         abstract void UpdateScheme(EvilScheme scheme);
    }
}