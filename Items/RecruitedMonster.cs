using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using RoR2;
using UnityEngine;
using R2API;

namespace BulwarksHaunt.Items
{
    public class RecruitedMonster : BaseItem
    {
        public override void OnLoad()
        {
            base.OnLoad();
            itemDef.name = "BulwarksHaunt_RecruitedMonster";
            SetItemTierWhenAvailable(ItemTier.NoTier);
            itemDef.tags = new ItemTag[] { };
            itemDef.canRemove = false;
            itemDef.hidden = true;

            overlayMaterial = BulwarksHauntPlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/Bulwark's Haunt/GhostFury/matGhostFuryOverlay.mat");
            On.RoR2.CharacterModel.UpdateOverlays += CharacterModel_UpdateOverlays;

            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.Util.GetBestBodyName += Util_GetBestBodyName;
            On.RoR2.HealthComponent.Heal += HealthComponent_Heal;
        }

        private float HealthComponent_Heal(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, float amount, ProcChainMask procChainMask, bool nonRegen)
        {
            if (self.body && self.body.inventory && self.body.inventory.GetItemCount(itemDef) > 0) return 0;
            return orig(self, amount, procChainMask, nonRegen);
        }

        private string Util_GetBestBodyName(On.RoR2.Util.orig_GetBestBodyName orig, GameObject bodyObject)
        {
            var result = orig(bodyObject);
            CharacterBody characterBody = bodyObject?.GetComponent<CharacterBody>();
            if (characterBody && characterBody.inventory && characterBody.inventory.GetItemCount(itemDef) > 0)
            {
                result = Language.GetStringFormatted("BODY_MODIFIER_BULWARKSHAUNT_RECRUITED", result);
            }
            return result;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.inventory)
            {
                var itemCount = sender.inventory.GetItemCount(itemDef);
                if (itemCount > 0)
                {
                    args.armorAdd += 40f;
                    args.damageMultAdd += 3f;
                    args.attackSpeedMultAdd += 0.5f;
                    args.moveSpeedMultAdd += 0.25f;
                    args.cooldownReductionAdd += 0.2f;
                }
            }
        }

        private void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            var itemCount = self.inventory.GetItemCount(itemDef);
            self.AddItemBehavior<BulwarksHauntRecruitedMonsterOverlayController>(itemCount);
        }

        public class BulwarksHauntRecruitedMonsterOverlayController : CharacterBody.ItemBehavior
        {
            public CharacterModel model;

            public void Start()
            {
                model = body.modelLocator?.modelTransform?.GetComponent<CharacterModel>();
                if (model)
                {
                    var matHelper = model.GetComponent<BulwarksHauntRecruitedMonsterModelMaterialHelper>();
                    if (!matHelper) matHelper = model.gameObject.AddComponent<BulwarksHauntRecruitedMonsterModelMaterialHelper>();
                    matHelper.matActive = true;
                }
            }

            public void OnDestroy()
            {
                if (model && body && body.inventory && body.inventory.GetItemCount(BulwarksHauntContent.Items.BulwarksHaunt_GhostFury) <= 0)
                {
                    var matHelper = model.GetComponent<BulwarksHauntRecruitedMonsterModelMaterialHelper>();
                    if (matHelper)
                        matHelper.matActive = false;
                }
            }
        }

        public class BulwarksHauntRecruitedMonsterModelMaterialHelper : MonoBehaviour
        {
            public CharacterModel model;
            public bool matActive = false;

            public void Awake()
            {
                model = GetComponent<CharacterModel>();
            }
        }

        public static Material overlayMaterial;
        private void CharacterModel_UpdateOverlays(On.RoR2.CharacterModel.orig_UpdateOverlays orig, CharacterModel self)
        {
            orig(self);
            if (self.visibility < VisibilityLevel.Visible || self.activeOverlayCount >= CharacterModel.maxOverlays) return;
            var matHelper = self.GetComponent<BulwarksHauntRecruitedMonsterModelMaterialHelper>();
            if (matHelper && matHelper.matActive)
            {
                Material[] array = self.currentOverlays;
                int num = self.activeOverlayCount;
                self.activeOverlayCount = num + 1;
                array[num] = overlayMaterial;
            }
        }
    }
}
