using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using RoR2;
using UnityEngine;
using R2API;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace BulwarksHaunt.Items
{
    public class Sword : BaseItem
    {
        public static GameObject swordObjPrefab;

        public static ConfigOptions.ConfigurableValue<bool> swordCanBeUnleashed = ConfigOptions.ConfigurableValue.CreateBool(
            BulwarksHauntPlugin.PluginGUID,
            BulwarksHauntPlugin.PluginName,
            BulwarksHauntPlugin.config,
            "Unleashed Blade",
            "Enabled",
            true,
            "If false, the Crystalline Blade will have no extra function, even if the challenge for that is completed. Does not take effect if you have one in your inventory."
        );

        public override void OnPluginAwake()
        {
            swordObjPrefab = Utils.CreateBlankPrefab("BulwarksHaunt_SwordObj", true);
        }

        public override void OnLoad()
        {
            base.OnLoad();
            itemDef.name = "BulwarksHaunt_Sword";
            SetItemTierWhenAvailable(ItemTier.Boss);
            itemDef.tags = new ItemTag[] {
                ItemTag.CannotCopy,
                ItemTag.CannotDuplicate,
                ItemTag.ObliterationRelated,
                ItemTag.WorldUnique
            };
            itemDef.pickupModelPrefab = PrepareModel(BulwarksHauntPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/Bulwark's Haunt/Sword/ItemModel.prefab"));
            itemDef.pickupIconSprite = BulwarksHauntPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/Bulwark's Haunt/Sword/Icon.png");

            var mat = itemDef.pickupModelPrefab.GetComponentInChildren<Renderer>().sharedMaterial;
            HopooShaderToMaterial.Standard.Apply(mat);
            HopooShaderToMaterial.Standard.Emission(mat, 1.2f, new Color32(5, 191, 163, 255));

            itemDisplayPrefab = PrepareItemDisplayModel(PrefabAPI.InstantiateClone(itemDef.pickupModelPrefab, itemDef.pickupModelPrefab.name + "Display", false));

            onSetupIDRS += () =>
            {
                AddDisplayRule("CommandoBody", "Chest", new Vector3(0.04253F, 0.29663F, -0.25561F), new Vector3(29.55021F, 82.85622F, 8.9027F), new Vector3(0.30648F, 0.30648F, 0.30648F));
                AddDisplayRule("HuntressBody", "Chest", new Vector3(0.11166F, 0.04213F, -0.2108F), new Vector3(26.45465F, 57.81559F, 6.97784F), new Vector3(0.28294F, 0.28294F, 0.28294F));
                AddDisplayRule("Bandit2Body", "Chest", new Vector3(0.01912F, 0.021F, -0.21426F), new Vector3(351.0675F, 84.49586F, 355.5016F), new Vector3(0.31408F, 0.31408F, 0.31408F));
                AddDisplayRule("ToolbotBody", "Chest", new Vector3(-0.01146F, 1.74499F, -2.56739F), new Vector3(346.6049F, 90F, 0F), new Vector3(2.12877F, 2.12877F, 2.12877F));
                AddDisplayRule("EngiBody", "Chest", new Vector3(0.00211F, 0.27507F, -0.32266F), new Vector3(345.7222F, 87.9728F, 0.14917F), new Vector3(0.3042F, 0.3042F, 0.3042F));
                AddDisplayRule("MageBody", "Chest", new Vector3(-0.03274F, 0.06306F, -0.36407F), new Vector3(1.72429F, 90.30163F, 7.33441F), new Vector3(0.28249F, 0.28249F, 0.28249F));
                AddDisplayRule("MercBody", "HandL", new Vector3(-0.68466F, 0.13413F, -0.2637F), new Vector3(79.03943F, 64.85763F, 356.0091F), new Vector3(0.41224F, 0.41224F, 0.41224F));
                AddDisplayRule("TreebotBody", "FlowerBase", new Vector3(0.08616F, 1.29111F, 0F), new Vector3(0F, 90F, 0F), new Vector3(0.6832F, 0.6832F, 0.6832F));
                AddDisplayRule("LoaderBody", "MechBase", new Vector3(0F, 0.05332F, -0.18497F), new Vector3(0F, 90F, 353.159F), new Vector3(0.31267F, 0.31267F, 0.31267F));
                AddDisplayRule("CrocoBody", "Head", new Vector3(-0.34086F, 3.75624F, -0.42997F), new Vector3(79.52129F, 180F, 90F), new Vector3(3.09337F, 3.09337F, 3.09337F));
                AddDisplayRule("CaptainBody", "Chest", new Vector3(0F, 0.11369F, -0.3755F), new Vector3(0F, 90F, 0F), new Vector3(0.35782F, 0.35782F, 0.35782F));
                AddDisplayRule("RailgunnerBody", "Backpack", new Vector3(-0.00001F, -0.02334F, -0.17555F), new Vector3(0F, 90F, 0F), new Vector3(0.32071F, 0.32071F, 0.32071F));
                AddDisplayRule("VoidSurvivorBody", "Head", new Vector3(-0.00698F, -0.37717F, -0.14311F), new Vector3(5.70154F, 1.22189F, 180.3688F), new Vector3(0.34386F, 0.34386F, 0.34386F));
            };

            On.EntityStates.Interactables.MSObelisk.EndingGame.DoFinalAction += EndingGame_DoFinalAction;

            BulwarksHauntContent.Resources.entityStateTypes.Add(typeof(TransitionToGhostWave));

            SetUpSwordObj();

            On.RoR2.UI.LogBook.LogBookController.Awake += LogBookController_Awake;
            On.RoR2.UI.LogBook.LogBookController.CanSelectItemEntry += LogBookController_CanSelectItemEntry;
        }

        private void EndingGame_DoFinalAction(On.EntityStates.Interactables.MSObelisk.EndingGame.orig_DoFinalAction orig, EntityStates.Interactables.MSObelisk.EndingGame self)
        {
            if (Util.GetItemCountForTeam(TeamIndex.Player, itemDef.itemIndex, false) > 0 || Util.GetItemCountForTeam(TeamIndex.Player, BulwarksHauntContent.Items.BulwarksHaunt_SwordUnleashed.itemIndex, false) > 0)
            {
                self.outer.SetNextState(new TransitionToGhostWave());
                return;
            }
            orig(self);
        }

        public class TransitionToGhostWave : EntityStates.BaseState
        {
            public static float duration = 3f;

            public override void FixedUpdate()
            {
                base.FixedUpdate();
                if (NetworkServer.active && fixedAge >= duration)
                {
                    Stage.instance.BeginAdvanceStage(GhostWave.sceneDef);
                    outer.SetNextState(new EntityStates.Idle());
                }
            }
        }

        public void SetUpSwordObj()
        {
            Utils.CopyChildren(BulwarksHauntPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/Bulwark's Haunt/Sword/SwordObj.prefab"), swordObjPrefab);
            swordObjPrefab.AddComponent<GenericDisplayNameProvider>().displayToken = "BULWARKSHAUNT_SWORDOBJ_NAME";
            swordObjPrefab.AddComponent<BulwarksHauntSwordObjInteraction>();
            swordObjPrefab.transform.localScale *= 4f;

            var highlight = swordObjPrefab.AddComponent<Highlight>();
            highlight.targetRenderer = swordObjPrefab.GetComponentInChildren<Renderer>();
            highlight.highlightColor = Highlight.HighlightColor.interactive;

            var entityLocatorHolder = swordObjPrefab.transform.Find("EntityLocatorHolder").gameObject;
            entityLocatorHolder.layer = LayerIndex.pickups.intVal;
            var sphereCollider = entityLocatorHolder.AddComponent<SphereCollider>();
            sphereCollider.radius = 1.5f;
            sphereCollider.isTrigger = true;
            entityLocatorHolder.AddComponent<EntityLocator>().entity = swordObjPrefab;

            On.RoR2.SceneDirector.PopulateScene += SceneDirector_PopulateScene;
        }

        internal static bool reloadLogbook = false;
        private void LogBookController_Awake(On.RoR2.UI.LogBook.LogBookController.orig_Awake orig, RoR2.UI.LogBook.LogBookController self)
        {
            orig(self);
            if (reloadLogbook)
            {
                reloadLogbook = false;
                RoR2.UI.LogBook.LogBookController.BuildStaticData();
            }
        }

        private bool LogBookController_CanSelectItemEntry(On.RoR2.UI.LogBook.LogBookController.orig_CanSelectItemEntry orig, ItemDef itemDef, System.Collections.Generic.Dictionary<RoR2.ExpansionManagement.ExpansionDef, bool> expansionAvailability)
        {
            var result = orig(itemDef, expansionAvailability);
            var localUser = LocalUserManager.GetFirstLocalUser();
            if (localUser != null && localUser.userProfile != null)
            {
                var unlockableName = "BulwarksHaunt_SwordUnleashed";
                if (itemDef == BulwarksHauntContent.Items.BulwarksHaunt_Sword && localUser.userProfile.HasUnlockable(unlockableName) && swordCanBeUnleashed)
                    return false;
                if ((itemDef == BulwarksHauntContent.Items.BulwarksHaunt_SwordUnleashed && !localUser.userProfile.HasUnlockable(unlockableName) && swordCanBeUnleashed) || !swordCanBeUnleashed)
                    return false;
            }
            return result;
        }

        private void SceneDirector_PopulateScene(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self)
        {
            orig(self);
            if (SceneCatalog.GetSceneDefForCurrentScene().baseSceneName == "skymeadow")
            {
                GameObject obj = Object.Instantiate(swordObjPrefab, new Vector3(35.21518f, 5.219295f - 0.8f, 263.5193f), Quaternion.Euler(new Vector3(0f, 90f, 0f)));
                NetworkServer.Spawn(obj);
            }
        }

        public class BulwarksHauntSwordObjInteraction : NetworkBehaviour, IInteractable
        {
            public string contextString = "BULWARKSHAUNT_SWORDOBJ_CONTEXT";

            public string GetContextString(Interactor activator)
            {
                return Language.GetString(contextString);
            }

            public Interactability GetInteractability(Interactor activator)
            {
                return Interactability.Available;
            }

            public void OnInteractionBegin(Interactor activator)
            {
                Inventory inventory = activator.GetComponent<CharacterBody>().inventory;
                if (inventory)
                {
                    var item = BulwarksHauntContent.Items.BulwarksHaunt_Sword;

                    var networkUser = Util.LookUpBodyNetworkUser(activator.gameObject);
                    if (networkUser)
                    {
                        var unlockableName = "BulwarksHaunt_SwordUnleashed";
                        var localUser = networkUser.localUser;
                        if ((localUser != null && localUser.userProfile.HasUnlockable(unlockableName)) ||
                            networkUser.unlockables.Contains(UnlockableCatalog.GetUnlockableDef(unlockableName)))
                        {
                            if (swordCanBeUnleashed)
                                item = BulwarksHauntContent.Items.BulwarksHaunt_SwordUnleashed;
                        }
                    }

                    inventory.GiveItem(item);
                    GenericPickupController.SendPickupMessage(inventory.GetComponent<CharacterMaster>(), PickupCatalog.FindPickupIndex(item.itemIndex));

                    if (NetworkServer.active) NetworkServer.UnSpawn(gameObject);
                    Destroy(gameObject);
                }
            }

            public bool ShouldIgnoreSpherecastForInteractibility(Interactor activator)
            {
                return false;
            }

            public bool ShouldShowOnScanner()
            {
                return true;
            }
        }
    }
}
