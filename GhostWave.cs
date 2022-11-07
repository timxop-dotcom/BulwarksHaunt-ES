using Mono.Cecil.Cil;
using MonoMod.Cil;
using MysticsRisky2Utils;
using MysticsRisky2Utils.ContentManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Navigation;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

namespace BulwarksHaunt
{
    public class GhostWave : BaseLoadableAsset
    {
        public static SceneDef sceneDef;
        public static GameObject sceneInfoObj;
        public static GameObject directorObj;
        public static GameObject gameManagerObj;
        public static GameObject ghostWaveControllerObj;

        public static MapNodeGroup groundNodeGroup;
        public static NodeGraph groundNodeGraph;
        
        public static MapNodeGroup airNodeGroup;
        public static NodeGraph airNodeGraph;

        public static DirectorCardCategorySelection dccsBulwarksHaunt_GhostWaveMonsters;
        public static DccsPool dpBulwarksHaunt_GhostWaveMonsters;

        public static DirectorCardCategorySelection dccsBulwarksHaunt_GhostWaveInteractables;
        public static DccsPool dpBulwarksHaunt_GhostWaveInteractables;

        public static GameObject ghostWaveControllerObjPrefab;

        private static System.DateTime debugTimestamp;

        public override void OnPluginAwake()
        {
            base.OnPluginAwake();
            ghostWaveControllerObjPrefab = Utils.CreateBlankPrefab("GhostWaveController2", true);
            NetworkingAPI.RegisterMessageType<GhostWaveControllerBaseState.MonsterWaves.SyncTotalEnemies>();
            NetworkingAPI.RegisterMessageType<GhostWaveControllerBaseState.MonsterWaves.SyncKilledEnemies>();
        }

        public static bool bakeNodesAtRuntime = false;
        public struct BakedNodes
        {
            public NodeSerialized[] nodes;
            public NodeGraph.Link[] links;
            public List<string> gateNames;
        }
        public struct NodeSerialized
        {
            public float x;
            public float y;
            public float z;
            public NodeGraph.LinkListIndex linkListIndex;
            public HullMask forbiddenHulls;
            public byte[] lineOfSightMaskBytes;
            public int lineOfSightMaskLength;
            public byte gateIndex;
            public NodeFlags flags;
        }

        public override void Load()
        {
            if (bakeNodesAtRuntime)
            {
                On.RoR2.Navigation.MapNodeGroup.Bake += MapNodeGroup_Bake1;
                IL.RoR2.Navigation.MapNodeGroup.Bake += MapNodeGroup_Bake;
            }

            SetUpStage();
            SetUpGameEnding();
            SetUpGamemode();
            OnLoad();

            asset = sceneDef;
            BulwarksHauntContent.Resources.sceneDefs.Add(sceneDef);

            On.RoR2.MusicTrackCatalog.Init += SetUpStageMusic;

            // Chance to appear in Bazaar seers
            On.RoR2.BazaarController.SetUpSeerStations += BazaarController_SetUpSeerStations;
        }

        private void MapNodeGroup_Bake1(On.RoR2.Navigation.MapNodeGroup.orig_Bake orig, MapNodeGroup self, NodeGraph nodeGraph)
        {
            debugTimestamp = System.DateTime.Now;
            orig(self, nodeGraph);
            BulwarksHauntPlugin.logger.LogMessage("Nodes baked! " + System.DateTime.Now.Subtract(debugTimestamp).TotalSeconds + "s elapsed");
        }

        private void MapNodeGroup_Bake(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(
                MoveType.After,
                x => x.MatchCallOrCallvirt<MapNode>("BuildLinks")
            ))
            {
                c.Emit(OpCodes.Ldloc, 0);
                c.Emit(OpCodes.Ldloc, 3);
                c.EmitDelegate<System.Action<List<MapNode>, int>>((nodes, i) => {
                    if ((i % 250) == 0)
                        BulwarksHauntPlugin.logger.LogMessage("Bake: " + i + "/" + nodes.Count + " nodes done, " + System.DateTime.Now.Subtract(debugTimestamp).TotalSeconds + "s elapsed");
                });
            }
        }

