using UnityEngine;

namespace Player.Weapon
{
    public class Recoil : MonoBehaviour
    {
        [SerializeField] private GameObject cameraRecoilHandler;
        [SerializeField] private float positionalSpeed = 10f;

        private GunHolder gunHolder;
        
        private void Awake()
        {
            gunHolder = GetComponent<GunHolder>();
            gunHolder.EventWeaponSwitch += OnWeaponSwitch;
        }

        private void OnWeaponSwitch()
        {
            gunHolder.EquipedWeapon.EventFire += OnFire;
        }

        private void OnFire(float _upRecoil, float _sideRecoil, float _adsRecoilDamping, bool _ads)
        {
            if (_ads)
            {
                _upRecoil *= 1f - _adsRecoilDamping;
                _sideRecoil *= 1f - _adsRecoilDamping;
            }

            var localRotation = cameraRecoilHandler.transform.localRotation;
            var currentRecoilAngles = localRotation.eulerAngles;
            localRotation = Quaternion.Lerp(
                localRotation,
                Quaternion.Euler(
                    currentRecoilAngles.x - _upRecoil,
                    currentRecoilAngles.y - _sideRecoil,
                    currentRecoilAngles.z),
                positionalSpeed * Time.deltaTime
            );
            cameraRecoilHandler.transform.localRotation = localRotation;
        }
    }
}