using UnityEngine;

namespace Player.Weapon
{
    public class Recoil : MonoBehaviour
    {
        [SerializeField] private GameObject cameraRecoilHandler;
        [SerializeField] private float positionalSpeed = 10f;
        [SerializeField] private GunHolder gunHolder;

        private void Setup()
        {
            gunHolder.EquipedWeapon.EventFire += OnFire;
        }

        private void OnFire(float upRecoil, float sideRecoil, float adsRecoilDamping, bool ads)
        {
            if (ads) {
                upRecoil *= 1f - adsRecoilDamping;
                sideRecoil *= 1f - adsRecoilDamping;
            }

            var localRotation = cameraRecoilHandler.transform.localRotation;
            var currentRecoilAngles = localRotation.eulerAngles;
            localRotation = Quaternion.Lerp(
                localRotation,
                Quaternion.Euler(
                    currentRecoilAngles.x - upRecoil,
                    currentRecoilAngles.y - sideRecoil,
                    currentRecoilAngles.z),
                positionalSpeed * Time.deltaTime
            );
            cameraRecoilHandler.transform.localRotation = localRotation;
        }
    }
}