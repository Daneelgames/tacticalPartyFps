using System.Collections.Generic;
using _src.Scripts;
using MrPink.Health;
using MrPink.PlayerSystem;
using MrPink.Units;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MrPink
{
    public class ExplosionController : MonoBehaviour
    {
        public float lifeTime = 0.2f;
        public int damage = 1000;
        public float explosionDistance = 5;
        public float explosionForce = 200;
        public float explosionForcePlayer = 100;
        public AudioSource au;
        private List<GameObject> collidedGameObjects = new List<GameObject>();
        private ScoringActionType scoringAction = ScoringActionType.NULL;
    
        public void Init(ScoringActionType action)
        {
            scoringAction = action;
            au.pitch = Random.Range(0.75f, 1.25f);
            au.Play();
            UnitsManager.Instance.RagdollTileExplosion(transform.position, explosionDistance, explosionForce, explosionForcePlayer);
        }

        private void Update()
        {
            if (lifeTime > 0)
                lifeTime -= Time.deltaTime;
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (lifeTime <= 0)
                return;
            if (collidedGameObjects.Contains(other.gameObject))
                return;
        
            collidedGameObjects.Add(other.gameObject);
        
            if (Vector3.Distance(transform.position,Game.Player.Position) <= explosionDistance)
            {
                collidedGameObjects.Add(Game.Player.GameObject);
                Game.Player.Health.Damage(damage, DamageSource.Environment);
                return;
            }
        
            var health = other.gameObject.GetComponent<BasicHealth>();
            if (health == null || health.IsDead) 
                return;
        
            var remainingDamage = health.Health - damage;
        
            if (remainingDamage > 0)
                health.Damage(damage, DamageSource.Environment);
            else
                UnitsManager.Instance.AddHealthEntityToQueue(health, scoringAction);
        }
    }
}