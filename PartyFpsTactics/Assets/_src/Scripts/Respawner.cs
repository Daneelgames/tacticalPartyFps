using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Respawner : MonoBehaviour
{
    public float corpseShredderY = -50;
    public List<Transform> redRespawns;
    public Vector2Int enemiesPerRoomMinMax = new Vector2Int(3,10);
    public List<Transform> blueRespawns;
    public int alliesAmount = 3;
    List<GameObject> tilesForSpawns = new List<GameObject>();

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
        tilesForSpawns = new List<GameObject>();

        for (int i = 0; i < LevelGenerator.Instance.spawnedLevels.Count; i++)
        {
            tilesForSpawns.Clear();
            foreach (var tile in LevelGenerator.Instance.spawnedLevels[i].tilesInside)
            {
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
        if (PlayerMovement.Instance.transform.position.y < corpseShredderY)
        {
            GameManager.Instance.Restart();
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