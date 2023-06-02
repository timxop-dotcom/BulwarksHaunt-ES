using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using static BulwarksHaunt.GhostWave;

namespace BulwarksHaunt
{
    [CreateAssetMenu(fileName = "NewWaveModifier.asset", menuName = "Bulwark's Haunt/Wave Modifier")]
    public class BulwarksHauntWaveModifier : ScriptableObject
    {
        private string _cachedName;
        public string cachedName
        {
            get { return _cachedName; }
            set
            {
                _cachedName = value;
                name = value;
            }
        }
        public string nameToken;
        public string popupToken;
        public string descriptionToken;

        [Header("Auto-assigned at runtime")]
        public bool active = false;
        public int index = -1;

        public delegate void WaveModifierEventHandler(GhostWaveControllerBaseState.MonsterWaves waveState = null);
        public WaveModifierEventHandler onActivated;
        public WaveModifierEventHandler onUpdate;
        public WaveModifierEventHandler onFixedUpdate;
        public WaveModifierEventHandler onDeactivated;

        [ContextMenu("Auto Populate Tokens")]
        public void AutoPopulateTokens()
        {
            nameToken = "BULWARKSHAUNT_GHOSTWAVE_MODIFIER_" + name.ToUpperInvariant() + "_NAME";
            popupToken = "BULWARKSHAUNT_GHOSTWAVE_MODIFIER_" + name.ToUpperInvariant() + "_POPUP";
            descriptionToken = "BULWARKSHAUNT_GHOSTWAVE_MODIFIER_" + name.ToUpperInvariant() + "_DESCRIPTION";
        }

        public void Activate(GhostWaveControllerBaseState.MonsterWaves waveState = null)
        {
            if (active) return;

            active = true;
            WaveModifierCatalog.activeWaveModifiers.Add(cachedName);

            if (Run.instance)
            {
                foreach (var pcmc in PlayerCharacterMasterController.instances)
                {
                    if (!pcmc.master) continue;

                    var notificationQueueHandler = CharacterMasterNotificationQueue.GetNotificationQueueForMaster(pcmc.master);
                    if (notificationQueueHandler)
                    {
                        var info = new CharacterMasterNotificationQueue.NotificationInfo(this);
                        notificationQueueHandler.PushNotification(info, 10f);
                    }
                }
            }

            Chat.AddMessage(new Chat.SimpleChatMessage
            {
                baseToken = "BULWARKSHAUNT_GHOSTWAVE_SYSTEM_WAVEMODIFIER",
                paramTokens = new string[]
                {
                    Language.currentLanguage.GetLocalizedStringByToken(nameToken)
                }
            });

            if (NetworkServer.active)
            {
                new SyncWaveModifierActive(index, true).Send(NetworkDestination.Clients);
            }

            if (onActivated != null) onActivated(waveState);
        }

        public void Deactivate(GhostWaveControllerBaseState.MonsterWaves waveState = null)
        {
            if (!active) return;

            active = false;
            WaveModifierCatalog.activeWaveModifiers.Remove(cachedName);

            if (NetworkServer.active)
            {
                new SyncWaveModifierActive(index, false).Send(NetworkDestination.Clients);
            }

            if (onDeactivated != null) onDeactivated(waveState);
        }

        public class SyncWaveModifierActive : INetMessage
        {
            int modifierIndex;
            bool active;

            public SyncWaveModifierActive()
            {
            }

            public SyncWaveModifierActive(int modifierIndex, bool active)
            {
                this.modifierIndex = modifierIndex;
                this.active = active;
            }

            public void Deserialize(NetworkReader reader)
            {
                modifierIndex = reader.ReadInt32();
                active = reader.ReadBoolean();
            }

            public void OnReceived()
            {
                if (NetworkServer.active) return;

                var modifier = WaveModifierCatalog.GetWaveModifier(modifierIndex);
                if (!modifier || active == modifier.active) return;

                if (active) modifier.Activate();
                else modifier.Deactivate();
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(modifierIndex);
                writer.Write(active);
            }
        }
    }
}