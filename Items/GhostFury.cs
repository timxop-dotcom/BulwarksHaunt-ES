using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using RoR2;
using UnityEngine;
using R2API;

namespace BulwarksHaunt.Items
{
    public class GhostFury : BaseItem
    {
        public static ConfigOptions.ConfigurableValue<float> baseHealth = ConfigOptions.ConfigurableValue.CreateFloat(
            BulwarksHauntPlugin.PluginGUID,
            BulwarksHauntPlugin.PluginName,
            BulwarksHauntPlugin.config,
            "Ghost Bonus Stats",
            "Base Health",
            160f,
            description: "How much flat HP should ghosts get"
        );
        public static ConfigOptions.ConfigurableValue<float> baseHealthPerLevel = ConfigOptions.ConfigurableValue.CreateFloat(
            BulwarksHauntPlugin.PluginGUID,
            BulwarksHauntPlugin.PluginName,
            BulwarksHauntPlugin.config,
            "Ghost Bonus Stats",
            "Base Health Per Level",
            48f,
            description: "How much flat HP should ghosts get for each level"
        );
        public static ConfigOptions.ConfigurableValue<float> armor = ConfigOptions.ConfigurableValue.CreateFloat(
            BulwarksHauntPlugin.PluginGUID,
            BulwarksHauntPlugin.PluginName,
            BulwarksHauntPlugin.config,
            "Ghost Bonus Stats",
            "Armor",
            20f,
            description: "How much armor should ghosts get"
        );
        public static ConfigOptions.ConfigurableValue<float> attackSpeed = ConfigOptions.ConfigurableValue.CreateFloat(
            BulwarksHauntPlugin.PluginGUID,
            BulwarksHauntPlugin.PluginName,
            BulwarksHauntPlugin.config,
            "Ghost Bonus Stats",
            "Attack Speed",
            10f,
            description: "How much attack speed should ghosts get (in %)"
        );
        public static ConfigOptions.ConfigurableValue<float> moveSpeed = ConfigOptions.ConfigurableValue.CreateFloat(
            BulwarksHauntPlugin.PluginGUID,
            BulwarksHauntPlugin.PluginName,
            BulwarksHauntPlugin.config,
            "Ghost Bonus Stats",
            "Move Speed",
            25f,
            description: "How much movement speed should ghosts get (in %)"
        );
        public static ConfigOptions.ConfigurableValue<float> cooldownReduction = ConfigOptions.ConfigurableValue.CreateFloat(
            BulwarksHauntPlugin.PluginGUID,
            BulwarksHauntPlugin.PluginName,
            BulwarksHauntPlugin.config,
            "Ghost Bonus Stats",
            "Cooldown Reduction",
            20f,
            description: "How much skill cooldown reduction should ghosts get (in %)"
        );

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
                    args.baseHealthAdd += baseHealth + baseHealthPerLevel * (sender.level - 1f);
                    args.armorAdd += armor;
                    args.attackSpeedMultAdd += attackSpeed / 100f;
                    args.moveSpeedMultAdd += moveSpeed / 100f;
                    args.cooldownReductionAdd += cooldownReduction / 100f;
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
