using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using RoR2;
using UnityEngine;
using R2API;

namespace BulwarksHaunt.Items
{
    public class GhostFury : BaseItem
    {
        public override void OnLoad()
        {
            base.OnLoad();
            itemDef.name = "BulwarksHaunt_GhostFury";
            SetItemTierWhenAvailable(ItemTier.NoTier);
            itemDef.tags = new ItemTag[] { };
            itemDef.canRemove = false;
            itemDef.hidden = true;

            glassMaterial = BulwarksHauntPlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/Bulwark's Haunt/GhostFury/matGhostFuryGlass.mat");
            overlayMaterial = BulwarksHauntPlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/Bulwark's Haunt/GhostFury/matGhostFuryOverlay.mat");
            CharacterModelMaterialOverrides.AddOverride("BulwarksHaunt_GhostFury", GlassMaterialOverride);
            On.RoR2.CharacterModel.UpdateOverlays += CharacterModel_UpdateOverlays;

            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.Util.GetBestBodyName += Util_GetBestBodyName;
        }

        private string Util_GetBestBodyName(On.RoR2.Util.orig_GetBestBodyName orig, GameObject bodyObject)
        {
            var result = orig(bodyObject);
            if (bodyObject)
            {
                var characterBody = bodyObject.GetComponent<CharacterBody>();
                if (characterBody && characterBody.inventory && characterBody.inventory.GetItemCount(itemDef) > 0)
                {
                    result = Language.GetStringFormatted("BODY_MODIFIER_BULWARKSHAUNT_GHOSTFURY", result);
                }
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
                    args.baseHealthAdd += 160f * (1f + 0.3f * (sender.level - 1f));
                    args.armorAdd += 40f;
                    args.attackSpeedMultAdd += 0.1f;
                    args.moveSpeedMultAdd += 0.25f;
                    args.cooldownReductionAdd += 0.2f;
                }
            }
        }

        private void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            var itemCount = self.inventory.GetItemCount(itemDef);
            if (self.inventory.GetItemCount(RoR2Content.Items.Ghost) > 0) itemCount = 0;
            self.AddItemBehavior<BulwarksHauntGhostFuryController>(itemCount);
        }

        public class BulwarksHauntGhostFuryController : CharacterBody.ItemBehavior
        {
            public CharacterModel model;

            public void Start()
            {
                if (body && body.modelLocator && body.modelLocator.modelTransform)
                {
                    model = body.modelLocator.modelTransform.GetComponent<CharacterModel>();
                    if (model)
                    {
                        var matHelper = model.GetComponent<BulwarksHauntGhostFuryModelMaterialHelper>();
                        if (!matHelper) matHelper = model.gameObject.AddComponent<BulwarksHauntGhostFuryModelMaterialHelper>();
                        matHelper.matActive = true;
                        CharacterModelMaterialOverrides.SetOverrideActive(model, "BulwarksHaunt_GhostFury", true);
                    }
                }
            }

            public void OnDestroy()
            {
                if (model && body && body.inventory && body.inventory.GetItemCount(BulwarksHauntContent.Items.BulwarksHaunt_GhostFury) <= 0)
                {
                    var matHelper = model.GetComponent<BulwarksHauntGhostFuryModelMaterialHelper>();
                    if (matHelper)
                        matHelper.matActive = false;
                    CharacterModelMaterialOverrides.SetOverrideActive(model, "BulwarksHaunt_GhostFury", false);
                }
            }
        }

        public class BulwarksHauntGhostFuryModelMaterialHelper : MonoBehaviour
        {
            public CharacterModel model;
            public bool matActive = false;

            public void Awake()
            {
                model = GetComponent<CharacterModel>();
            }
        }

        public static Material glassMaterial;
        public static Material overlayMaterial;

        public void GlassMaterialOverride(CharacterModel characterModel, ref Material material, ref bool ignoreOverlays)
        {
            if (characterModel.visibility >= VisibilityLevel.Visible && !ignoreOverlays)
            {
                material = glassMaterial;
            }
        }

        private void CharacterModel_UpdateOverlays(On.RoR2.CharacterModel.orig_UpdateOverlays orig, CharacterModel self)
        {
            orig(self);
            if (self.visibility < VisibilityLevel.Visible || self.activeOverlayCount >= CharacterModel.maxOverlays) return;
            var matHelper = self.GetComponent<BulwarksHauntGhostFuryModelMaterialHelper>();
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