        public void SetUpStage()
        {
            var sceneAssetBundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(BulwarksHauntPlugin.pluginInfo.Location), "bulwarkshauntsceneassetbundle"));

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;

            // create SceneDef
            sceneDef = ScriptableObject.CreateInstance<SceneDef>();
            sceneDef.nameToken = "MAP_BULWARKSHAUNT_GHOSTWAVE_NAME";
            sceneDef.subtitleToken = "MAP_BULWARKSHAUNT_GHOSTWAVE_SUBTITLE";
            sceneDef.loreToken = "MAP_BULWARKSHAUNT_GHOSTWAVE_LORE";
            sceneDef.baseSceneNameOverride = "BulwarksHaunt_GhostWave";
            sceneDef.cachedName = "BulwarksHaunt_GhostWave";
            sceneDef.blockOrbitalSkills = false;
            sceneDef.dioramaPrefab = BulwarksHauntPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/Bulwark's Haunt/GhostWave/DioramaDisplay.prefab");
            var mpp = sceneDef.dioramaPrefab.AddComponent<ModelPanelParameters>();
            mpp.cameraPositionTransform = mpp.transform.Find("Camera Position");
            mpp.focusPointTransform = mpp.transform.Find("Focus Point");
            mpp.minDistance = 10f;
            mpp.maxDistance = 120f;
            sceneDef.isOfflineScene = false;
            sceneDef.previewTexture = BulwarksHauntPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/Bulwark's Haunt/GhostWave/texBulwarksHaunt_GhostWavePreview.png").texture;
            sceneDef.portalMaterial = Material.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/bazaar/matBazaarSeerFrozenwall.mat").WaitForCompletion());
            sceneDef.portalMaterial.SetTexture("_MainTex", sceneDef.previewTexture);
            sceneDef.portalSelectionMessageString = "BAZAAR_SEER_BULWARKSHAUNT_GHOSTWAVE";
            sceneDef.sceneType = SceneType.Intermission;
            sceneDef.shouldIncludeInLogbook = true;
            sceneDef.stageOrder = 101;
            sceneDef.suppressNpcEntry = false;
            sceneDef.suppressPlayerEntry = false;
            sceneDef.validForRandomSelection = false;

            if (bakeNodesAtRuntime)
            {
                // Create Ground NodeGraph
                var go = Utils.CreateBlankPrefab("BulwarksHaunt_GhostWaveGroundNodeGroup");
                var go2 = Object.Instantiate(BulwarksHauntPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/Bulwark's Haunt/GhostWave/World.prefab")); // we need this for the NodeGraph baker to detect collision with the world
                groundNodeGroup = go.AddComponent<MapNodeGroup>();
                groundNodeGroup.graphType = MapNodeGroup.GraphType.Ground;

                var groundNavMesh = BulwarksHauntPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/Bulwark's Haunt/GhostWave/NavMeshes/navmesh 4.prefab").GetComponentInChildren<MeshFilter>().mesh;
                foreach (var vertex in groundNavMesh.vertices)
                    groundNodeGroup.AddNode(new Vector3(vertex.x, vertex.y, vertex.z));
                groundNodeGraph = ScriptableObject.CreateInstance<NodeGraph>();
                groundNodeGraph.Clear();
                groundNodeGraph.name = "BulwarksHaunt_GhostWaveGroundNodeGraph";
                groundNodeGroup.UpdateNoCeilingMasks();
                groundNodeGroup.UpdateTeleporterMasks();
                groundNodeGroup.Bake(groundNodeGraph);

                // Create Air NodeGraph
                // Hacky: it's just the Ground NodeGraph, but slightly elevated
                go = Utils.CreateBlankPrefab("BulwarksHaunt_GhostWaveAirNodeGroup");
                airNodeGroup = go.AddComponent<MapNodeGroup>();
                airNodeGroup.graphType = MapNodeGroup.GraphType.Air;

                foreach (var vertex in groundNavMesh.vertices)
                {
                    airNodeGroup.AddNode(new Vector3(vertex.x, vertex.y, vertex.z) + 12f * Vector3.up);
                }
                airNodeGraph = ScriptableObject.CreateInstance<NodeGraph>();
                airNodeGraph.Clear();
                airNodeGraph.name = "BulwarksHaunt_GhostWaveAirNodeGraph";
                airNodeGroup.UpdateNoCeilingMasks();
                airNodeGroup.Bake(airNodeGraph);

                Object.Destroy(go2);

                var path = System.IO.Path.Combine(Application.persistentDataPath, "BulwarksHaunt", "BakedNodes");
                if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);

                void SaveBakedNodesToFile(MapNodeGroup mapNodeGroup, NodeGraph nodeGraph)
                {
                    var groupTypeStr = "Ground";
                    if (mapNodeGroup.graphType == MapNodeGroup.GraphType.Air) groupTypeStr = "Air";

                    var bakedNodesObj = new BakedNodes
                    {
                        nodes = nodeGraph.nodes.Select((x) =>
                        {
                            return new NodeSerialized
                            {
                                x = x.position.x,
                                y = x.position.y,
                                z = x.position.z,
                                flags = x.flags,
                                forbiddenHulls = x.forbiddenHulls,
                                gateIndex = x.gateIndex,
                                lineOfSightMaskBytes = x.lineOfSightMask.bytes,
                                lineOfSightMaskLength = x.lineOfSightMask.length,
                                linkListIndex = x.linkListIndex
                            };
                        }).ToArray(),
                        links = nodeGraph.links.ToArray(),
                        gateNames = nodeGraph.gateNames.ToList()
                    };

                    var encodedNodeGraph = JsonConvert.SerializeObject(bakedNodesObj, new JsonSerializerSettings
                    {
                        Culture = System.Globalization.CultureInfo.InvariantCulture
                    });
                    System.IO.File.WriteAllBytes(System.IO.Path.Combine(path, "BakedNodeGraph_" + sceneDef.baseSceneName + "_" + groupTypeStr + ""), new UTF8Encoding().GetBytes(encodedNodeGraph));
                }
                SaveBakedNodesToFile(groundNodeGroup, groundNodeGraph);
                SaveBakedNodesToFile(airNodeGroup, airNodeGraph);
            }
            else
            {
                var loadingTimestamp = System.DateTime.Now;
                BulwarksHauntPlugin.logger.LogMessage("Loading prebaked nodes...");

                NodeGraph LoadBakedNodesFromFile(MapNodeGroup.GraphType graphType)
                {
                    var groupTypeStr = "Ground";
                    if (graphType == MapNodeGroup.GraphType.Air) groupTypeStr = "Air";

                    var path = System.IO.Path.GetDirectoryName(BulwarksHauntPlugin.pluginInfo.Location);
                    var encodedNodeGraph = new UTF8Encoding().GetString(System.IO.File.ReadAllBytes(System.IO.Path.Combine(path, "BakedNodeGraph_" + sceneDef.baseSceneName + "_" + groupTypeStr)));
                    var decodedNodeGraph = JsonConvert.DeserializeObject<BakedNodes>(encodedNodeGraph, new JsonSerializerSettings
                    {
                        Culture = System.Globalization.CultureInfo.InvariantCulture
                    });
                    var properNodeGraph = ScriptableObject.CreateInstance<NodeGraph>();
                    properNodeGraph.Clear();
                    properNodeGraph.nodes = decodedNodeGraph.nodes.Select((x) =>
                    {
                        var lineOfSightMask = new SerializableBitArray(x.lineOfSightMaskLength);
                        lineOfSightMask.bytes = x.lineOfSightMaskBytes;
                        return new NodeGraph.Node
                        {
                            flags = x.flags,
                            forbiddenHulls = x.forbiddenHulls,
                            gateIndex = x.gateIndex,
                            lineOfSightMask = lineOfSightMask,
                            linkListIndex = x.linkListIndex,
                            position = new Vector3(x.x, x.y, x.z)
                        };
                    }).ToArray();
                    properNodeGraph.links = decodedNodeGraph.links.ToArray();
                    properNodeGraph.gateNames = decodedNodeGraph.gateNames.ToList();
                    properNodeGraph.OnNodeCountChanged();
                    properNodeGraph.RebuildBlockMap();

                    return properNodeGraph;
                }

                groundNodeGraph = LoadBakedNodesFromFile(MapNodeGroup.GraphType.Ground);
                groundNodeGraph.name = "BulwarksHaunt_GhostWaveGroundNodeGraph";

                var go = Utils.CreateBlankPrefab("BulwarksHaunt_GhostWaveGroundNodeGroup");
                groundNodeGroup = go.AddComponent<MapNodeGroup>();
                groundNodeGroup.graphType = MapNodeGroup.GraphType.Ground;
                groundNodeGroup.nodeGraph = groundNodeGraph;
                foreach (var node in groundNodeGraph.nodes)
                    groundNodeGroup.AddNode(node.position);

                airNodeGraph = LoadBakedNodesFromFile(MapNodeGroup.GraphType.Air);
                airNodeGraph.name = "BulwarksHaunt_GhostWaveAirNodeGraph";
                
                go = Utils.CreateBlankPrefab("BulwarksHaunt_GhostWaveAirNodeGroup");
                airNodeGroup = go.AddComponent<MapNodeGroup>();
                airNodeGroup.graphType = MapNodeGroup.GraphType.Air;
                airNodeGroup.nodeGraph = airNodeGraph;
                foreach (var node in airNodeGraph.nodes)
                    airNodeGroup.AddNode(node.position);

                BulwarksHauntPlugin.logger.LogMessage("Loaded baked nodes! " + System.DateTime.Now.Subtract(loadingTimestamp).TotalSeconds + "s elapsed");
            }

            // Create enemy pools
            dccsBulwarksHaunt_GhostWaveMonsters = ScriptableObject.CreateInstance<DirectorCardCategorySelection>();
            dccsBulwarksHaunt_GhostWaveMonsters.AddCategory("Normal", 1f);
            dccsBulwarksHaunt_GhostWaveMonsters.AddCard(0, new DirectorCard()
            {
                spawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/Wisp/cscLesserWisp.asset").WaitForCompletion(),
                selectionWeight = 100,
                spawnDistance = DirectorCore.MonsterSpawnDistance.Standard
            });
            dccsBulwarksHaunt_GhostWaveMonsters.AddCard(0, new DirectorCard()
            {
                spawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/GreaterWisp/cscGreaterWisp.asset").WaitForCompletion(),
                selectionWeight = 40,
                spawnDistance = DirectorCore.MonsterSpawnDistance.Standard
            });

            dpBulwarksHaunt_GhostWaveMonsters = ScriptableObject.CreateInstance<DccsPool>();
            dpBulwarksHaunt_GhostWaveMonsters.poolCategories = new DccsPool.Category[]
            {
                new DccsPool.Category()
                {
                    name = "Base",
                    alwaysIncluded = new DccsPool.PoolEntry[]
                    {
                        new DccsPool.PoolEntry() { dccs = dccsBulwarksHaunt_GhostWaveMonsters, weight = 1f }
                    },
                    categoryWeight = 1f,
                    includedIfConditionsMet = new DccsPool.ConditionalPoolEntry[] { },
                    includedIfNoConditionsMet = new DccsPool.PoolEntry[] { }
                }
            };

            // Create interactable pools
            dccsBulwarksHaunt_GhostWaveInteractables = ScriptableObject.CreateInstance<DirectorCardCategorySelection>();

            dpBulwarksHaunt_GhostWaveInteractables = ScriptableObject.CreateInstance<DccsPool>();
            dpBulwarksHaunt_GhostWaveInteractables.poolCategories = new DccsPool.Category[]
            {
                new DccsPool.Category()
                {
                    name = "Base",
                    alwaysIncluded = new DccsPool.PoolEntry[] { },
                    categoryWeight = 1f,
                    includedIfConditionsMet = new DccsPool.ConditionalPoolEntry[] { },
                    includedIfNoConditionsMet = new DccsPool.PoolEntry[] { }
                }
            };

            var stageLogUnlockable = ScriptableObject.CreateInstance<UnlockableDef>();
            stageLogUnlockable.cachedName = "Logs.Stages.BulwarksHaunt_GhostWave";
            stageLogUnlockable.nameToken = "UNLOCKABLE_LOG_STAGES_BULWARKSHAUNT_GHOSTWAVE";
            BulwarksHauntContent.Resources.unlockableDefs.Add(stageLogUnlockable);
        }

        private void SetUpStageMusic(On.RoR2.MusicTrackCatalog.orig_Init orig)
        {
            orig();
            sceneDef.mainTrack = MusicTrackCatalog.FindMusicTrackDef("muSong13");
            sceneDef.bossTrack = MusicTrackCatalog.FindMusicTrackDef("muSong13");
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "BulwarksHaunt_GhostWave")
            {
                var rootGameObjs = new List<GameObject>();
                scene.GetRootGameObjects(rootGameObjs);

                // Manage the SceneInfo object
                sceneInfoObj = rootGameObjs.First(x => x.name == "SceneInfo");
                sceneInfoObj.SetActive(false);

                // Create and fill out the SceneInfo
                var sceneInfo = sceneInfoObj.AddComponent<SceneInfo>();
                sceneInfo.groundNodeGroup = groundNodeGroup;
                sceneInfo.groundNodesAsset = groundNodeGraph;
                sceneInfo.airNodeGroup = airNodeGroup;
                sceneInfo.airNodesAsset = airNodeGraph;

                // Create and fill out the ClassicStageInfo
                var classicStageInfo = sceneInfoObj.AddComponent<ClassicStageInfo>();

                classicStageInfo.monsterDccsPool = dpBulwarksHaunt_GhostWaveMonsters;
                classicStageInfo.interactableDccsPool = null;
                classicStageInfo.interactableCategories = dccsBulwarksHaunt_GhostWaveInteractables;
                classicStageInfo.sceneDirectorInteractibleCredits = 0;
                classicStageInfo.sceneDirectorMonsterCredits = 30;
                classicStageInfo.bonusInteractibleCreditObjects = new ClassicStageInfo.BonusInteractibleCreditObject[] { };
                classicStageInfo.possibleMonsterFamilies = new ClassicStageInfo.MonsterFamily[] { };

                sceneInfoObj.SetActive(true);

                // Manage the Director object
                directorObj = rootGameObjs.First(x => x.name == "Director");
                directorObj.AddComponent<DirectorCore>();

                // Add SceneDirector
                var sceneDirector = directorObj.AddComponent<SceneDirector>();
                // sceneDirector.teleporterSpawnCard = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/Teleporters/iscTeleporter.asset").WaitForCompletion();
                sceneDirector.expRewardCoefficient = 0.4f;
                sceneDirector.eliteBias = 9999f; // No elites on start
                sceneDirector.spawnDistanceMultiplier = 1.4f;

                var combatDirector = directorObj.AddComponent<CombatDirector>();
                combatDirector.customName = "Normal"; // Spawns non-elite enemies every now and then
                combatDirector.monsterCredit = 0f;
                combatDirector.eliteBias = 9999f;
                combatDirector.expRewardCoefficient = 0.2f;
                combatDirector.goldRewardCoefficient = 1f;
                combatDirector.minSeriesSpawnInterval = 0.1f;
                combatDirector.maxSeriesSpawnInterval = 1f;
                combatDirector.minRerollSpawnInterval = 2f;
                combatDirector.maxRerollSpawnInterval = 4f;
                combatDirector.moneyWaveIntervals = new RangeFloat[]
                {
                    new RangeFloat() { min = 10f, max = 20f }
                };
                combatDirector.creditMultiplier = 0.75f;
                combatDirector.skipSpawnIfTooCheap = false;
                combatDirector.teamIndex = TeamIndex.Monster;
                combatDirector.onSpawnedServer = new CombatDirector.OnSpawnedServer();

                combatDirector = directorObj.AddComponent<CombatDirector>();
                combatDirector.customName = "Strong"; // Sometimes spawns regular enemies, most of the time spawns chunky elites, longer spawn timer
                combatDirector.monsterCredit = 0f;
                combatDirector.eliteBias = 1f;
                combatDirector.expRewardCoefficient = 0.2f;
                combatDirector.goldRewardCoefficient = 1f;
                combatDirector.minSeriesSpawnInterval = 0.1f;
                combatDirector.maxSeriesSpawnInterval = 1f;
                combatDirector.minRerollSpawnInterval = 2f;
                combatDirector.maxRerollSpawnInterval = 4f;
                combatDirector.maximumNumberToSpawnBeforeSkipping = 1;
                combatDirector.moneyWaveIntervals = new RangeFloat[]
                {
                    new RangeFloat() { min = 30f, max = 60f }
                };
                combatDirector.creditMultiplier = 1.6f;
                combatDirector.teamIndex = TeamIndex.Monster;
                combatDirector.onSpawnedServer = new CombatDirector.OnSpawnedServer();

                void enableDirector()
                {
                    directorObj.SetActive(true);
                    RoR2Application.onLateUpdate -= enableDirector;
                }
                RoR2Application.onLateUpdate += enableDirector;

                // Add GameManager
                gameManagerObj = rootGameObjs.First(x => x.name == "GameManager");
                gameManagerObj.AddComponent<GlobalEventManager>();

                // Add GhostWaveController
                if (NetworkServer.active)
                {
                    ghostWaveControllerObj = Object.Instantiate(ghostWaveControllerObjPrefab);
                    NetworkServer.Spawn(ghostWaveControllerObj);
                }

                // Assign SurfaceDefs
                var worldObj = rootGameObjs.First(x => x.name == "World");

                var sdMystery = Addressables.LoadAssetAsync<SurfaceDef>("RoR2/Base/Common/sdMystery.asset").WaitForCompletion();
                var sdWood = Addressables.LoadAssetAsync<SurfaceDef>("RoR2/Base/Common/sdWood.asset").WaitForCompletion();
                var sdMetal = Addressables.LoadAssetAsync<SurfaceDef>("RoR2/Base/Common/sdMetal.asset").WaitForCompletion();
                var sdStone = Addressables.LoadAssetAsync<SurfaceDef>("RoR2/Base/Common/sdStone.asset").WaitForCompletion();

                worldObj.transform.Find("Main Island").gameObject.AddComponent<SurfaceDefProvider>().surfaceDef = sdMystery;
                void AssignSurfaceDefToChildColliders(SurfaceDef surfaceDef, List<string> parentNames)
                {
                    foreach (var objName in parentNames)
                    {
                        var tr = worldObj.transform.Find(objName);
                        if (tr)
                            foreach (var collider in tr.GetComponentsInChildren<Collider>())
                            {
                                var surfaceDefProvider = collider.GetComponent<SurfaceDefProvider>();
                                if (surfaceDefProvider != null) continue;
                                surfaceDefProvider = collider.gameObject.AddComponent<SurfaceDefProvider>();
                                surfaceDefProvider.surfaceDef = surfaceDef;
                            }
                    }
                }
                AssignSurfaceDefToChildColliders(sdWood, new List<string>()
                {
                    "island_tree_02_1k", "island_tree_01_1k"
                });
                AssignSurfaceDefToChildColliders(sdMetal, new List<string>()
                {
                    "ironFenceBorder", "ironFenceCurve", "ironFenceBorderGate", "ironFence", "ironFence (1)"
                });
                AssignSurfaceDefToChildColliders(sdStone, new List<string>()
                {
                    "cross", "gravestoneBroken", "gravestoneCrossLarge", "columnLarge"
                });

                // Set up MapZones
                var mapZonesObj = rootGameObjs.First(x => x.name == "MapZones");

                mapZonesObj.transform.Find("OutOfBounds/TriggerEnter/Top").position += Vector3.down * 100f;

                var zones = mapZonesObj.transform.Find("OutOfBounds/TriggerEnter");
                for (var i = 0; i < zones.childCount; i++)
                {
                    var mapZoneObj = zones.GetChild(i);
                    var mapZone = mapZoneObj.gameObject.AddComponent<MapZone>();
                    mapZone.triggerType = MapZone.TriggerType.TriggerEnter;
                    mapZone.zoneType = MapZone.ZoneType.OutOfBounds;
                }
                zones = mapZonesObj.transform.Find("OutOfBounds/TriggerExit");
                for (var i = 0; i < zones.childCount; i++)
                {
                    var mapZoneObj = zones.GetChild(i);
                    var mapZone = mapZoneObj.gameObject.AddComponent<MapZone>();
                    mapZone.triggerType = MapZone.TriggerType.TriggerExit;
                    mapZone.zoneType = MapZone.ZoneType.OutOfBounds;
                }

                // Create Log Pickup
                /* It kept falling off so I made it a final wave reward instead
                if (NetworkServer.active)
                {
                    var logPickup = Object.Instantiate(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/LogPickup.prefab").WaitForCompletion(), new Vector3(65.34444f, 99.01913f, -16.5297f), Quaternion.identity);
                    logPickup.GetComponentInChildren<UnlockPickup>().unlockableDef = UnlockableCatalog.GetUnlockableDef("Logs.Stages.BulwarksHaunt_GhostWave");
                    logPickup.GetComponent<TeamFilter>().teamIndex = TeamIndex.Player;
                    logPickup.GetComponentInChildren<VelocityRandomOnStart>().minSpeed = 0;
                    logPickup.GetComponentInChildren<VelocityRandomOnStart>().maxSpeed = 0;
                    NetworkServer.Spawn(logPickup);
                }
                */
            }
        }

        private void BazaarController_SetUpSeerStations(On.RoR2.BazaarController.orig_SetUpSeerStations orig, BazaarController self)
        {
            orig(self);
            var canReplaceStageInSeers = Run.instance.stageClearCount >= 8 && !Run.instance.GetEventFlag("NoMysterySpace");
            foreach (SeerStationController seerStationController in self.seerStations)
            {
                if (seerStationController.GetComponent<PurchaseInteraction>().available)
                {
                    if (canReplaceStageInSeers && self.rng.nextNormalizedFloat < 0.01f)
                    {
                        seerStationController.SetTargetScene(sceneDef);
                    }
                }
            }
        }

        public void SetUpGameEnding()
        {
            var gameEndingDef = ScriptableObject.CreateInstance<GameEndingDef>();
            gameEndingDef.cachedName = "BulwarksHaunt_HauntedEnding";
            gameEndingDef.backgroundColor = new Color32(76, 43, 30, 255);
            gameEndingDef.foregroundColor = new Color32(255, 144, 48, 255);
            gameEndingDef.endingTextToken = "GAME_RESULT_UNKNOWN";
            gameEndingDef.icon = BulwarksHauntPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/Bulwark's Haunt/texGameResultHauntedIcon.png");
            gameEndingDef.material = Addressables.LoadAssetAsync<GameEndingDef>("RoR2/Base/ClassicRun/LimboEnding.asset").WaitForCompletion().material;
            gameEndingDef.isWin = true;
            gameEndingDef.showCredits = false;
            gameEndingDef.gameOverControllerState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.GameOver.LingerShort));
            gameEndingDef.lunarCoinReward = 0; // Already gives coins for completing each wave

            BulwarksHauntContent.GameEndings.BulwarksHaunt_HauntedEnding = gameEndingDef;
            BulwarksHauntContent.Resources.gameEndingDefs.Add(gameEndingDef);
        }

        public void SetUpGamemode()
        {
            var ghostWaveController = ghostWaveControllerObjPrefab.AddComponent<BulwarksHauntGhostWaveController>();
            var esm = ghostWaveControllerObjPrefab.AddComponent<EntityStateMachine>();
            esm.mainStateType = esm.initialStateType = new EntityStates.SerializableEntityStateType(typeof(GhostWaveControllerBaseState.Intro));
            NetworkStateMachine nsm = ghostWaveControllerObjPrefab.AddComponent<NetworkStateMachine>();
            nsm.stateMachines = new EntityStateMachine[] {
                esm
            };
            var postProcessingObj = new GameObject("PostProcessing (FadeOut)");
            postProcessingObj.SetActive(false);
            postProcessingObj.layer = LayerIndex.postProcess.intVal;
            postProcessingObj.transform.SetParent(ghostWaveControllerObjPrefab.transform);
            PostProcessVolume postProcessVolume = postProcessingObj.AddComponent<PostProcessVolume>();
            postProcessVolume.isGlobal = true;
            postProcessVolume.priority = 70;
            postProcessVolume.weight = 0f;
            postProcessVolume.sharedProfile = BulwarksHauntPlugin.AssetBundle.LoadAsset<PostProcessProfile>("Assets/Mods/Bulwark's Haunt/GhostWave/ppGhostWaveFadeOut.asset");
            
            Run.onRunStartGlobal += Run_onRunStartGlobal;
            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;

            BulwarksHauntContent.Resources.entityStateTypes.Add(typeof(GhostWaveControllerBaseState));
            BulwarksHauntContent.Resources.entityStateTypes.Add(typeof(GhostWaveControllerBaseState.Intro));
            BulwarksHauntContent.Resources.entityStateTypes.Add(typeof(GhostWaveControllerBaseState.MonsterWaves));
            BulwarksHauntContent.Resources.entityStateTypes.Add(typeof(GhostWaveControllerBaseState.BreakBetweenWaves));
            BulwarksHauntContent.Resources.entityStateTypes.Add(typeof(GhostWaveControllerBaseState.Ending));
            BulwarksHauntContent.Resources.entityStateTypes.Add(typeof(GhostWaveControllerBaseState.FadeOut));
        }

        public static void PlaySoundForViewedCameras(string soundName)
        {
            foreach (var camera in CameraRigController.instancesList.Where(x => x.localUserViewer != null))
            {
                Util.PlaySound(soundName, camera.gameObject);
            }
        }

        private void Run_onRunStartGlobal(Run obj)
        {
            killedEnemiesPerStage.Clear();
            waveRng.Clear();
        }

        private void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            orig(self, damageReport);
            if (damageReport.victimTeamIndex == TeamIndex.Monster && damageReport.attackerTeamIndex == TeamIndex.Player && damageReport.victimMaster)
            {
                var currentStage = Run.instance.stageClearCount + 1;
                if (currentStage <= maxWaves && SceneInfo.instance.countsAsStage)
                {
                    var killedEnemyData = new KilledEnemyData
                    {
                        bodyIndex = damageReport.victimBodyIndex,
                        inventory = new List<ItemCountPair>(),
                        equipment = null
                    };
                    if (damageReport.victimBody)
                    {
                        if (damageReport.victimBody.inventory)
                        {
                            foreach (var itemIndex in damageReport.victimBody.inventory.itemAcquisitionOrder)
                            {
                                var itemCount = damageReport.victimBody.inventory.itemStacks[(int)itemIndex];
                                killedEnemyData.inventory.Add(new ItemCountPair() { itemDef = ItemCatalog.GetItemDef(itemIndex), count = itemCount });
                            }
                        }
                        if (damageReport.victimBody.equipmentSlot)
                        {
                            killedEnemyData.equipment = EquipmentCatalog.GetEquipmentDef(damageReport.victimBody.equipmentSlot.equipmentIndex);
                        }
                    }

                    if (!killedEnemiesPerStage.ContainsKey(currentStage))
                    {
                        killedEnemiesPerStage.Add(currentStage, new List<KilledEnemyData>());
                        var newRng = new Xoroshiro128Plus(Run.instance.runRNG);
                        for (var i = 0; i < currentStage; i++) newRng = new Xoroshiro128Plus(newRng.nextUlong);
                        waveRng.Add(currentStage, newRng);
                    }
                    killedEnemiesPerStage[currentStage].Add(killedEnemyData);

                    while (killedEnemiesPerStage[currentStage].Count > waveEnemyHardCap)
                    {
                        killedEnemiesPerStage[currentStage].RemoveAt(0);
                    }
                }
            }
        }

        public static Dictionary<int, List<KilledEnemyData>> killedEnemiesPerStage = new Dictionary<int, List<KilledEnemyData>>();
        public static Dictionary<int, Xoroshiro128Plus> waveRng = new Dictionary<int, Xoroshiro128Plus>();
        public static int waveEnemyHardCap = 120;
        public struct KilledEnemyData
        {
            public BodyIndex bodyIndex;
            public List<ItemCountPair> inventory;
            public EquipmentDef equipment;
        }
        public static CharacterSpawnCard GenerateSpawnCardForKilledEnemy(KilledEnemyData killedEnemyData)
        {
            var bodyPrefab = BodyCatalog.GetBodyPrefab(killedEnemyData.bodyIndex);
            var bodyComponent = bodyPrefab.GetComponent<CharacterBody>();
            var bodyName = BodyCatalog.GetBodyName(killedEnemyData.bodyIndex);

            var newEnemySpawnCard = ScriptableObject.CreateInstance<CharacterSpawnCard>();
            newEnemySpawnCard.hullSize = bodyComponent.hullClassification;
            newEnemySpawnCard.prefab = MasterCatalog.GetMasterPrefab(MasterCatalog.FindAiMasterIndexForBody(killedEnemyData.bodyIndex));
            newEnemySpawnCard.itemsToGrant = killedEnemyData.inventory.ToArray();
            newEnemySpawnCard.equipmentToGrant = new EquipmentDef[] { };
            if (killedEnemyData.equipment != null)
                HG.ArrayUtils.ArrayAppend(ref newEnemySpawnCard.equipmentToGrant, killedEnemyData.equipment);
            newEnemySpawnCard.nodeGraphType = bodyComponent.isFlying ? MapNodeGroup.GraphType.Air : MapNodeGroup.GraphType.Ground;
            if (bodyName == "GrandParentBody") newEnemySpawnCard.nodeGraphType = MapNodeGroup.GraphType.Ground;

            if (newEnemySpawnCard.prefab == null) return null;

            return newEnemySpawnCard;
        }

        public static int maxWaves = 8;

        public class BulwarksHauntGhostWaveController : MonoBehaviour
        {
            public static BulwarksHauntGhostWaveController instance;
            
            public int wave = 1;
            public int currentSpeechVariant = 1;

            public float oobDamageTimer = 0f;
            public float oobDamageInterval = 5f;
            public float oobRadius = 350f;
            public float oobRadiusShrinkAmount = 100f;
            public float oobShrinkDuration = 300f;
            public float oobShrinkTimer = 0f;
            public float oobHealthLoss = 0.34f;
            public Vector3 oobCenter = Vector3.zero;

            public void Start()
            {
                instance = this;
            }

            public void FixedUpdate()
            {
                if (NetworkServer.active)
                {
                    oobDamageTimer -= Time.fixedDeltaTime;
                    while (oobDamageTimer <= 0)
                    {
                        oobDamageTimer += oobDamageInterval;

                        var currentRadius = oobRadius - oobRadiusShrinkAmount * Mathf.Min(oobShrinkTimer / oobShrinkDuration, 1f);
                        for (TeamIndex teamIndex = TeamIndex.Neutral; teamIndex < TeamIndex.Count; teamIndex += 1)
                        {
                            if (teamIndex != TeamIndex.Player && teamIndex != TeamIndex.None && teamIndex != TeamIndex.Neutral)
                            {
                                foreach (TeamComponent teamComponent in TeamComponent.GetTeamMembers(teamIndex))
                                {
                                    CharacterBody body = teamComponent.body;
                                    if (!body.healthComponent) continue;
                                    if (Vector3.Distance(teamComponent.transform.position, oobCenter) >= currentRadius)
                                    {
                                        body.healthComponent.TakeDamage(new DamageInfo
                                        {
                                            damage = oobHealthLoss * body.healthComponent.fullCombinedHealth,
                                            position = body.corePosition,
                                            damageType = DamageType.BypassArmor | DamageType.BypassBlock,
                                            damageColorIndex = DamageColorIndex.Void
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public void OnEnable()
            {
                RoR2.UI.ObjectivePanelController.collectObjectiveSources += ObjectivePanelController_collectObjectiveSources;
            }

            public void OnCompleted()
            {
                RoR2.UI.ObjectivePanelController.collectObjectiveSources -= ObjectivePanelController_collectObjectiveSources;
            }

            public void OnDisable()
            {
                RoR2.UI.ObjectivePanelController.collectObjectiveSources -= ObjectivePanelController_collectObjectiveSources;
            }

            private void ObjectivePanelController_collectObjectiveSources(CharacterMaster master, List<RoR2.UI.ObjectivePanelController.ObjectiveSourceDescriptor> objectiveSourcesList)
            {
                objectiveSourcesList.Add(new RoR2.UI.ObjectivePanelController.ObjectiveSourceDescriptor
                {
                    master = master,
                    objectiveType = typeof(ClearGhostWavesObjectiveController),
                    source = this
                });
            }

            public class ClearGhostWavesObjectiveController : RoR2.UI.ObjectivePanelController.ObjectiveTracker
            {
                public override string GenerateString()
                {
                    var controller = (BulwarksHauntGhostWaveController)sourceDescriptor.source;
                    wave = controller.wave;
                    if (wave == maxWaves) return Language.GetString("OBJECTIVE_BULWARKSHAUNT_GHOSTWAVE_CLEARWAVES_FINAL");
                    return string.Format(Language.GetString("OBJECTIVE_BULWARKSHAUNT_GHOSTWAVE_CLEARWAVES"), wave);
                }

                public override bool IsDirty()
                {
                    return ((BulwarksHauntGhostWaveController)sourceDescriptor.source).wave != wave;
                }

                public int wave = -1;
            }
        }

        public class GhostWaveControllerBaseState : EntityStates.EntityState
        {
            public string chatFormatStringToken = "BULWARKSHAUNT_GHOSTWAVE_VOICE_FORMAT";

            public BulwarksHauntGhostWaveController ghostWaveController;

            public override void OnEnter()
            {
                base.OnEnter();
                ghostWaveController = GetComponent<BulwarksHauntGhostWaveController>();
            }

            public override void Update()
            {
                base.Update();
            }

            public override void OnExit()
            {
                base.OnExit();
            }

            public class Intro : GhostWaveControllerBaseState
            {
                public float speechTimer = 5f;
                public float speechInterval = 8f;
                public int speechVariant = 1;
                public int speechProgress = 1;

                public override void OnEnter()
                {
                    base.OnEnter();
                    speechVariant = RoR2Application.rng.RangeInt(1, 4);
                }

                public override void Update()
                {
                    base.Update();
                    if (Run.instance.livingPlayerCount > 0)
                        speechTimer -= Time.deltaTime;
                    if (speechTimer <= 0f)
                    {
                        var speechToken = "BULWARKSHAUNT_GHOSTWAVE_INTRO_" + speechVariant + "_" + speechProgress;
                        if (!Language.IsTokenInvalid(speechToken))
                        {
                            if (NetworkServer.active)
                            {
                                Chat.SendBroadcastChat(new Chat.NpcChatMessage
                                {
                                    baseToken = speechToken,
                                    formatStringToken = chatFormatStringToken,
                                    sender = null,
                                    sound = null
                                });
                            }
                        }
                        else
                        {
                            if (isAuthority)
                                outer.SetNextState(new MonsterWaves());
                        }
                        speechTimer = speechInterval;
                        speechProgress++;
                    }
                }
            }

            public class MonsterWaves : GhostWaveControllerBaseState
            {
                public List<KilledEnemyData> enemiesThisWave;
                public Xoroshiro128Plus rng;

                public List<CharacterMaster> aliveEnemies = new List<CharacterMaster>();
                public int killedEnemies = 0;
                public int totalEnemies = 0;

                public float spawnTimer = 5f;
                public float spawnInterval = 20f;
                public int spawnBatchCount = 10;
                public int speechVariant = 1;

                public float spawnBatchTimer = 0f;
                public float spawnBatchInterval = 0.33f;
                public int spawnBatchCurrent = 0;

                public float aliveEnemiesRecheckTimer = 60f;
                public float aliveEnemiesRecheckInterval = 60f;
                public float antiSoftlockTimer = 600f;

                public override void OnEnter()
                {
                    base.OnEnter();

                    if (ghostWaveController.wave == 1)
                    {
                        speechVariant = RoR2Application.rng.RangeInt(1, 4);
                        ghostWaveController.currentSpeechVariant = speechVariant;
                    }
                    else
                    {
                        speechVariant = ghostWaveController.currentSpeechVariant;
                    }

                    PlaySoundForViewedCameras("Play_ui_obj_nullWard_activate");

                    if (killedEnemiesPerStage.ContainsKey(ghostWaveController.wave))
                    {
                        enemiesThisWave = killedEnemiesPerStage[ghostWaveController.wave].Where((x) =>
                        {
                            if (x.bodyIndex == BodyIndex.None || !BodyCatalog.GetBodyPrefab(x.bodyIndex)) return false;
                            return true;
                        }).ToList();
                        totalEnemies = enemiesThisWave.Count;
                        if (NetworkServer.active)
                            new SyncTotalEnemies(gameObject.GetComponent<NetworkIdentity>().netId, totalEnemies).Send(NetworkDestination.Clients);
                        rng = new Xoroshiro128Plus(waveRng[ghostWaveController.wave]);
                    }
                    else
                    {
                        // Fallback - Add random enemy ghosts
                        if (NetworkServer.active)
                        {
                            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                            {
                                baseToken = "BULWARKSHAUNT_GHOSTWAVE_SYSTEM_WAVEFALLBACK",
                                paramTokens = new string[]
                                {
                                    ghostWaveController.wave.ToString()
                                }
                            });
                        }
                        
                        enemiesThisWave = new List<KilledEnemyData>();
                        rng = new Xoroshiro128Plus(RoR2Application.rng.nextUlong);

                        var smallEnemies = new List<BodyIndex>()
                        {
                            BodyCatalog.FindBodyIndex("BeetleBody"),
                            BodyCatalog.FindBodyIndex("WispBody"),
                            BodyCatalog.FindBodyIndex("LemurianBody"),
                            BodyCatalog.FindBodyIndex("ImpBody"),
                            BodyCatalog.FindBodyIndex("JellyfishBody")
                        };
                        var smallEnemiesCount = Mathf.FloorToInt(18f + 7f * (float)ghostWaveController.wave);

                        var heavyEnemies = new List<BodyIndex>()
                        {
                            BodyCatalog.FindBodyIndex("BeetleGuardBody"),
                            BodyCatalog.FindBodyIndex("GreaterWispBody"),
                            BodyCatalog.FindBodyIndex("LemurianBruiserBody"),
                            BodyCatalog.FindBodyIndex("BellBody"),
                            BodyCatalog.FindBodyIndex("ClayBruiserBody"),
                            BodyCatalog.FindBodyIndex("BellBody"),
                            BodyCatalog.FindBodyIndex("MiniMushroomBody"),
                            BodyCatalog.FindBodyIndex("ParentBody"),
                            BodyCatalog.FindBodyIndex("GolemBody")
                        };
                        var heavyEnemiesCount = Mathf.FloorToInt(2f + 3f * (float)ghostWaveController.wave);

                        var bosses = new List<BodyIndex>()
                        {
                            BodyCatalog.FindBodyIndex("BeetleQueen2Body"),
                            BodyCatalog.FindBodyIndex("ClayBossBody"),
                            BodyCatalog.FindBodyIndex("GrandParentBody"),
                            BodyCatalog.FindBodyIndex("GravekeeperBody"),
                            BodyCatalog.FindBodyIndex("ImpBossBody"),
                            BodyCatalog.FindBodyIndex("MagmaWormBody"),
                            BodyCatalog.FindBodyIndex("RoboBallBossBody"),
                            BodyCatalog.FindBodyIndex("VagrantBody"),
                            BodyCatalog.FindBodyIndex("TitanBody")
                        };
                        var bossesCount = Mathf.FloorToInt(1f + 0.5f * (float)ghostWaveController.wave);

                        var eliteRoll = new WeightedSelection<EquipmentDef>();
                        eliteRoll.AddChoice(null, 1f);
                        if (ghostWaveController.wave >= 3)
                        {
                            eliteRoll.AddChoice(RoR2Content.Equipment.AffixRed, 0.0111f * (float)ghostWaveController.wave);
                            eliteRoll.AddChoice(RoR2Content.Equipment.AffixBlue, 0.0111f * (float)ghostWaveController.wave);
                            eliteRoll.AddChoice(RoR2Content.Equipment.AffixWhite, 0.0111f * (float)ghostWaveController.wave);
                        }
                        if (ghostWaveController.wave >= 6)
                        {
                            eliteRoll.AddChoice(RoR2Content.Equipment.AffixPoison, 0.0085f * (float)ghostWaveController.wave);
                            eliteRoll.AddChoice(RoR2Content.Equipment.AffixHaunted, 0.0085f * (float)ghostWaveController.wave);
                        }

                        for (var i = 0; i < smallEnemiesCount; i++)
                        {
                            enemiesThisWave.Add(new KilledEnemyData()
                            {
                                bodyIndex = rng.NextElementUniform(smallEnemies),
                                inventory = new List<ItemCountPair>(),
                                equipment = eliteRoll.Evaluate(rng.nextNormalizedFloat)
                            });
                        }
                        for (var i = 0; i < heavyEnemiesCount; i++)
                        {
                            enemiesThisWave.Add(new KilledEnemyData()
                            {
                                bodyIndex = rng.NextElementUniform(heavyEnemies),
                                inventory = new List<ItemCountPair>(),
                                equipment = eliteRoll.Evaluate(rng.nextNormalizedFloat)
                            });
                        }
                        for (var i = 0; i < bossesCount; i++)
                        {
                            enemiesThisWave.Add(new KilledEnemyData()
                            {
                                bodyIndex = rng.NextElementUniform(bosses),
                                inventory = new List<ItemCountPair>(),
                                equipment = null
                            });
                        }

                        enemiesThisWave = enemiesThisWave.Where((x) =>
                        {
                            if (x.bodyIndex == BodyIndex.None || !BodyCatalog.GetBodyPrefab(x.bodyIndex)) return false;
                            return true;
                        }).ToList();
                        totalEnemies = enemiesThisWave.Count;
                        if (NetworkServer.active)
                            new SyncTotalEnemies(gameObject.GetComponent<NetworkIdentity>().netId, totalEnemies).Send(NetworkDestination.Clients);
                    }
                    spawnBatchCount += 2 * (ghostWaveController.wave - 1);
                    GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;

                    RoR2.UI.ObjectivePanelController.collectObjectiveSources += ObjectivePanelController_collectObjectiveSources;
                }

                public uint GetCoinReward()
                {
                    return (uint)(1 + Mathf.CeilToInt((float)ghostWaveController.wave / 4f));
                }

                private void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
                {
                    if (aliveEnemies.Contains(damageReport.victimMaster))
                    {
                        aliveEnemies.Remove(damageReport.victimMaster);
                        killedEnemies++;
                        if (NetworkServer.active)
                            new SyncKilledEnemies(gameObject.GetComponent<NetworkIdentity>().netId, killedEnemies).Send(NetworkDestination.Clients);
                        aliveEnemiesRecheckTimer = 0.1f;
                        if (aliveEnemies.Count <= 0)
                            spawnTimer = 1f;
                    }

                    if (damageReport.victimMaster?.playerCharacterMasterController != null)
                    {
                        if (NetworkServer.active)
                        {
                            Chat.SendBroadcastChat(new Chat.NpcChatMessage
                            {
                                baseToken = "BULWARKSHAUNT_GHOSTWAVE_LOSS_" + RoR2Application.rng.RangeInt(1, 5),
                                formatStringToken = chatFormatStringToken,
                                sender = null,
                                sound = null
                            });
                        }
                    }
                }

                public override void FixedUpdate()
                {
                    base.FixedUpdate();
                    if (NetworkServer.active)
                    {
                        if (enemiesThisWave.Count > 0)
                        {
                            spawnTimer -= Time.fixedDeltaTime;
                            if (spawnTimer <= 0f)
                            {
                                spawnTimer += spawnInterval;
                                spawnBatchTimer = 0f;
                                spawnBatchCurrent = spawnBatchCount;
                            }

                            if (spawnBatchCurrent > 0)
                            {
                                spawnBatchTimer -= Time.fixedDeltaTime;
                                if (spawnBatchTimer <= 0f)
                                {
                                    spawnBatchTimer += spawnBatchInterval;
                                    TryUnloadFromBatch();
                                }
                            }
                        }

                        aliveEnemiesRecheckTimer -= Time.fixedDeltaTime;
                        antiSoftlockTimer -= Time.fixedDeltaTime;
                        if ((aliveEnemiesRecheckTimer <= 0f || antiSoftlockTimer <= 0f) && Run.instance.livingPlayerCount > 0)
                        {
                            aliveEnemiesRecheckTimer = aliveEnemiesRecheckInterval;
                            CheckAllEnemiesDead();
                        }

                        ghostWaveController.oobShrinkTimer += Time.fixedDeltaTime;
                    }
                }

                public void TryUnloadFromBatch()
                {
                    TeamDef teamDef = TeamCatalog.GetTeamDef(TeamIndex.Monster);
                    if (TeamComponent.GetTeamMembers(TeamIndex.Monster).Count >= teamDef.softCharacterLimit)
                    {
                        return;
                    }

                    var newEnemy = rng.NextElementUniform(enemiesThisWave);
                    var newEnemySpawnCard = GenerateSpawnCardForKilledEnemy(newEnemy);

                    if (newEnemySpawnCard) {
                        DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(newEnemySpawnCard, new DirectorPlacementRule
                        {
                            placementMode = DirectorPlacementRule.PlacementMode.Random
                        }, RoR2Application.rng);
                        directorSpawnRequest.teamIndexOverride = TeamIndex.Monster;
                        directorSpawnRequest.ignoreTeamMemberLimit = true;
                        directorSpawnRequest.onSpawnedServer += (spawnResult) =>
                        {
                            if (spawnResult.success)
                            {
                                enemiesThisWave.Remove(newEnemy);
                                spawnBatchCurrent--;

                                var master = spawnResult.spawnedInstance.GetComponent<CharacterMaster>();
                                aliveEnemies.Add(master);

                                if (master.inventory && master.inventory.GetItemCount(BulwarksHauntContent.Items.BulwarksHaunt_GhostFury) <= 0)
                                {
                                    master.inventory.GiveItem(BulwarksHauntContent.Items.BulwarksHaunt_GhostFury);
                                }
                            }
                        };
                        DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
                    } else
                    {
                        enemiesThisWave.Remove(newEnemy);
                        spawnBatchCurrent--;
                        killedEnemies++;
                        if (NetworkServer.active)
                            new SyncKilledEnemies(gameObject.GetComponent<NetworkIdentity>().netId, killedEnemies).Send(NetworkDestination.Clients);
                    }
                }

                public void CheckAllEnemiesDead()
                {
                    if (!NetworkServer.active) return;

                    FilterInvalidAliveEnemies();
                    if (killedEnemies >= totalEnemies || antiSoftlockTimer <= 0f || (aliveEnemies.Count == 0 && enemiesThisWave.Count == 0))
                    {
                        Advance();
                        foreach (var enemy in TeamComponent.GetTeamMembers(TeamIndex.Monster))
                        {
                            if (enemy.body && enemy.body.healthComponent && enemy.body.healthComponent.alive)
                            {
                                enemy.body.healthComponent.Suicide();
                            }
                        }
                    }
                }

                public void FilterInvalidAliveEnemies()
                {
                    var count = aliveEnemies.Count;
                    aliveEnemies = aliveEnemies.Where(x => x != null && x.hasBody).ToList();
                    if (aliveEnemies.Count < count)
                    {
                        killedEnemies += count - aliveEnemies.Count;
                    }
                }

                public void Advance()
                {
                    if (NetworkServer.active)
                    {
                        RespawnPlayers();

                        var coinReward = GetCoinReward();
                        for (int i = 0; i < NetworkUser.readOnlyInstancesList.Count; i++)
                        {
                            NetworkUser networkUser = NetworkUser.readOnlyInstancesList[i];
                            if (networkUser && networkUser.isParticipating)
                            {
                                networkUser.AwardLunarCoins(coinReward);
                            }
                        }

                        Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                        {
                            baseToken = "BULWARKSHAUNT_GHOSTWAVE_SYSTEM_WAVECLEAR",
                            paramTokens = new string[]
                            {
                                coinReward.ToString()
                            }
                        });
                    }

                    var nextWave = ghostWaveController.wave + 1;
                    if (nextWave <= maxWaves)
                    {
                        if (isAuthority)
                            outer.SetNextState(new BreakBetweenWaves());
                        if (NetworkServer.active)
                        {
                            Chat.SendBroadcastChat(new Chat.NpcChatMessage
                            {
                                baseToken = "BULWARKSHAUNT_GHOSTWAVE_WAVECLEAR_" + speechVariant + "_" + ghostWaveController.wave,
                                formatStringToken = chatFormatStringToken,
                                sender = null,
                                sound = null
                            });
                        }
                    }
                    else
                    {
                        if (isAuthority)
                            outer.SetNextState(new Ending());
                    }
                }

                public void RespawnPlayers()
                {
                    foreach (PlayerCharacterMasterController pcmc in PlayerCharacterMasterController.instances)
                    {
                        CharacterMaster master = pcmc.master;
                        if (pcmc.isConnected && master.IsDeadAndOutOfLivesServer())
                        {
                            Vector3 vector = master.deathFootPosition;
                            master.Respawn(vector, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                            CharacterBody body = master.GetBody();
                            if (body)
                            {
                                body.AddTimedBuff(RoR2Content.Buffs.Immune, 3f);
                                foreach (EntityStateMachine entityStateMachine in body.GetComponents<EntityStateMachine>())
                                {
                                    entityStateMachine.initialStateType = entityStateMachine.mainStateType;
                                }
                                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/HippoRezEffect"), new EffectData
                                {
                                    origin = vector,
                                    rotation = body.transform.rotation
                                }, true);
                            }
                        }
                    }
                }

                public override void OnExit()
                {
                    base.OnExit();
                    ghostWaveController.oobShrinkTimer = 0;
                    PlaySoundForViewedCameras("Play_ui_obj_nullWard_complete");
                    GlobalEventManager.onCharacterDeathGlobal -= GlobalEventManager_onCharacterDeathGlobal;
                    RoR2.UI.ObjectivePanelController.collectObjectiveSources -= ObjectivePanelController_collectObjectiveSources;
                }

                private void ObjectivePanelController_collectObjectiveSources(CharacterMaster master, List<RoR2.UI.ObjectivePanelController.ObjectiveSourceDescriptor> objectiveSourcesList)
                {
                    objectiveSourcesList.Add(new RoR2.UI.ObjectivePanelController.ObjectiveSourceDescriptor
                    {
                        master = master,
                        objectiveType = typeof(KillGhostEnemiesObjectiveController),
                        source = outer
                    });
                }

                public class KillGhostEnemiesObjectiveController : RoR2.UI.ObjectivePanelController.ObjectiveTracker
                {
                    public override string GenerateString()
                    {
                        var controller = (MonsterWaves)((EntityStateMachine)sourceDescriptor.source).state;
                        killedEnemies = controller.killedEnemies;
                        totalEnemies = Mathf.Max(controller.totalEnemies, 1);
                        var progress = (float)killedEnemies / (float)totalEnemies;
                        if (killedEnemies == -1) progress = 0f;
                        return string.Format(Language.GetString("OBJECTIVE_BULWARKSHAUNT_GHOSTWAVE_KILLENEMIES"), Mathf.FloorToInt(progress * 100f).ToString());
                    }

                    public override bool IsDirty()
                    {
                        var state = ((EntityStateMachine)sourceDescriptor.source).state as MonsterWaves;
                        if (state != null)
                            return state.killedEnemies != killedEnemies;
                        return false;
                    }

                    public int killedEnemies = -1;
                    public int totalEnemies = 1;
                }

                public class SyncTotalEnemies : INetMessage
                {
                    NetworkInstanceId objID;
                    int totalEnemies;

                    public SyncTotalEnemies()
                    {
                    }

                    public SyncTotalEnemies(NetworkInstanceId objID, int totalEnemies)
                    {
                        this.objID = objID;
                        this.totalEnemies = totalEnemies;
                    }

                    public void Deserialize(NetworkReader reader)
                    {
                        objID = reader.ReadNetworkId();
                        totalEnemies = reader.ReadInt32();
                    }

                    public void OnReceived()
                    {
                        if (NetworkServer.active) return;
                        GameObject obj = Util.FindNetworkObject(objID);
                        if (obj)
                        {
                            EntityStateMachine esm = obj.GetComponent<EntityStateMachine>();
                            if (esm)
                            {
                                var state = esm.state as MonsterWaves;
                                if (state != null)
                                {
                                    state.totalEnemies = totalEnemies;
                                }
                            }
                        }
                    }

                    public void Serialize(NetworkWriter writer)
                    {
                        writer.Write(objID);
                        writer.Write(totalEnemies);
                    }
                }

                public class SyncKilledEnemies : INetMessage
                {
                    NetworkInstanceId objID;
                    int killedEnemies;

                    public SyncKilledEnemies()
                    {
                    }

                    public SyncKilledEnemies(NetworkInstanceId objID, int killedEnemies)
                    {
                        this.objID = objID;
                        this.killedEnemies = killedEnemies;
                    }

                    public void Deserialize(NetworkReader reader)
                    {
                        objID = reader.ReadNetworkId();
                        killedEnemies = reader.ReadInt32();
                    }

                    public void OnReceived()
                    {
                        if (NetworkServer.active) return;
                        GameObject obj = Util.FindNetworkObject(objID);
                        if (obj)
                        {
                            EntityStateMachine esm = obj.GetComponent<EntityStateMachine>();
                            if (esm)
                            {
                                var state = esm.state as MonsterWaves;
                                if (state != null)
                                {
                                    state.killedEnemies = killedEnemies;
                                }
                            }
                        }
                    }

                    public void Serialize(NetworkWriter writer)
                    {
                        writer.Write(objID);
                        writer.Write(killedEnemies);
                    }
                }
            }

            public class BreakBetweenWaves : GhostWaveControllerBaseState
            {
                public float timeLeft = 15f;

                public override void OnEnter()
                {
                    base.OnEnter();
                    RoR2.UI.ObjectivePanelController.collectObjectiveSources += ObjectivePanelController_collectObjectiveSources;
                }

                private void ObjectivePanelController_collectObjectiveSources(CharacterMaster master, List<RoR2.UI.ObjectivePanelController.ObjectiveSourceDescriptor> objectiveSourcesList)
                {
                    objectiveSourcesList.Add(new RoR2.UI.ObjectivePanelController.ObjectiveSourceDescriptor
                    {
                        master = master,
                        objectiveType = typeof(WaitForNextWaveObjectiveController),
                        source = outer
                    });
                }

                public override void OnExit()
                {
                    base.OnExit();
                    RoR2.UI.ObjectivePanelController.collectObjectiveSources -= ObjectivePanelController_collectObjectiveSources;
                    ghostWaveController.wave++;
                }

                public override void Update()
                {
                    base.Update();
                    if (Run.instance.livingPlayerCount > 0)
                        timeLeft -= Time.deltaTime;
                    if (timeLeft <= 0 && isAuthority)
                    {
                        outer.SetNextState(new MonsterWaves());
                    }
                }

                public class WaitForNextWaveObjectiveController : RoR2.UI.ObjectivePanelController.ObjectiveTracker
                {
                    public override string GenerateString()
                    {
                        var controller = (BreakBetweenWaves)((EntityStateMachine)sourceDescriptor.source).state;
                        var timeLeft = Mathf.Max(controller.timeLeft, 0f);
                        var t = System.TimeSpan.FromSeconds(timeLeft);
                        var formattedTime = string.Format("{0:D2}:{1:D2}",
                            t.Seconds,
                            t.Milliseconds / 10
                        );
                        return string.Format(Language.GetString("OBJECTIVE_BULWARKSHAUNT_GHOSTWAVE_WAITFORNEXTWAVE"), formattedTime);
                    }

                    public override bool IsDirty()
                    {
                        return (((EntityStateMachine)sourceDescriptor.source).state as BreakBetweenWaves) != null;
                    }
                }
            }

            public class Ending : GhostWaveControllerBaseState
            {
                public float speechTimer = 0f;
                public float speechInterval = 8f;
                public int speechVariant = 1;
                public int speechProgress = 1;

                public override void OnEnter()
                {
                    base.OnEnter();
                    PlaySoundForViewedCameras("Play_moonBrother_swing_horizontal");
                    if (NetworkServer.active)
                    {
                        var logPickup = Object.Instantiate(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/LogPickup.prefab").WaitForCompletion(), new Vector3(-3.090973f, 65.82108f, 42.75804f), Quaternion.identity);
                        logPickup.GetComponentInChildren<UnlockPickup>().unlockableDef = UnlockableCatalog.GetUnlockableDef("Logs.Stages.BulwarksHaunt_GhostWave");
                        logPickup.GetComponent<TeamFilter>().teamIndex = TeamIndex.Player;
                        NetworkServer.Spawn(logPickup);
                    }
                    speechVariant = RoR2Application.rng.RangeInt(1, 4);
                    ghostWaveController.OnCompleted();

                    var combatDirectors = new List<CombatDirector>(CombatDirector.instancesList);
                    foreach (var combatDirector in combatDirectors)
                    {
                        combatDirector.enabled = false;
                    }
                }

                public override void Update()
                {
                    base.Update();
                    if (Run.instance.livingPlayerCount > 0)
                        speechTimer -= Time.deltaTime;
                    if (speechTimer <= 0f)
                    {
                        var speechToken = "BULWARKSHAUNT_GHOSTWAVE_WIN_" + speechVariant + "_" + speechProgress;
                        var speechTokenNext = "BULWARKSHAUNT_GHOSTWAVE_WIN_" + speechVariant + "_" + (speechProgress + 1);
                        if (!Language.IsTokenInvalid(speechToken))
                        {
                            if (NetworkServer.active)
                            {
                                Chat.SendBroadcastChat(new Chat.NpcChatMessage
                                {
                                    baseToken = speechToken,
                                    formatStringToken = chatFormatStringToken,
                                    sender = null,
                                    sound = null
                                });
                            }
                        }
                        if (Language.IsTokenInvalid(speechTokenNext) || Language.IsTokenInvalid(speechToken))
                        {
                            if (isAuthority)
                                outer.SetNextState(new FadeOut());
                        }
                        speechProgress++;
                        speechTimer = speechInterval;
                    }
                }
            }

            public class FadeOut : GhostWaveControllerBaseState
            {
                public float duration = 8f;
                public GameObject postProcessingObj;
                public PostProcessVolume postProcessVolume;
                public bool ended = false;

                public override void OnEnter()
                {
                    base.OnEnter();
                    postProcessingObj = transform.Find("PostProcessing (FadeOut)").gameObject;
                    if (postProcessingObj)
                    {
                        postProcessingObj.SetActive(true);
                        postProcessVolume = postProcessingObj.GetComponent<PostProcessVolume>();
                    }
                }

                public override void Update()
                {
                    base.Update();

                    float num = Mathf.Clamp01(age / duration);
                    num *= num;
                    if (postProcessVolume)
                    {
                        postProcessVolume.weight = num;
                    }

                    if (!ended && num >= 1f)
                    {
                        ended = true;
                        PlaySoundForViewedCameras("Play_elite_haunt_spawn");
                        if (NetworkServer.active)
                            Run.instance.BeginGameOver(BulwarksHauntContent.GameEndings.BulwarksHaunt_HauntedEnding);
                    }
                }

                public override void FixedUpdate()
                {
                    base.FixedUpdate();
                    if (ended)
                    {
                        foreach (CharacterBody characterBody in CharacterBody.readOnlyInstancesList)
                        {
                            if (characterBody.hasEffectiveAuthority)
                            {
                                EntityStateMachine entityStateMachine = EntityStateMachine.FindByCustomName(characterBody.gameObject, "Body");
                                if (entityStateMachine && !(entityStateMachine.state is EntityStates.Idle))
                                {
                                    entityStateMachine.SetInterruptState(new EntityStates.Idle(), EntityStates.InterruptPriority.Frozen);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
