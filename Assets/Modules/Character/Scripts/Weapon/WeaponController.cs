using System.Collections;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;
using Random = UnityEngine.Random;

namespace Player.Weapon
{
    public enum WeaponState
    {
        Ready,
        Empty,
        Reloading,
    }

    [RequireComponent(typeof(Ammo))]
    public class WeaponController : MonoBehaviour
    {
        [Header("References:")]
        [SerializeField] private Transform shootPoint;
        [SerializeField] private GameObject bullet;
        [SerializeField] private GameObject muzzleFlash;
        [SerializeField] private Ammo ammo;

        [Header("Instance Settings:")] 
        [SerializeField] private Vector3 initialPosition;
        [SerializeField] private Quaternion initialRotation;

        [Header("Weapon Settings:")] 
        [SerializeField] private string displayName;
        [SerializeField] private float range = 100f;
        [SerializeField] private bool automatic;
        [SerializeField] private float fireRate;
        [SerializeField] private string gunSound;
        [SerializeField] private Vector3 adsOffset;

        [Header("Burst Settings")] 
        [SerializeField] private bool burst;
        [SerializeField] private int burstRounds;
        [SerializeField] private Vector3 burstFireIntensity;
        [SerializeField] private float adsIntensityDamping = 0.4f;
        
        [Header("Recoil Settings:")]
        [SerializeField] private float upRecoil = 3f;
        [SerializeField] private float sideRecoil = 1f;
        [SerializeField] private float adsRecoilDamping = 0.9f;

        [Header("Hip Fire Settings:")]
        [SerializeField] private Vector3 startSpread = Vector3.zero;
        [SerializeField] private float spreadCooldown = 1f;
        [SerializeField] private float spreadMultiplier = 0.05f;
        [SerializeField] private float maxSpreadMultiplier = 0.25f;
        
        private float nextFire;

        private WeaponState state = WeaponState.Empty;

        private Vector3 currentSpread;
        public float CurrentSpreadMultiplier { get; private set; }

        public bool Aiming { get; private set; }

        public Transform ShootPoint => shootPoint;

        public delegate void AimingDownSightDelegate(Vector3 _adsOffset, bool _released);
        public event AimingDownSightDelegate EventAimingDownSight;
        
        public delegate void FireDelegate(float _upRecoil, float _sideRecoil, float _adsRecoilDamping, bool _ads);
        public event FireDelegate EventFire;

        public Ammo Ammo => ammo;

        public string DisplayName => displayName;

        private InputManager inputManager;

        private void Awake() 
        {
            ammo.EventEmptyClip += () => state = WeaponState.Empty;
            ammo.EventReloading += () => state = WeaponState.Reloading;
            ammo.EventReloaded += () => state = WeaponState.Ready;
        }

        private void Start()
        {
            inputManager = InputManager.Instance;

            var weaponTransform = transform;
            weaponTransform.localPosition = initialPosition;
            weaponTransform.localRotation = initialRotation;
            
            // Default spread
            currentSpread = startSpread;

            var playerAim = inputManager.PlayerAim();
            playerAim.started += HandleStartADS;
            playerAim.canceled += HandleEndADS;
        }

        private void OnDestroy() {
            ammo.EventEmptyClip -= () => state = WeaponState.Empty;
            ammo.EventReloading -= () => state = WeaponState.Reloading;
            ammo.EventReloaded -= () => state = WeaponState.Ready;

            var playerAim = inputManager.PlayerAim();
            playerAim.started -= HandleStartADS;
            playerAim.canceled -= HandleEndADS;
        }

        private void Update()
        {
            HandleReload();
            
            // Play empty clip sound
            if (state != WeaponState.Ready) return;
            
            var fire = inputManager.PlayerFiredThisFrame() || inputManager.PlayerFiredThisFrame() && automatic;
            var canShoot = fire && Time.time >= nextFire;

            if (!canShoot) return;
            
            Shoot(shootPoint.position);

            nextFire = Time.time + 1f / fireRate;
            
            EventFire?.Invoke(upRecoil, sideRecoil, adsRecoilDamping, Aiming);

            ammo.Decrement();
        }

        private void Shoot(Vector3 _shootPosition)
        {
            if (Camera.main == null) return;
            
            var ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            var targetPoint = Physics.Raycast(ray, out var hit) ? hit.point : ray.GetPoint(range);
            var direction = targetPoint - _shootPosition;

            if (!Aiming && !burst)
            {
                direction = AddSpreadToBullet(direction);
            }

            if (burst)
            {
                BurstFire(_shootPosition, direction);
            }
            else
            {
                Instantiate(bullet, _shootPosition, Quaternion.LookRotation(direction));
            }

            if (muzzleFlash)
            {
                var flash = Instantiate(muzzleFlash, _shootPosition, Quaternion.LookRotation(direction), transform);
                flash.transform.localScale *= 0.1f;
                Destroy(flash, 0.1f);
            }
        }

        private void BurstFire(Vector3 _shootPosition, Vector3 _direction)
        {
            for (var i = 0; i < burstRounds; i++)
            {
                var randomExtraRotation = new Vector3(
                    Random.Range(-burstFireIntensity.x, burstFireIntensity.x),
                    Random.Range(-burstFireIntensity.y, burstFireIntensity.y),
                    Random.Range(-burstFireIntensity.z, burstFireIntensity.z)
                );

                if (Aiming)
                {
                    randomExtraRotation *= adsIntensityDamping;
                }
                
                var rotation = Quaternion.LookRotation(_direction);
                var rotationWithRandomness = Quaternion.Euler(
                    rotation.eulerAngles.x + randomExtraRotation.x,
                    rotation.eulerAngles.y + randomExtraRotation.y,
                    rotation.eulerAngles.z + randomExtraRotation.z
                );
                
                Instantiate(bullet, _shootPosition, rotationWithRandomness);
            }
        }

        private void HandleStartADS(CallbackContext ctx)
        {
            EventAimingDownSight?.Invoke(adsOffset, false);
            Aiming = true;
        }

        private void HandleEndADS(CallbackContext ctx)
        {
            EventAimingDownSight?.Invoke(adsOffset, true);
            Aiming = false;
        }

        private void HandleReload()
        {
            if (inputManager.PlayerReloadedThisFrame())
            {
                ammo.ForceReload();
            }
        }

        private Vector3 AddSpreadToBullet(Vector3 _direction)
        {
            _direction += new Vector3(
                Random.Range(-currentSpread.x, currentSpread.x),
                Random.Range(-currentSpread.y, currentSpread.y),
                Random.Range(-currentSpread.z, currentSpread.z)
                );

            if (!(CurrentSpreadMultiplier < maxSpreadMultiplier)) return _direction;
            
            CurrentSpreadMultiplier += spreadMultiplier;
            currentSpread = startSpread * (1 + CurrentSpreadMultiplier);
            StartCoroutine(ResetAddedSpread());

            return _direction;
        }

        private IEnumerator ResetAddedSpread()
        {
            yield return new WaitForSeconds(spreadCooldown);
        
            CurrentSpreadMultiplier -= spreadMultiplier;
        }
    }
}
