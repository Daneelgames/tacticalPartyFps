using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtClosestBuilding : MonoBehaviour
{
    private Vector3 closestBuildingPosition;

    private void Start()
    {
        StartCoroutine(GetClosestBuilding());
    }

    IEnumerator GetClosestBuilding()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            
            if (BuildingGenerator.Instance.spawnedBuildings.Count <= 0)
                continue;
            
            var buildings = BuildingGenerator.Instance.spawnedBuildings;
            float distance = 10000;
            
            for (int i = 0; i < buildings.Count; i++)
            {
                float newDist = Vector3.Distance(transform.position, buildings[i].worldPos);
                if (newDist < distance)
                {
                    distance = newDist;
                    closestBuildingPosition = buildings[i].worldPos;
                }

                yield return null;

            }
        }
    }

    private void Update()
    {
        transform.LookAt(closestBuildingPosition);
    }
}
