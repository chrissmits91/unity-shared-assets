using UnityEngine;

namespace Player.Weapon
{
    public class GunHolder : MonoBehaviour
    {
        [SerializeField] private WeaponController equipedWeapon;
        [SerializeField] private Vector3 hipFirePosition = new Vector3(0.21f, -0.2f, 0.44f);
        [SerializeField] private Vector3 adsPosition = new Vector3(0f, -0.22f, 0.44f);
        [SerializeField] private float aimSpeed;
        
        private float adsZoomLerpTime;
        private float adsReleaseLerpTime;

        public WeaponController EquipedWeapon => equipedWeapon;
        
        private void Start()
        {
            transform.localPosition = hipFirePosition;

            equipedWeapon.EventAimingDownSight += OnAimingDownSight;
        }

        private void OnDestroy() {
            equipedWeapon.EventAimingDownSight -= OnAimingDownSight;
        }

        private void OnAimingDownSight(Vector3 adsOffset, bool released) {
            if (released) {
                transform.localPosition = hipFirePosition;
            } else {
                transform.localPosition = adsPosition + adsOffset;
            }
        }
    }
}