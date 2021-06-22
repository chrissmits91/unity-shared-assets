using UnityEngine;

namespace Player.Weapon
{
    public class CameraRecoilReset : MonoBehaviour
    {
        [SerializeField] private float recoilReturnSpeed = 1f;

        private void LateUpdate()
        {
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                Quaternion.Euler(0, 0, 0),
                recoilReturnSpeed * Time.deltaTime
            );
        }
    }
}