using UnityEngine;

namespace CuttingRoomFloor
{
    [RequireComponent(typeof(DualWieldSynergyProcessor))]
    internal class CustomDualWieldSynergySwitcher : MonoBehaviour
    {
        public int firstDualWieldGun;
        public int secondDualWieldGun;

        private Gun m_gun;
        private DualWieldSynergyProcessor m_processor;

        protected void Awake()
        {
            this.m_gun = base.GetComponent<Gun>();
            this.m_processor = base.GetComponent<DualWieldSynergyProcessor>();
        }

        // this is (at least) called every time you switch to the gun, as that's when the gun is enabled again
        protected void OnEnable()
        {
            if (m_processor && m_gun && m_gun.CurrentOwner is PlayerController player)
            {
                m_processor.PartnerGunID = ShouldUseSecondGun(player) ? secondDualWieldGun : firstDualWieldGun;
            }
        }

        // this is exactly like checking player.HasGun for both guns, but it only uses one loop instead of two
        private bool ShouldUseSecondGun(PlayerController player)
        {
            bool foundSecondGun = false;

            foreach (var gun in player.inventory.AllGuns)
            {
                if (gun.PickupObjectId == firstDualWieldGun)
                {
                    return false;
                }

                if (gun.PickupObjectId == secondDualWieldGun)
                {
                    foundSecondGun = true;
                }
            }

            return foundSecondGun;
        }
    }
}