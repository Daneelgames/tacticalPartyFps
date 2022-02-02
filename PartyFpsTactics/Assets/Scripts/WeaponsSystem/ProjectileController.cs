using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem.Processors;
using Random = UnityEngine.Random;

public class ProjectileController : MonoBehaviour
{
    public float projectileSpeed = 100;
    public int damage = 100;
    public float lifeTime = 2;
    public Rigidbody rb;
    private float gravity = 13;
    public LayerMask solidsMask;
    public LayerMask unitsMask;
    private Vector3 currentPosition;
    private Vector3 lastPosition;
    private float distanceBetweenPositions;
    private HealthController ownerHc;
    public AudioSource shotAu;
    public AudioSource flyAu;
    public AudioClip hitSolidFx;
    public AudioClip hitUnitFx;
    public AudioSource hitAu;
    private bool dead = false;

    public Transform debrisParticles;
    public Transform bloodParticles;
    public void Init(HealthController _ownerHc)
    {
        ownerHc = _ownerHc;
        lastPosition = transform.position;
        shotAu.pitch = Random.Range(0.75f, 1.25f);
        shotAu.Play();
        flyAu.pitch = Random.Range(0.75f, 1.25f);
        flyAu.Play();
        StartCoroutine(MoveProjectile());
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(lastPosition, currentPosition - lastPosition);
    }

    private void Update()
    {
        if (dead)
            return;
        
        rb.velocity = transform.forward * projectileSpeed + Vector3.down * gravity * Time.deltaTime;
        currentPosition  = transform.position;
        distanceBetweenPositions = Vector3.Distance(currentPosition, lastPosition);
        if (Physics.Raycast(lastPosition, currentPosition - lastPosition, out var hit, distanceBetweenPositions, solidsMask, QueryTriggerInteraction.Collide))
        {
            if (hit.transform == null)
                return;
                
            var type = TryToDamage(hit.collider); // 0 solid, 1 unit
            
            if (type == 0) HitSolidFeedback();
            else HitUnitFeedback(hit.point);
            
            Death();
            return;
        }
        if (Physics.SphereCast(lastPosition, 0.3f, currentPosition - lastPosition, out hit, distanceBetweenPositions, unitsMask, QueryTriggerInteraction.Collide))
        {
            if (hit.transform == null)
                return;
            
            if (hit.collider.gameObject == ownerHc.gameObject)
                return;

            if (hit.collider.gameObject == PlayerMovement.Instance.gameObject)
            {
                HitUnitFeedback(hit.point);
                PlayerMovement.Instance.Death(ownerHc);
                Death();
                return;
            }
                
            TryToDamage(hit.collider);
            HitUnitFeedback(hit.point);
            Death();
        }
    }

    int TryToDamage(Collider coll)
    {
        int damagedObjectType = 0;// 0 - solid, 1 - unit
        var bodyPart = coll.gameObject.GetComponent<BodyPart>();
        if (bodyPart)
        {
            if (bodyPart.hc == null && bodyPart.localHealth > 0)
            {
                UnitsManager.Instance.RagdollTileExplosion(transform.position);
                bodyPart.DamageTile(damage);
                damagedObjectType = 0;
            }
            if (bodyPart.hc == ownerHc)
                return -1;
                
            if (bodyPart && bodyPart.hc)
            {
                UnitsManager.Instance.RagdollTileExplosion(transform.position);
                bodyPart.hc.Damage(damage);
            }
        }

        return damagedObjectType;
    }

    IEnumerator MoveProjectile()
    {
        float currentLifeTime = 0;
        while (true)
        {
            if (dead)
                yield break;

            yield return null;
            currentLifeTime += Time.deltaTime;
            if (currentLifeTime > lifeTime)
            {
                Destroy(gameObject);
                yield break;
            }
            lastPosition = transform.position;
        }
    }

    void HitSolidFeedback()
    {
        hitAu.clip = hitSolidFx;
        hitAu.pitch = Random.Range(0.75f, 1.25f);
        hitAu.Play();
        debrisParticles.parent = null;
        debrisParticles.gameObject.SetActive(true);
    }
    void HitUnitFeedback(Vector3 contactPoint)
    {
        hitAu.clip = hitUnitFx;
        hitAu.pitch = Random.Range(0.75f, 1.25f);
        hitAu.Play();
        bloodParticles.parent = null;
        bloodParticles.position = contactPoint;
        bloodParticles.gameObject.SetActive(true);
    }

    void Death()
    {
        dead = true;
        rb.isKinematic = true;
        transform.GetChild(0).gameObject.SetActive(false);
        StartCoroutine(DeathCoroutine());
        Destroy(gameObject, 3);
    }

    IEnumerator DeathCoroutine()
    {
        float t = 0;
        while (t < 0.5f)
        {
            flyAu.volume -= Time.deltaTime * 50;
            yield return null;
        }
    }
}
