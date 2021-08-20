using Dungeonator;
using ItemAPI;
using UnityEngine;

namespace CuttingRoomFloor
{
    internal class CustomTransformGunSynergyProcessor : MonoBehaviour
    {
        public string SynergyToActivateTransformation;

        public CustomSynergyType? SynergyToSuppressTransformation;

        public int NonSynergyGunId;

        public int SynergyGunId;

        private Gun m_gun;

        private bool m_transformed;

        public CustomTransformGunSynergyProcessor() : base()
        {
            this.NonSynergyGunId = -1;
            this.SynergyGunId = -1;
        }

        protected void Awake()
        {
            this.m_gun = base.GetComponent<Gun>();
        }

        protected void Update()
        {
            if (Dungeon.IsGenerating || Dungeon.ShouldAttemptToLoadFromMidgameSave)
            {
                return;
            }

            if (this.m_gun && this.m_gun.CurrentOwner is PlayerController)
            {
                PlayerController playerController = this.m_gun.CurrentOwner as PlayerController;

                if (!this.m_gun.enabled)
                {
                    return;
                }

                bool hasSuppressSynergy = SynergyToSuppressTransformation != null && playerController.HasActiveBonusSynergy(SynergyToSuppressTransformation.Value, false);

                if (hasSuppressSynergy)
                {
                    this.m_transformed = false;
                    // don't transform ourselves, the other synergy has already done that
                }

                if (playerController.PlayerHasActiveSynergy(this.SynergyToActivateTransformation) && !this.m_transformed && !hasSuppressSynergy)
                {
                    this.m_transformed = true;
                    this.m_gun.TransformToTargetGun(PickupObjectDatabase.GetById(this.SynergyGunId) as Gun);
                }
                else if ((!playerController.PlayerHasActiveSynergy(this.SynergyToActivateTransformation)) && this.m_transformed)
                {
                    this.m_transformed = false;

                    this.m_gun.TransformToTargetGun(PickupObjectDatabase.GetById(this.NonSynergyGunId) as Gun);
                }
            }
            else if (this.m_gun && !this.m_gun.CurrentOwner && this.m_transformed)
            {
                this.m_transformed = false;
                this.m_gun.TransformToTargetGun(PickupObjectDatabase.GetById(this.NonSynergyGunId) as Gun);
            }
        }
    }
}