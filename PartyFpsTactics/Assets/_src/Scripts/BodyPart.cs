using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPart : MonoBehaviour
{
    public HealthController hc;
    public int localHealth = 100;

    public void DamageTile(int dmg, ScoringActionType action = ScoringActionType.NULL)
    {
        if (localHealth <= 0)
            return;
        
        localHealth -= dmg;
        
        if (localHealth <= 0)
        {
            if (action != ScoringActionType.NULL)
            {
                ScoringSystem.Instance.RegisterAction(ScoringActionType.TileDestroyed, 1);
            }
            
            LevelGenerator.Instance.DebrisParticles(transform.position);
            Destroy(gameObject); 
            return;
        }
        LevelGenerator.Instance.TileDamaged(this);
    }

    public void Kill(bool combo)
    {
        if (hc)
            hc.Damage(hc.health);
        else if (localHealth > 0)
        {
            if (combo)
                ScoringSystem.Instance.RegisterAction(ScoringActionType.TileDestroyed);
            DamageTile(localHealth);
        }
    }
}