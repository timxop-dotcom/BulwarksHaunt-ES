using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using RoR2;
using System.Runtime.CompilerServices;

namespace BulwarksHaunt
{
    internal static class ProperSaveSupport
    {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void Init()
        {
            ProperSave.SaveFile.OnGatgherSaveData += SaveFile_OnGatgherSaveData;
            ProperSave.Loading.OnLoadingEnded += Loading_OnLoadingEnded;
        }

        public class BulwarksHauntSaveData
        {
            [DataMember(Name = "keps")]
            public List<IntKilledEnemyListPair> killedEnemiesPerStage { get; set; }

            public class IntKilledEnemyListPair
            {
                [DataMember(Name = "i")]
                public int Int { get; set; }

                [DataMember(Name = "l")]
                public List<BulwarksHauntKilledEnemySaveData> KilledEnemyList { get; set; }
            }

            public class BulwarksHauntKilledEnemySaveData
            {
                [DataMember(Name = "bi")]
                public int BodyIndex { get; set; }

                [DataMember(Name = "iii")]
                public List<int> InventoryItemIndices { get; set; }

                [DataMember(Name = "iic")]
                public List<int> InventoryItemCounts { get; set; }

                [DataMember(Name = "ei")]
                public int EquipmentIndex { get; set; }
            }

            [DataMember(Name = "wr")]
            public List<IntSeedListPair> waveRng { get; set; }

            public class IntSeedListPair
            {
                [DataMember(Name = "i")]
                public int Int { get; set; }

                [DataMember(Name = "s0")]
                public ulong Seed0 { get; set; }

                [DataMember(Name = "s1")]
                public ulong Seed1 { get; set; }
            }

            internal BulwarksHauntSaveData()
            {
                killedEnemiesPerStage = new List<IntKilledEnemyListPair>();
                foreach (var stage in GhostWave.killedEnemiesPerStage.Keys)
                {
                    var killedEnemiesData = new List<BulwarksHauntKilledEnemySaveData>();
                    foreach (var killedEnemy in GhostWave.killedEnemiesPerStage[stage])
                    {
                        var killedEnemyData = new BulwarksHauntKilledEnemySaveData();
                        killedEnemyData.BodyIndex = (int)killedEnemy.bodyIndex;
                        killedEnemyData.InventoryItemIndices = killedEnemy.inventory.Select(x => (int)(x.itemDef != null ? x.itemDef.itemIndex : ItemIndex.None)).ToList();
                        killedEnemyData.InventoryItemCounts = killedEnemy.inventory.Select(x => x.count).ToList();
                        killedEnemyData.EquipmentIndex = (int)(killedEnemy.equipment != null ? killedEnemy.equipment.equipmentIndex : EquipmentIndex.None);
                        killedEnemiesData.Add(killedEnemyData);
                    }
                    var pair = new IntKilledEnemyListPair();
                    pair.Int = stage;
                    pair.KilledEnemyList = killedEnemiesData;
                    killedEnemiesPerStage.Add(pair);
                }

                waveRng = new List<IntSeedListPair>();
                foreach (var stage in GhostWave.waveRng.Keys)
                {
                    var pair = new IntSeedListPair();
                    pair.Int = stage;
                    pair.Seed0 = GhostWave.waveRng[stage].state0;
                    pair.Seed1 = GhostWave.waveRng[stage].state1;
                    waveRng.Add(pair);
                }
            }

            internal void Load()
            {
                GhostWave.killedEnemiesPerStage.Clear();
                foreach (var pair in killedEnemiesPerStage)
                {
                    var killedEnemies = new List<GhostWave.KilledEnemyData>();
                    foreach (var killedEnemyData in pair.KilledEnemyList)
                    {
                        var killedEnemy = new GhostWave.KilledEnemyData();
                        killedEnemy.bodyIndex = (BodyIndex)killedEnemyData.BodyIndex;
                        killedEnemy.inventory = new List<ItemCountPair>();
                        for (var i = 0; i < killedEnemyData.InventoryItemIndices.Count; i++)
                        {
                            var itemIndex = (ItemIndex)killedEnemyData.InventoryItemIndices[i];
                            var itemCount = i < killedEnemyData.InventoryItemCounts.Count ? killedEnemyData.InventoryItemCounts[i] : 1;
                            killedEnemy.inventory.Add(new ItemCountPair
                            {
                                itemDef = ItemCatalog.GetItemDef(itemIndex),
                                count = itemCount
                            });
                        }
                        killedEnemy.equipment = EquipmentCatalog.GetEquipmentDef((EquipmentIndex)killedEnemyData.EquipmentIndex);
                        killedEnemies.Add(killedEnemy);
                    }
                    GhostWave.killedEnemiesPerStage[pair.Int] = killedEnemies;
                }

                GhostWave.waveRng.Clear();
                foreach (var pair in waveRng)
                {
                    var rng = new Xoroshiro128Plus(0);
                    rng.state0 = pair.Seed0;
                    rng.state1 = pair.Seed1;
                    GhostWave.waveRng[pair.Int] = rng;
                }
            }
        }

        private static void SaveFile_OnGatgherSaveData(Dictionary<string, object> obj)
        {
            obj.Add("BulwarksHaunt_SaveData", new BulwarksHauntSaveData());
        }

        private static void Loading_OnLoadingEnded(ProperSave.SaveFile obj)
        {
            var data = obj.GetModdedData<BulwarksHauntSaveData>("BulwarksHaunt_SaveData");
            if (data != null) data.Load();
        }
    }
}