using System.Collections.Generic;
using MrPink.PlayerSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _src.Scripts
{
    public class Respawner : MonoBehaviour
    {
        public float corpseShredderY = -50;
        public List<Transform> redRespawns;
        public Vector2Int enemiesPerRoomMinMax = new Vector2Int(3,10);
        public List<Transform> blueRespawns;
        public int alliesAmount = 3;
        List<BodyPart> tilesForSpawns = new List<BodyPart>();

        public static Respawner Instance;
        public bool spawn = false;
        private void Awake()
        {
            Instance = this;
        }

        public void Init()
        {
            if (!spawn)
                return;
        
            // create enemy spawns
            tilesForSpawns = new List<BodyPart>();
            enemiesPerRoomMinMax = ProgressionManager.Instance.levelDatas[ProgressionManager.Instance.currentLevelIndex].enemiesPerRoomMinMax;

            for (int i = 0; i < LevelGenerator.Instance.spawnedLevels.Count; i++)
            {
                tilesForSpawns.Clear();
                for (var index = LevelGenerator.Instance.spawnedLevels[i].tilesInside.Count - 1; index >= 0; index--)
                {
                    var tile = LevelGenerator.Instance.spawnedLevels[i].tilesInside[index];
                    if (tile == null)
                    {
                        LevelGenerator.Instance.spawnedLevels[i].tilesInside.RemoveAt(index);
                        continue;
                    }
                    tilesForSpawns.Add(tile);
                }

                for (int j = 0; j < alliesAmount; j++)
                {
                    var randomTile = tilesForSpawns[Random.Range(0, tilesForSpawns.Count)];
                    var newSpawnPoint = new GameObject("BlueSpawnPoint");
                    newSpawnPoint.transform.parent = transform;
                    blueRespawns.Add(newSpawnPoint.transform);
                
                    GameManager.Instance.SpawnBlueUnit(randomTile.transform.position);   
                }
            
                if (i < 1)
                    continue;

                for (int j = 0; j < Random.Range(enemiesPerRoomMinMax.x, enemiesPerRoomMinMax.y); j++)
                {
                    var randomTile = tilesForSpawns[Random.Range(0, tilesForSpawns.Count)];
                    var newSpawnPoint = new GameObject("RedSpawnPoint");
                    newSpawnPoint.transform.parent = transform;
                    redRespawns.Add(newSpawnPoint.transform);
                
                    GameManager.Instance.SpawnRedUnit(randomTile.transform.position);
                }
            }
        }

        void Update()
        {
            if (Player.GameObject.transform.position.y < corpseShredderY)
            {
                GameManager.Instance.StartProcScene();
                return;
            }
            for (int i = UnitsManager.Instance.unitsInGame.Count - 1; i >= 0; i--)
            {
                if (i >= UnitsManager.Instance.unitsInGame.Count)
                    continue;
            
                var corpse = UnitsManager.Instance.unitsInGame[i];
                if (corpse.HumanVisualController && corpse.HumanVisualController.rigidbodies[0].transform.position.y < corpseShredderY)
                {
                    /*
                switch (corpse.team)
                {
                    case HealthController.Team.Blue:
                        GameManager.Instance.SpawnBlueUnit(blueRespawns[Random.Range(0, blueRespawns.Count)].position);
                        break;
                    case HealthController.Team.Red:
                        var spawners = GetSpawnersInRange(redRespawns, PlayerMovement.Instance.transform.position, 10, 100);
                        if (spawners.Count == 0)
                        {
                            spawners = new List<Transform>(redRespawns);
                        }
                        GameManager.Instance.SpawnRedUnit(spawners[Random.Range(0, spawners.Count)].position);
                        break;
                }
                */

                    Destroy(corpse.gameObject);
                }
            }
        }

        public List<Transform> GetSpawnersInRange(List<Transform> spawners, Vector3 origin, float minRange, float maxRange)
        {
            List<Transform> temp = new List<Transform>(spawners);
            for (int i = temp.Count - 1; i >= 0; i--)
            {
                float dst = Vector3.Distance(origin, temp[i].position);
                if (dst < minRange || dst > maxRange)
                    temp.RemoveAt(i);
            }

            return temp;
        }
    }
}
