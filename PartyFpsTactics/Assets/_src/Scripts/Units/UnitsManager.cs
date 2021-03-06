using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MrPink.Health;
using MrPink.PlayerSystem;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace MrPink.Units
{
    public class UnitsManager : MonoBehaviour
    {
        public static UnitsManager Instance;
        public List<HealthController> unitsInGame = new List<HealthController>();

        public int defaultInduranceDamage = 100;
    
        public float tileExplosionDistance = 3;
        public float tileExplosionForce = 100;
        public float tileExplosionForceBarrels = 50;
        public float tileExplosionForcePlayer = 100;

        public PhysicMaterial corpsesMaterial;
        public List<HealthController> redTeamUnitPrefabs;
        public List<HealthController> blueTeamUnitPrefabs;
        public List<HealthController> neutralUnitPrefabs;
        public List<HealthController> desertBeastsPrefabs;
    
        // TODO use real queue
        private readonly List<BasicHealth> _bodyPartsQueueToKill = new List<BasicHealth>();
        private readonly List<BasicHealth> _bodyPartsQueueToKillCombo = new List<BasicHealth>();

        private Transform _spawnRoot;
    
        private void Awake()
        {
            Instance = this;

            _spawnRoot = new GameObject("Unit Spawn Root").transform;
        }

        private void Start()
        {
            StartCoroutine(UniTask.ToCoroutine(BodyPartsKillQueue));
        }

        public HealthController SpawnUnit(HealthController prefab, Vector3 pos, Transform rotationTransform = null)
        {
            var rot = Quaternion.identity;
            if (rotationTransform != null)
                rot = rotationTransform.rotation;
            return Instantiate(prefab, pos, rot, _spawnRoot);
        }
        
        public void SpawnBlueUnit(Vector3 pos)
        {
            pos = SamplePos(pos);
            Instantiate(blueTeamUnitPrefabs[Random.Range(0, blueTeamUnitPrefabs.Count)], pos, Quaternion.identity, _spawnRoot);
        }
    
        public HealthController SpawnRedUnit(Vector3 pos)
        {
            pos = SamplePos(pos);
            return Instantiate(redTeamUnitPrefabs[Random.Range(0, redTeamUnitPrefabs.Count)], pos, Quaternion.identity, _spawnRoot);
        }
    
        public void SpawnNeutralUnit(Vector3 pos)
        {
            pos = SamplePos(pos);
            Instantiate(neutralUnitPrefabs[Random.Range(0, neutralUnitPrefabs.Count)], pos, Quaternion.identity, _spawnRoot);
        }
    
        public void SpawnDesertBeast(Vector3 pos)
        {
            pos = SamplePos(pos);
            Instantiate(desertBeastsPrefabs[Random.Range(0, desertBeastsPrefabs.Count)], pos, Quaternion.identity, _spawnRoot);
        }

        Vector3 SamplePos(Vector3 pos)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(pos, out hit, 10, NavMesh.AllAreas))
            {
                pos = hit.position;
            }

            return pos;
        }

        
        public void RagdollTileExplosion(Vector3 explosionPosition, float distance = -1, float force = -1,
            float playerForce = -1, ScoringActionType action = ScoringActionType.NULL, int enduranceDamage = -1)
        {
            if (distance < 0)
                distance = tileExplosionDistance;

            if (force < 0)
                force = tileExplosionForce;

            if (playerForce < 0)
                playerForce = tileExplosionForcePlayer;

            if (enduranceDamage < 0)
                enduranceDamage = defaultInduranceDamage;
            
            // BUMP ENEMIES
            for (int i = 0; i < unitsInGame.Count; i++)
            {
                if (Vector3.Distance(explosionPosition, unitsInGame[i].transform.position + Vector3.up) <= distance)
                {
                    if (unitsInGame[i].playerMovement)
                    {
                        unitsInGame[i].playerMovement.rb
                            .AddForce(
                                (unitsInGame[i].visibilityTrigger.transform.position - explosionPosition).normalized *
                                playerForce, ForceMode.VelocityChange);
                        continue;
                    }

                    if (unitsInGame[i].rb) // BARRELS
                    {
                        unitsInGame[i].rb
                            .AddForce(
                                (unitsInGame[i].visibilityTrigger.transform.position - explosionPosition).normalized *
                                tileExplosionForceBarrels, ForceMode.VelocityChange);

                        unitsInGame[i].Damage(1, DamageSource.Player);
                        if (action != ScoringActionType.NULL)
                            ScoringSystem.Instance.RegisterAction(ScoringActionType.BarrelBumped, 3);

                        continue;
                    }

                    if (unitsInGame[i].DamageEndurance(enduranceDamage) <= 0)
                    {
                        if (unitsInGame[i].HumanVisualController)
                        {
                            if (unitsInGame[i].health > 0 && action != ScoringActionType.NULL)
                                ScoringSystem.Instance.RegisterAction(ScoringActionType.EnemyBumped, 2);
                            unitsInGame[i].HumanVisualController.ActivateRagdoll();
                            unitsInGame[i].HumanVisualController.ExplosionRagdoll(explosionPosition, force, distance);
                        }

                        if (unitsInGame[i].AiMovement)
                            unitsInGame[i].AiMovement.StopActivities();

                    }
                }
            }

            // BUMP PROPS
            bool propBumped = false;
            
            if (BuildingGenerator.Instance != null)
                for (int i = 0; i < BuildingGenerator.Instance.spawnedProps.Count; i++)
                {
                    if (Vector3.Distance(BuildingGenerator.Instance.spawnedProps[i].transform.position, explosionPosition) >
                        distance)
                        continue;

                    var rb = BuildingGenerator.Instance.spawnedProps[i].Rigidbody;

                    if (!rb) 
                        continue;
                
                    propBumped = true;
                    rb.AddExplosionForce(tileExplosionForceBarrels * 30, explosionPosition, distance);
                    BuildingGenerator.Instance.spawnedProps[i].tileAttack.dangerous = true;
                }

            if (propBumped && action != ScoringActionType.NULL)
                ScoringSystem.Instance.RegisterAction(ScoringActionType.PropBumped, 2);

        }

        public void AddHealthEntityToQueue(BasicHealth part, ScoringActionType action)
        {
            if (action != ScoringActionType.NULL)
                _bodyPartsQueueToKillCombo.Add(part);
            else
                _bodyPartsQueueToKill.Add(part);
        }

        public void MoveUnitsToRespawnPoints(bool destroyDead, bool healAlive)
        {
            for (int i = 0; i < unitsInGame.Count; i++)
            {
                var unit = unitsInGame[i];
                if (!unit)
                    continue;

                if (unit.health <= 0)
                {
                 if (destroyDead)
                     Destroy(unit.gameObject);
                
                 continue;
                }
                
                if (unit.health > 0)
                {
                    if (unit.health > 0 && unit.selfUnit && unit.selfUnit.UnitMovement)
                        unit.selfUnit.UnitMovement.TeleportToRespawnPosition();
                    
                    if (healAlive)
                        unit.AddHealth(unit.healthMax);
                }

            }
        }
        
        private async UniTask BodyPartsKillQueue()
        {
            int handledInFrame = 0;
            while (true)
            {
                handledInFrame = await HandleKillQueue(_bodyPartsQueueToKillCombo, DamageSource.Player, handledInFrame);
                await HandleKillQueue(_bodyPartsQueueToKill, DamageSource.Environment, handledInFrame);
            
                await UniTask.DelayFrame(1);
                handledInFrame = 0;
            }
        }

        private static async UniTask<int> HandleKillQueue(List<BasicHealth> healthToKillQueue, DamageSource damageSource, int handledInFrame)
        {
            if (healthToKillQueue.Count <= 0)
                return handledInFrame;

            for (int i = healthToKillQueue.Count - 1; i >= 0; i--)
            {
                if (healthToKillQueue[i] != null)
                    healthToKillQueue[i].Kill(damageSource);
                healthToKillQueue.RemoveAt(i);
                
                handledInFrame++;
                if (handledInFrame > 3)
                {
                    handledInFrame = 0;
                    await UniTask.DelayFrame(1);
                }
            }

            return handledInFrame;
        }
    
    }
}