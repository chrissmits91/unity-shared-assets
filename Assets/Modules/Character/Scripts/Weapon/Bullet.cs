using System.Collections.Generic;
using Core;
using UnityEngine;

namespace Player.Weapon
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float speed = 120f;
        [SerializeField] private LayerMask collideWith;
        [SerializeField] private float range = 240f;
        [SerializeField] private float damage = 5f;
        [SerializeField] private float impactForce = 2f;

        [SerializeField] private List<Impact> impactEffects = new List<Impact>();

        private float travelled;

        private void Update()
        {
            var t = transform;
            t.position += t.forward * (speed * Time.deltaTime);

            if (travelled >= range)
            {
                Destroy(gameObject);
            }

            var ray = Helper.ForwardCollisionRay(
                transform,
                speed,
                out var nextTravelDistance,
                out var nextPosition);

            travelled += nextTravelDistance;

            // Accurate collision detection
            if (Physics.Raycast(ray, out var hit, nextTravelDistance, collideWith))
            {
                Hit(hit);

                Destroy(gameObject);
            }

            transform.position += nextPosition;
        }

        private void Hit(RaycastHit _hit)
        {
            Debug.Log($"HitReg on {_hit.transform.name}");
        }
        
        // private void ImpactEffect()
        // {
        //     switch (hit.collider.tag)
        //     {
        //         case "Enemy":
        //         {
        //             var effect = impactEffects.Find(_x => _x.tag == "Flesh");
        //             Destroy(
        //                 Instantiate(effect.effect, hit.point, Quaternion.LookRotation(hit.normal),
        //                     hit.collider.transform), effect.destroyAfter);
        //             break;
        //         }
        //         case "Ground":
        //         {
        //             var effect = impactEffects.Find(_x => _x.tag == "Ground");
        //             Destroy(
        //                 Instantiate(effect.effect, hit.point, Quaternion.LookRotation(hit.normal),
        //                     hit.collider.transform), effect.destroyAfter);
        //             break;
        //         }
        //     }
        // }
    }
}