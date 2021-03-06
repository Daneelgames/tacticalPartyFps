using System;
using System.Collections.Generic;
using MrPink.Health;
using MrPink.PlayerSystem;
using UnityEngine;

namespace MrPink.WeaponsSystem
{
    public class TileAttack : BaseAttackCollider
    {
        [Header("TileAttack scales base damage by RB velocity")]
        [Tooltip("IF !PROP = can damage even if !dangerous")]
        public bool prop = false;
        public bool dangerous = false;
        public Rigidbody rb;
        private Vector3 lastFrameVelocity;
        public float minVelocityMagnitudeToAttack = 1;

        private List<Collider> collidedObjects = new List<Collider>();
        private float unitMinimalDamageTime = 0;
        public float unitMinimalDamageTimeMax = 1;

        private float t = 1;
        private void LateUpdate()
        {
            if (unitMinimalDamageTime > 0)
            {
                unitMinimalDamageTime -= Time.deltaTime;
                return;
            }
            
            if (!dangerous)
                return;
            
            lastFrameVelocity = rb.velocity;
            t -= Time.deltaTime;
            if (t <= 0)
            {
                t = 1;
                collidedObjects.Clear();
                if (lastFrameVelocity.magnitude < 1)
                    dangerous = false;
            }
        }

        
        private void OnCollisionEnter(Collision coll)
        {
            if (!dangerous && prop)
                return;
            if ( ! BuildingGenerator.Instance.IsLevelReady)
                return;
            if (coll.gameObject.layer != 7 /*&& coll.gameObject.layer != 6 && coll.gameObject.layer != 12*/)
                return;
            
            if (rb.velocity.magnitude < minVelocityMagnitudeToAttack)
                return;
            
            if (collidedObjects.Contains(coll.collider))
                return;
            
            collidedObjects.Add(coll.collider);
            
            //Debug.Log("TileAttack tryToDamage; rb.velocity.magnitude " + rb.velocity.magnitude);
            float scaler = rb.velocity.magnitude;
            if (coll.collider.gameObject == Game.Player.Movement.gameObject)
                scaler *= 0.5f;
            
            if (unitMinimalDamageTime > 0)
                scaler = 0.01f;
            
            var target = TryDoDamage(coll.collider, scaler);
            
            switch (target)
            {
                case CollisionTarget.Solid:
                    PlayHitSolidFeedback(coll.transform.position);
                    break;
                
                case CollisionTarget.Creature:
                    PlayHitUnitFeedback(coll.transform.position);
                    unitMinimalDamageTime = unitMinimalDamageTimeMax; 
                    break;
            }
        }
    }
}