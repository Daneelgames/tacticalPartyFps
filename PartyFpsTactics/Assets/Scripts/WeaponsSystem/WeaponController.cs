using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Transform shotHolder;
    public ProjectileController projectilePrefab;
    public float cooldown = 1;
    bool onCooldown = false;
    public bool OnCooldown
    {
        get { return onCooldown; }
        set { onCooldown = value; }
    }
    
    public void Shot(HealthController ownerHc)
    {
        Shot(shotHolder.forward, ownerHc);
    }

    public void Shot(Vector3 direction, HealthController ownerHc)
    {
        var newProjectile = Instantiate(projectilePrefab, shotHolder.position, Quaternion.LookRotation(direction));
        newProjectile.Init(ownerHc);
        StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown()
    {
        OnCooldown = true;
        yield return new WaitForSeconds(cooldown);
        OnCooldown = false;
    }
}