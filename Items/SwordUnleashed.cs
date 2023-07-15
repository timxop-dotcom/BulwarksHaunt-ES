using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using RoR2;
using UnityEngine;
using R2API;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using RoR2.CharacterAI;
using System.Linq;

namespace BulwarksHaunt.Items
{
    public class SwordUnleashed : BaseItem
    {
        public static GameObject useEffect;

        public static ConfigOptions.ConfigurableValue<float> radius = ConfigOptions.ConfigurableValue.CreateFloat(
            BulwarksHauntPlugin.PluginGUID,
            BulwarksHauntPlugin.PluginName,
            BulwarksHauntPlugin.config,
            "Unleashed Blade",
            "Radius",
            25f,
            description: "How large should the radius of the recruitment effect be (in meters)",
            stringsToAffect: new System.Collections.Generic.List<string>()
            {
                "ITEM_BULWARKSHAUNT_SWORD_UNLEASHED_DESC"
            }
        );
        public static ConfigOptions.ConfigurableValue<float> radiusPerStack = ConfigOptions.ConfigurableValue.CreateFloat(
            BulwarksHauntPlugin.PluginGUID,
            BulwarksHauntPlugin.PluginName,
            BulwarksHauntPlugin.config,
            "Unleashed Blade",
            "RadiusPerStack",
            5f,
            description: "How large should the radius of the recruitment effect be for each additional stack of this item (in meters)",
            stringsToAffect: new System.Collections.Generic.List<string>()
            {
                "ITEM_BULWARKSHAUNT_SWORD_UNLEASHED_DESC"
            }
        );
        public static ConfigOptions.ConfigurableValue<float> duration = ConfigOptions.ConfigurableValue.CreateFloat(
            BulwarksHauntPlugin.PluginGUID,
            BulwarksHauntPlugin.PluginName,
            BulwarksHauntPlugin.config,
            "Unleashed Blade",
            "Duration",
            25f,
            description: "How long should the recruitment effect be (in seconds)",
            stringsToAffect: new System.Collections.Generic.List<string>()
            {
                "ITEM_BULWARKSHAUNT_SWORD_UNLEASHED_DESC"
            }
        );
        public static ConfigOptions.ConfigurableValue<float> durationPerStack = ConfigOptions.ConfigurableValue.CreateFloat(
            BulwarksHauntPlugin.PluginGUID,
            BulwarksHauntPlugin.PluginName,
            BulwarksHauntPlugin.config,
            "Unleashed Blade",
            "DurationPerStack",
            5f,
            description: "How long should the recruitment effect be for each additional stack of this item (in seconds)",
            stringsToAffect: new System.Collections.Generic.List<string>()
            {
                "ITEM_BULWARKSHAUNT_SWORD_UNLEASHED_DESC"
            }
        );

