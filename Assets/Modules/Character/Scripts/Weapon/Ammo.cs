using UnityEngine;
using System.Collections;

namespace Player.Weapon
{
    public class Ammo : MonoBehaviour
    {
        [SerializeField] private float totalAmmo;
        [SerializeField] private float clipSize;
        [SerializeField] private float reloadTime;
        [SerializeField] private float reloadTimeWhenEmpty;

        private float currentClipSize;

        public delegate void EmptyClipDelegate();
        public event EmptyClipDelegate EventEmptyClip;

        public delegate void ReloadingDelegate();
        public event ReloadingDelegate EventReloading;

        public delegate void ReloadedDelegate();
        public event ReloadedDelegate EventReloaded;
        
        public delegate void ClipSizeDelegate(float _currentClipSize, float _totalAmmo);
        public event ClipSizeDelegate EventClipSizeChanged;

        private void Start()
        {
            Reload();
        }

        public void ForceReload()
        {
            if (totalAmmo <= 0) return;
            
            EventReloading?.Invoke();
            StartCoroutine(Reloading(currentClipSize <= 0 ? reloadTimeWhenEmpty : reloadTime));
        }

        public void Refresh()
        {
            EventClipSizeChanged?.Invoke(currentClipSize, totalAmmo);
        }

        public void Decrement()
        {
            currentClipSize--;
            EventClipSizeChanged?.Invoke(currentClipSize, totalAmmo);
            if (currentClipSize <= 0)
            {
                EventEmptyClip?.Invoke();
            }
        }

        private void Reload()
        {
            if (totalAmmo < clipSize)
            {
                currentClipSize = totalAmmo;
                totalAmmo = 0;
            }
            else
            {
                currentClipSize = clipSize;
                totalAmmo -= clipSize;
            }
            
            EventReloaded?.Invoke();
            EventClipSizeChanged?.Invoke(currentClipSize, totalAmmo);
        }

        private IEnumerator Reloading(float _reloadTime)
        {
            yield return new WaitForSeconds(_reloadTime);

            Reload();
        } 
    }
}
