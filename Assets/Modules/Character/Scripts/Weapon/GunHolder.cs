using UnityEngine;

namespace Player.Weapon
{
    public class GunHolder : MonoBehaviour
    {
        [SerializeField] private WeaponController equipedWeapon;
        [SerializeField] private Vector3 hipFirePosition = new Vector3(0.21f, -0.2f, 0.44f);
        [SerializeField] private Vector3 adsPosition = new Vector3(0f, -0.22f, 0.44f);
        [SerializeField] private float aimSpeed;
        [SerializeField] private float desiredAdsZoom = 10f;
        
        private float defaultFieldOfView;
        private float desiredAdsFieldOfView;
        private float adsZoomFraction;
        private float adsResetFraction;

        public WeaponController EquipedWeapon => equipedWeapon;
        
        public delegate void WeaponSwitchDelegate();
        public event WeaponSwitchDelegate EventWeaponSwitch;
        
        private void Start()
        {
            defaultFieldOfView = Camera.main.fieldOfView;
            desiredAdsFieldOfView = defaultFieldOfView - desiredAdsZoom;
            transform.localPosition = hipFirePosition;
        }

        public void EquipWeapon(WeaponController _weapon)
        {
            equipedWeapon = _weapon;
            equipedWeapon.EventAimingDownSight += OnAimingDownSight;
            
            EventWeaponSwitch?.Invoke();
            equipedWeapon.Ammo.Refresh();
        }
        
        private void Update()
        {
            if (equipedWeapon && !equipedWeapon.Aiming && Camera.main.fieldOfView != defaultFieldOfView)
            {
                HandleAimingDownSightReset();
            }
        }

        private void HandleAimingDownSightReset()
        {
            adsResetFraction += Time.deltaTime;
            
            Camera.main.fieldOfView = Mathf.SmoothStep(Camera.main.fieldOfView, defaultFieldOfView,
                adsResetFraction);
                
            transform.localPosition = Vector3.Lerp(transform.localPosition, hipFirePosition,
                adsResetFraction);
        }

        private void OnAimingDownSight(Vector3 _adsoffset, bool _released)
        {
            if (_released)
            {
                adsZoomFraction = 0f;
            }
            else
            {
                adsResetFraction = 0f;
                adsZoomFraction += Time.deltaTime;
                
                Camera.main.fieldOfView = Mathf.SmoothStep(Camera.main.fieldOfView, desiredAdsFieldOfView,
                    adsZoomFraction);
                
                transform.localPosition = Vector3.Lerp(transform.localPosition, adsPosition + _adsoffset,
                    adsZoomFraction);
            }
        }
    }
}