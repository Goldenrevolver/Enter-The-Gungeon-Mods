using UnityEngine;

namespace CuttingRoomFloor
{
    internal class CustomSynergyHandRemover : MonoBehaviour
    {
        public CustomSynergyType SynergyToCheck;

        private Gun m_gun;

        protected void Awake()
        {
            this.m_gun = base.GetComponent<Gun>();
        }

        // calling this only On Enable / On Gun Change doesn't work due to how the dual wield logic is set up
        protected void Update()
        {
            if (m_gun && m_gun.CurrentOwner is PlayerController player && player.CurrentGun == m_gun)
            {
                m_gun.additionalHandState = player.HasActiveBonusSynergy(SynergyToCheck) ? AdditionalHandState.HideBoth : AdditionalHandState.None;

                // only check every 8 frames (check without modulo to be faster)
                if ((Time.frameCount & 7) == 0)
                {
                    // this is not a very expensive operation, so we can afford to do it every 8 frames
                    player.ProcessHandAttachment();
                }
            }
        }

        protected void OnDisable()
        {
            if (m_gun && m_gun.CurrentOwner is PlayerController player && player.CurrentGun == m_gun)
            {
                // basically call PlayerController.ToggleHandRenderers without actually calling it

                var m_hideHandRenderers = Tools.GetFieldValue<OverridableBool>(typeof(PlayerController), "m_hideHandRenderers", player);
                bool flag = !m_hideHandRenderers.Value;

                if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES && !ArkController.IsResettingPlayers)
                {
                    flag = false;
                }

                if (player?.primaryHand)
                {
                    player.primaryHand.ForceRenderersOff = !flag;
                }

                if (player?.secondaryHand)
                {
                    player.secondaryHand.ForceRenderersOff = !flag;
                }
            }
        }
    }
}