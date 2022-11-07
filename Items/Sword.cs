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
            /*
            onSetupIDRS += () =>
            {
                AddDisplayRule("CommandoBody", "Head", new Vector3(0.09206F, 0.33178F, -0.09136F), new Vector3(16.03024F, 63.62674F, 304.511F), new Vector3(0.02301F, 0.02301F, 0.02301F));
                AddDisplayRule("HuntressBody", "Head", new Vector3(0.01367F, 0.21655F, -0.17495F), new Vector3(357.0015F, 65.06252F, 277.7556F), new Vector3(0.02242F, 0.02242F, 0.02242F));
                AddDisplayRule("Bandit2Body", "Head", new Vector3(0.05274F, 0.09501F, -0.10753F), new Vector3(350.0532F, 63.19105F, 297.9137F), new Vector3(0.02805F, 0.02805F, 0.02805F));
                AddDisplayRule("ToolbotBody", "Head", new Vector3(-1.5345F, 1.70927F, 2.3488F), new Vector3(25.30116F, 73.93195F, 115.0976F), new Vector3(0.31308F, 0.31308F, 0.31308F));
                AddDisplayRule("EngiBody", "HeadCenter", new Vector3(0.05794F, 0.12538F, -0.14704F), new Vector3(0.49978F, 72.40844F, 310.3805F), new Vector3(0.03243F, 0.03243F, 0.03243F));
                AddDisplayRule("MageBody", "Head", new Vector3(0.06019F, 0.16738F, -0.14399F), new Vector3(343.2434F, 235.5832F, 58.464F), new Vector3(0.02055F, 0.02055F, 0.02055F));
                AddDisplayRule("MercBody", "Head", new Vector3(0.09912F, 0.20856F, -0.03218F), new Vector3(346.0291F, 221.2525F, 67.84089F), new Vector3(0.02243F, 0.02243F, 0.02243F));
                AddDisplayRule("TreebotBody", "MIAntennae4", new Vector3(-0.00461F, 0.14457F, 0.00345F), new Vector3(-0.00002F, 36.79062F, 272.2785F), new Vector3(0.0496F, 0.0496F, 0.0496F));
                AddDisplayRule("LoaderBody", "Head", new Vector3(0.07075F, 0.22212F, -0.05101F), new Vector3(16.6799F, 61.79085F, 306.0373F), new Vector3(0.02181F, 0.02181F, 0.02181F));
                AddDisplayRule("CrocoBody", "Head", new Vector3(1.45411F, 0.59183F, 1.08829F), new Vector3(56.441F, 150.6051F, 281.6682F), new Vector3(0.33415F, 0.59025F, 0.33415F));
                AddDisplayRule("CaptainBody", "Chest", new Vector3(0.00136F, 0.37981F, 0.1799F), new Vector3(350.816F, 280.0829F, 276.5578F), new Vector3(0.02509F, 0.02509F, 0.02509F));
                AddDisplayRule("RailgunnerBody", "Head", new Vector3(0.05764F, 0.1407F, -0.13382F), new Vector3(355.0823F, 235.3653F, 70.07189F), new Vector3(0.01869F, 0.01869F, 0.01869F));
                AddDisplayRule("VoidSurvivorBody", "Neck", new Vector3(-0.00094F, 0.24878F, -0.20333F), new Vector3(3.48902F, 270F, 270F), new Vector3(0.02107F, 0.02107F, 0.02107F));
            };
            */

            On.EntityStates.Interactables.MSObelisk.EndingGame.DoFinalAction += EndingGame_DoFinalAction;

            BulwarksHauntContent.Resources.entityStateTypes.Add(typeof(TransitionToGhostWave));

            SetUpSwordObj();
        }

        private void EndingGame_DoFinalAction(On.EntityStates.Interactables.MSObelisk.EndingGame.orig_DoFinalAction orig, EntityStates.Interactables.MSObelisk.EndingGame self)
        {
            if (Util.GetItemCountForTeam(TeamIndex.Player, itemDef.itemIndex, false) > 0)
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
                    inventory.GiveItem(BulwarksHauntContent.Items.BulwarksHaunt_Sword);
                    GenericPickupController.SendPickupMessage(inventory.GetComponent<CharacterMaster>(), PickupCatalog.FindPickupIndex(BulwarksHauntContent.Items.BulwarksHaunt_Sword.itemIndex));

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
