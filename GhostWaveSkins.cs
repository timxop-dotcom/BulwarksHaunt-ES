using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BulwarksHaunt
{
    public static class GhostWaveSkins
    {
        public static void Init()
        {
            On.RoR2.SkinDef.Bake += SkinDef_Bake;

            var skinDef = CreateGhoulSkinForSurvivor(
                "Commando",
                Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoBody.prefab").WaitForCompletion(),
                Addressables.LoadAssetAsync<SkinDef>("RoR2/Base/Commando/skinCommandoDefault.asset").WaitForCompletion()
            );
            ReplaceRendererInfoOnSkin(
                skinDef, 0,
                new int[] { 0, 1, 2 },
                newTexture: BulwarksHauntPlugin.AssetBundle.LoadAsset<Texture>("Assets/Mods/Bulwark's Haunt/Skins/Commando/texCommandoPaletteDiffuseGhoul.png"),
                newEmissionColor: new Color32(169, 244, 161, 255)
            );

            skinDef = CreateGhoulSkinForSurvivor(
                "Huntress",
                Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/HuntressBody.prefab").WaitForCompletion(),
                Addressables.LoadAssetAsync<SkinDef>("RoR2/Base/Huntress/skinHuntressDefault.asset").WaitForCompletion()
            );
            ReplaceRendererInfoOnSkin(
                skinDef, 1,
                new int[] { 1, 10 },
                newTexture: BulwarksHauntPlugin.AssetBundle.LoadAsset<Texture>("Assets/Mods/Bulwark's Haunt/Skins/Huntress/texHuntressDiffuseGhoul.png"),
                newEmissionColor: new Color32(163, 226, 171, 255)
            );

            skinDef = CreateGhoulSkinForSurvivor(
                "Bandit2",
                Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2Body.prefab").WaitForCompletion(),
                Addressables.LoadAssetAsync<SkinDef>("RoR2/Base/Bandit2/skinBandit2Default.asset").WaitForCompletion()
            );
            ReplaceRendererInfoOnSkin(
                skinDef, 0,
                new int[] { 0, 1, 2 },
                newTexture: BulwarksHauntPlugin.AssetBundle.LoadAsset<Texture>("Assets/Mods/Bulwark's Haunt/Skins/Bandit2/texBandit2DiffuseGhoul.png"),
                newEmissionTexture: BulwarksHauntPlugin.AssetBundle.LoadAsset<Texture>("Assets/Mods/Bulwark's Haunt/Skins/Bandit2/texBandit2EmissionGhoul.png")
            );
            ReplaceRendererInfoOnSkin(
                skinDef, 3,
                new int[] { 3, 6 },
                newTexture: BulwarksHauntPlugin.AssetBundle.LoadAsset<Texture>("Assets/Mods/Bulwark's Haunt/Skins/Bandit2/texBanditCoatDiffuseGhoul.png")
            );

            skinDef = CreateGhoulSkinForSurvivor(
                "Toolbot",
                Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/ToolbotBody.prefab").WaitForCompletion(),
                Addressables.LoadAssetAsync<SkinDef>("RoR2/Base/Toolbot/skinToolbotDefault.asset").WaitForCompletion()
            );
            ReplaceRendererInfoOnSkin(
                skinDef, 1,
                new int[] { 1 },
                newTexture: BulwarksHauntPlugin.AssetBundle.LoadAsset<Texture>("Assets/Mods/Bulwark's Haunt/Skins/Toolbot/texTrimSheetConstruction2Ghoul.png")
            );

            skinDef = CreateGhoulSkinForSurvivor(
                "Engi",
                Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiBody.prefab").WaitForCompletion(),
                Addressables.LoadAssetAsync<SkinDef>("RoR2/Base/Engi/skinEngiDefault.asset").WaitForCompletion()
            );
            ReplaceRendererInfoOnSkin(
                skinDef, 2,
                new int[] { 2 },
                newTexture: BulwarksHauntPlugin.AssetBundle.LoadAsset<Texture>("Assets/Mods/Bulwark's Haunt/Skins/Engi/texEngiDiffuseGhoul.png"),
                newEmissionTexture: BulwarksHauntPlugin.AssetBundle.LoadAsset<Texture>("Assets/Mods/Bulwark's Haunt/Skins/Engi/texEngiEmissionGhoul.png")
            );

            skinDef = CreateGhoulSkinForSurvivor(
                "Mage",
                Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MageBody.prefab").WaitForCompletion(),
                Addressables.LoadAssetAsync<SkinDef>("RoR2/Base/Mage/skinMageDefault.asset").WaitForCompletion()
            );
            ReplaceRendererInfoOnSkin(
                skinDef, 2,
                new int[] { 2, 3 },
                newTexture: BulwarksHauntPlugin.AssetBundle.LoadAsset<Texture>("Assets/Mods/Bulwark's Haunt/Skins/Mage/texMageDiffuseGhoul.png")
            );

            skinDef = CreateGhoulSkinForSurvivor(
                "Merc",
                Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercBody.prefab").WaitForCompletion(),
                Addressables.LoadAssetAsync<SkinDef>("RoR2/Base/Merc/skinMercDefault.asset").WaitForCompletion()
            );
            ReplaceRendererInfoOnSkin(
                skinDef, 0,
                new int[] { 0 },
                newTexture: BulwarksHauntPlugin.AssetBundle.LoadAsset<Texture>("Assets/Mods/Bulwark's Haunt/Skins/Merc/texMercDiffuseGhoul.png")
            );

            skinDef = CreateGhoulSkinForSurvivor(
                "Treebot",
                Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Treebot/TreebotBody.prefab").WaitForCompletion(),
                Addressables.LoadAssetAsync<SkinDef>("RoR2/Base/Treebot/skinTreebotDefault.asset").WaitForCompletion()
            );
            ReplaceRendererInfoOnSkin(
                skinDef, 1,
                new int[] { 1 },
                newTexture: BulwarksHauntPlugin.AssetBundle.LoadAsset<Texture>("Assets/Mods/Bulwark's Haunt/Skins/Treebot/texTreebotFlowerDiffuseGhoul.png")
            );
            ReplaceRendererInfoOnSkin(
                skinDef, 0,
                new int[] { 0 },
                newEmissionColor: new Color32(177, 201, 120, 255)
            );

            skinDef = CreateGhoulSkinForSurvivor(
                "Loader",
                Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Loader/LoaderBody.prefab").WaitForCompletion(),
                Addressables.LoadAssetAsync<SkinDef>("RoR2/Base/Loader/skinLoaderDefault.asset").WaitForCompletion()
            );
            ReplaceRendererInfoOnSkin(
                skinDef, 1,
                new int[] { 1, 2 },
                newTexture: BulwarksHauntPlugin.AssetBundle.LoadAsset<Texture>("Assets/Mods/Bulwark's Haunt/Skins/Loader/texLoaderPilotDiffuseGhoul.png"),
                newEmissionColor: new Color32(155, 234, 110, 255)
            );

            skinDef = CreateGhoulSkinForSurvivor(
                "Croco",
                Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/CrocoBody.prefab").WaitForCompletion(),
                Addressables.LoadAssetAsync<SkinDef>("RoR2/Base/Croco/skinCrocoDefault.asset").WaitForCompletion()
            );
            ReplaceRendererInfoOnSkin(
                skinDef, 0,
                new int[] { 0 },
                newTexture: BulwarksHauntPlugin.AssetBundle.LoadAsset<Texture>("Assets/Mods/Bulwark's Haunt/Skins/Croco/texCrocoDiffuseGhoul.png")
            );

            skinDef = CreateGhoulSkinForSurvivor(
                "Captain",
                Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/CaptainBody.prefab").WaitForCompletion(),
                Addressables.LoadAssetAsync<SkinDef>("RoR2/Base/Captain/skinCaptainDefault.asset").WaitForCompletion()
            );
            ReplaceRendererInfoOnSkin(
                skinDef, 0,
                new int[] { 0, 1, 6 },
                newTexture: BulwarksHauntPlugin.AssetBundle.LoadAsset<Texture>("Assets/Mods/Bulwark's Haunt/Skins/Captain/texCaptainPaletteGhoul.png")
            );
            ReplaceRendererInfoOnSkin(
                skinDef, 2,
                new int[] { 2 },
                newTexture: BulwarksHauntPlugin.AssetBundle.LoadAsset<Texture>("Assets/Mods/Bulwark's Haunt/Skins/Captain/texCaptainPaletteGhoul.png"),
                newEmissionColor: new Color32(170, 226, 124, 255)
            );
            ReplaceRendererInfoOnSkin(
                skinDef, 3,
                new int[] { 3 },
                newTexture: BulwarksHauntPlugin.AssetBundle.LoadAsset<Texture>("Assets/Mods/Bulwark's Haunt/Skins/Captain/texCaptainJacketDiffuseGhoul.png")
            );
            ReplaceRendererInfoOnSkin(
                skinDef, 4,
                new int[] { 4, 5 },
                newEmissionColor: new Color32(170, 226, 124, 255)
            );

            skinDef = CreateGhoulSkinForSurvivor(
                "RailGunner",
                Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerBody.prefab").WaitForCompletion(),
                Addressables.LoadAssetAsync<SkinDef>("RoR2/DLC1/Railgunner/skinRailGunnerDefault.asset").WaitForCompletion()
            );
            ReplaceRendererInfoOnSkin(
                skinDef, 2,
                new int[] { 2 },
                newTexture: BulwarksHauntPlugin.AssetBundle.LoadAsset<Texture>("Assets/Mods/Bulwark's Haunt/Skins/Railgunner/texRailGunnerDiffuseGhoul.png")
            );
            ReplaceRendererInfoOnSkin(
                skinDef, 3,
                new int[] { 3 },
                newEmissionColor: new Color32(208, 224, 209, 255)
            );

            skinDef = CreateGhoulSkinForSurvivor(
                "VoidSurvivor",
                Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorBody.prefab").WaitForCompletion(),
                Addressables.LoadAssetAsync<SkinDef>("RoR2/DLC1/VoidSurvivor/skinVoidSurvivorDefault.asset").WaitForCompletion()
            );
            ReplaceRendererInfoOnSkin(
                skinDef, 0,
                new int[] { 0 },
                newTexture: BulwarksHauntPlugin.AssetBundle.LoadAsset<Texture>("Assets/Mods/Bulwark's Haunt/Skins/VoidSurvivor/texVoidSurvivorFleshDiffuseGhoul.png"),
                newEmissionTexture: BulwarksHauntPlugin.AssetBundle.LoadAsset<Texture>("Assets/Mods/Bulwark's Haunt/Skins/VoidSurvivor/texVoidSurvivorFleshEmissionGhoul.png")
            );
            ReplaceRendererInfoOnSkin(
                skinDef, 1,
                new int[] { 1 },
                newTexture: BulwarksHauntPlugin.AssetBundle.LoadAsset<Texture>("Assets/Mods/Bulwark's Haunt/Skins/VoidSurvivor/texVoidSurvivorFleshDiffuseGhoul.png"),
                newEmissionTexture: BulwarksHauntPlugin.AssetBundle.LoadAsset<Texture>("Assets/Mods/Bulwark's Haunt/Skins/VoidSurvivor/texVoidSurvivorFleshEmissionGhoul.png")
            );
        }

        private static bool cancelBake = false;
        private static void SkinDef_Bake(On.RoR2.SkinDef.orig_Bake orig, SkinDef self)
        {
            if (!cancelBake) orig(self);
        }

        public static SkinDef CreateGhoulSkinForSurvivor(string charName, GameObject bodyPrefab, SkinDef baseSkinDef)
        {
            cancelBake = true;
            var skinDef = ScriptableObject.CreateInstance<SkinDef>();
            cancelBake = false;
            skinDef.name = "skin" + charName + "BulwarksHauntAlt";
            skinDef.baseSkins = new SkinDef[]
            {
                baseSkinDef
            };
            skinDef.icon = BulwarksHauntPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/Bulwark's Haunt/Skins/texGenericGhoulSkinIcon.png");
            skinDef.nameToken = "GENERIC_SKIN_BULWARKSHAUNT_ALT_NAME";
            skinDef.unlockableDef = ScriptableObject.CreateInstance<UnlockableDef>();
            skinDef.unlockableDef.cachedName = "Skins." + charName + ".BulwarksHaunt_Alt";
            skinDef.unlockableDef.nameToken = "GENERIC_SKIN_BULWARKSHAUNT_ALT_NAME";
            skinDef.unlockableDef.achievementIcon = skinDef.icon;
            BulwarksHauntContent.Resources.unlockableDefs.Add(skinDef.unlockableDef);
            skinDef.rootObject = baseSkinDef.rootObject;
            skinDef.rendererInfos = new CharacterModel.RendererInfo[] { };
            foreach (var rendererInfo in baseSkinDef.rendererInfos)
            {
                var newRendererInfo = new CharacterModel.RendererInfo()
                {
                    renderer = rendererInfo.renderer,
                    defaultMaterial = rendererInfo.defaultMaterial,
                    defaultShadowCastingMode = rendererInfo.defaultShadowCastingMode,
                    ignoreOverlays = rendererInfo.ignoreOverlays
                };
                HG.ArrayUtils.ArrayAppend(ref skinDef.rendererInfos, newRendererInfo);
            }

            var modelLocator = bodyPrefab.GetComponent<ModelLocator>();
            if (modelLocator)
            {
                var modelTransform = modelLocator.modelTransform.gameObject;
                if (modelTransform)
                {
                    var modelSkinController = modelTransform.GetComponent<ModelSkinController>();
                    if (modelSkinController)
                    {
                        HG.ArrayUtils.ArrayAppend(ref modelSkinController.skins, in skinDef);
                    }
                }
            }

            return skinDef;
        }

        public static void ReplaceRendererInfoOnSkin(SkinDef skinDef, int origTextureRendererInfoIndex, int[] rendererInfoIndices, Texture newTexture = null, Color newEmissionColor = default(Color), Texture newEmissionTexture = null)
        {
            var mat = Material.Instantiate(skinDef.rendererInfos[origTextureRendererInfoIndex].defaultMaterial);
            if (newTexture != null) mat.SetTexture("_MainTex", newTexture);
            if (!Color.Equals(newEmissionColor, default(Color)))
            {
                mat.SetColor("_EmColor", newEmissionColor);
                mat.SetColor("_EmissionColor", newEmissionColor);
            }
            if (newEmissionTexture != null) mat.SetTexture("_EmTex", newEmissionTexture);
            foreach (var rendererInfoIndex in rendererInfoIndices)
            {
                skinDef.rendererInfos[rendererInfoIndex] = new CharacterModel.RendererInfo
                {
                    renderer = skinDef.rendererInfos[rendererInfoIndex].renderer,
                    defaultMaterial = mat,
                    defaultShadowCastingMode = skinDef.rendererInfos[rendererInfoIndex].defaultShadowCastingMode,
                    ignoreOverlays = skinDef.rendererInfos[rendererInfoIndex].ignoreOverlays
                };
            }
        }
    }
}