        public override void OnLoad()
        {
            base.OnLoad();
            itemDef.name = "BulwarksHaunt_SwordUnleashed";
            SetItemTierWhenAvailable(ItemTier.Boss);
            itemDef.tags = new ItemTag[] {
                ItemTag.CannotCopy,
                ItemTag.CannotDuplicate,
                ItemTag.ObliterationRelated,
                ItemTag.WorldUnique,
                ItemTag.Utility
            };
            itemDef.pickupModelPrefab = PrepareModel(BulwarksHauntPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/Bulwark's Haunt/Sword/ItemModel.prefab"));
            itemDef.pickupIconSprite = BulwarksHauntPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/Bulwark's Haunt/Sword/Icon.png");

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

            var unlockableDef = ScriptableObject.CreateInstance<UnlockableDef>();
            unlockableDef.cachedName = "BulwarksHaunt_SwordUnleashed";
            unlockableDef.nameToken = "ITEM_BULWARKSHAUNT_SWORD_NAME";

            unlockableDef.achievementIcon = MysticsRisky2Utils.Utils.CreateItemIconWithBackgroundFromItem(itemDef);

            BulwarksHauntContent.Resources.unlockableDefs.Add(unlockableDef);

            On.RoR2.EquipmentSlot.OnEquipmentExecuted += EquipmentSlot_OnEquipmentExecuted; ;
        }

        private void EquipmentSlot_OnEquipmentExecuted(On.RoR2.EquipmentSlot.orig_OnEquipmentExecuted orig, EquipmentSlot self)
        {
            orig(self);
            if (NetworkServer.active && self.inventory && self.teamComponent)
            {
                var itemCount = self.inventory.GetItemCount(itemDef);
                if (itemCount > 0)
                {
                    var currentPosition = self.GetAimRay().origin;
                    var myTeamIndex = self.teamComponent.teamIndex;
                    var currentRadius = radius + radiusPerStack * (float)(itemCount - 1);
                    var currentRadiusSqr = currentRadius * currentRadius;

                    if (useEffect)
                    {
                        EffectManager.SpawnEffect(useEffect, new EffectData
                        {
                            origin = currentPosition,
                            scale = currentRadius
                        }, false);
                    }

                    for (TeamIndex teamIndex = TeamIndex.Neutral; teamIndex < TeamIndex.Count; teamIndex++)
                    {
                        if (teamIndex != myTeamIndex)
                        {
                            var teamMembers = TeamComponent.GetTeamMembers(teamIndex).ToList();
                            foreach (TeamComponent teamComponent in teamMembers)
                            {
                                Vector3 vector = teamComponent.transform.position - currentPosition;
                                if (vector.sqrMagnitude <= currentRadiusSqr)
                                {
                                    CharacterBody body = teamComponent.GetComponent<CharacterBody>();
                                    if (body && !body.isBoss && body.master)
                                    {
                                        var hijackHelper = body.GetComponent<BulwarksHauntSwordTeamHijack>();
                                        if (hijackHelper == null)
                                        {
                                            hijackHelper = body.gameObject.AddComponent<BulwarksHauntSwordTeamHijack>();
                                            hijackHelper.oldTeamIndex = teamIndex;
                                            if (body.inventory) body.inventory.GiveItem(BulwarksHauntContent.Items.BulwarksHaunt_RecruitedMonster);
                                        }
                                        hijackHelper.timer = Mathf.Max(hijackHelper.timer, duration + durationPerStack * (float)(itemCount - 1));
                                        hijackHelper.SetTeam(myTeamIndex);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void AfterContentPackLoaded()
        {
            base.AfterContentPackLoaded();
            itemDef.nameToken = "ITEM_BULWARKSHAUNT_SWORD_NAME";
            itemDef.pickupToken = "ITEM_BULWARKSHAUNT_SWORD_UNLEASHED_PICKUP";
            itemDef.descriptionToken = "ITEM_BULWARKSHAUNT_SWORD_UNLEASHED_DESC";
            itemDef.loreToken = "ITEM_BULWARKSHAUNT_SWORD_LORE";
        }

        public class BulwarksHauntSwordTeamHijack : MonoBehaviour, ILifeBehavior
        {
            public TeamIndex oldTeamIndex;
            public CharacterBody body;
            public float timer = 5f;

            public void Awake()
            {
                body = GetComponent<CharacterBody>();
            }

            public void FixedUpdate()
            {
                timer -= Time.fixedDeltaTime;
                if (timer <= 0f)
                {
                    Undo();
                    Destroy(this);
                }
            }

            public void OnDeathStart()
            {
                Undo();
            }

            public void Undo()
            {
                SetTeam(oldTeamIndex);
                if (body.inventory) body.inventory.RemoveItem(BulwarksHauntContent.Items.BulwarksHaunt_RecruitedMonster);
            }

            public void SetTeam(TeamIndex teamIndex)
            {
                if (body.teamComponent.teamIndex == teamIndex) return;

                if (body.teamComponent.indicator)
                {
                    UnityEngine.Object.Destroy(body.teamComponent.indicator);
                    body.teamComponent.indicator = null;
                }

                var master = body.master;
                if (master) master.teamIndex = teamIndex;
                body.teamComponent.teamIndex = teamIndex;
                if (master)
                {
                    BaseAI ai = master.GetComponent<BaseAI>();
                    if (ai)
                    {
                        ai.enemyAttention = 0.1f;
                        ai.targetRefreshTimer = 0.1f;
                        ai.skillDriverUpdateTimer = 0.1f;
                        ai.currentEnemy.Reset();
                    }
                }
            }
        }
    }
}
