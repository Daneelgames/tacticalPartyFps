using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class UnitVision : MonoBehaviour
{
    private HealthController hc;
    public float fov = 70.0f;
    private RaycastHit hit;
    public LayerMask raycastsLayerMask;
    public Transform raycastOrigin;

    private List<HealthController> visibleEnemies = new List<HealthController>();

    public List<HealthController> VisibleEnemies
    {
        get { return visibleEnemies; }
    }
    private void Start()
    {
        hc = GetComponent<HealthController>();
        StartCoroutine(CheckEnemies());
    }

    IEnumerator CheckEnemies()
    {
        while (hc.health > 0)
        {
            for (int i = 0; i < UnitsManager.Instance.unitsInGame.Count; i++)
            {
                var enemy = UnitsManager.Instance.unitsInGame[i];
                
                if (enemy.health <= 0)
                {
                    if (visibleEnemies.Contains(enemy))
                    {
                        visibleEnemies.Remove(enemy);
                    }
                    continue;
                }
                
                if (enemy.team == hc.team || 
                    enemy.team == HealthController.Team.NULL)
                    continue;

                if (LineOfSight(enemy.visibilityTrigger.transform))
                {
                    if (!visibleEnemies.Contains(enemy))
                    {
                        visibleEnemies.Add(enemy);
                    }
                }
                else if (visibleEnemies.Contains(enemy))
                {
                    visibleEnemies.Remove(enemy);
                }
                yield return new WaitForSeconds(0.1f);   
            }
            
            yield return null;
        }
        visibleEnemies.Clear();
    }

    bool LineOfSight (Transform target) 
    {
        if (Vector3.Angle((target.position + Vector3.one * 1.25f) - raycastOrigin.position, transform.forward) <= fov &&
            Physics.Linecast(raycastOrigin.position, target.position, out hit, raycastsLayerMask) &&
            hit.collider.transform == target)
        {
            return true;
        }

        return false;
    }
}