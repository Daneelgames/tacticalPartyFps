using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CustomLadderTool : MonoBehaviour
{
    public int maxLadderLength = 10;
    public void ConstructLadder(Vector3 targetPos)
    {
        /*
        if (Physics.Raycast(transform.position, targetPos - transform.position, out var hit, Mathf.Infinity, GameManager.Instance.AllSolidsMask))
        {
            targetPos = hit.point;
        }*/
        
        StartCoroutine(LevelGenerator.Instance.SpawnLadder(targetPos, transform.position, false, LevelGenerator.Instance.generatedBuildingFolder, maxLadderLength));
    }
}