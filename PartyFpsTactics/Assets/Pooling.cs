using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.WeaponsSystem;
using UnityEngine;
using UnityEngine.Pool;

public class Pooling : MonoBehaviour
{
    public static Pooling Instance;
    public List<AttackColliderTaggedPrefab> attackColliderPrefabs;
    public List<ParticlesTaggedPrefab> particlesPrefabs;
    [Space]
    
    public List<AttackColliderPool> AttackColliderPools;
    public List<ParticlesPool> ParticlesPools;


    [Serializable]
    public class AttackColliderPool
    {
        [Serializable]
        public enum AttackColliderPrefabTag
        {
            DesertBeast, DesertBeastSmall, AiPistol, AiSmg, AiShotgun, PlayerPistol, PlayerSword 
        }

        public AttackColliderPrefabTag attackColliderPrefabTag;
        public List<BaseAttackCollider> pool;
    }
    [Serializable]
    public class AttackColliderTaggedPrefab
    {
        public AttackColliderPool.AttackColliderPrefabTag tag;
        public BaseAttackCollider prefab;
    }
    [Serializable]
    public class ParticlesPool
    {
        [Serializable]
        public enum ParticlePrefabTag
        {
            Blood, Debris 
        }

        public ParticlePrefabTag particlePrefabTag;
        public List<GameObject> pool;
    }
    [Serializable]
    public class ParticlesTaggedPrefab
    {
        public ParticlesPool.ParticlePrefabTag tag;
        public GameObject prefab;
    }
    
    private void Awake()
    {
        Instance = this;
        AttackColliderPools = new List<AttackColliderPool>();
        ParticlesPools = new List<ParticlesPool>();

    }

    public BaseAttackCollider SpawnProjectile(AttackColliderPool.AttackColliderPrefabTag attackColliderPrefabTag, Transform newParent, Vector3 spawnPos, Quaternion spawnRot)
    {
        if (newParent)
        {
            spawnPos = newParent.position;
            spawnRot = newParent.rotation;
        }

        BaseAttackCollider pooledCollider;
        // find pool
        AttackColliderPool foundPool = null;
        for (int i = 0; i < AttackColliderPools.Count; i++)
        {
            if (AttackColliderPools[i].attackColliderPrefabTag == attackColliderPrefabTag)
            {
                foundPool = AttackColliderPools[i];
                break;
            }
        }

        if (foundPool == null)
        {
            var newList = new AttackColliderPool();
            newList.pool = new List<BaseAttackCollider>();
            newList.attackColliderPrefabTag = attackColliderPrefabTag;
            
            InstantiateCollider(newList);
            
            AttackColliderPools.Add(newList);
            foundPool = newList;
        }

        pooledCollider = GetColliderFromPool(foundPool);
        pooledCollider.transform.parent = newParent;
        
        if (attackColliderPrefabTag == AttackColliderPool.AttackColliderPrefabTag.PlayerSword)
            pooledCollider.transform.localScale = Vector3.one;
        
        pooledCollider.transform.position = spawnPos;
        pooledCollider.transform.rotation = spawnRot;
        pooledCollider.gameObject.SetActive(true); 
        return pooledCollider;
    }

    public GameObject SpawnParticle(ParticlesPool.ParticlePrefabTag _particlePrefabtag, Vector3 pos, Quaternion rot)
    {
        ParticlesPool poolFound = null;
        GameObject pooledParticle = null;
        for (int i = 0; i < ParticlesPools.Count; i++)
        {
            if (ParticlesPools[i].particlePrefabTag == _particlePrefabtag)
            {
                poolFound = ParticlesPools[i];
                break;
            }
        }

        if (poolFound == null)
        {
            var newList = new ParticlesPool();
            newList.particlePrefabTag = _particlePrefabtag;
            newList.pool = new List<GameObject>();
            InstantiateParticle(newList);
            ParticlesPools.Add(newList);
            poolFound = newList;
        }
        
        pooledParticle = GetParticleFromPool(poolFound);
        pooledParticle.transform.position = pos;
        pooledParticle.transform.rotation = rot;
        pooledParticle.SetActive(true); 
        return pooledParticle;
    }

    void InstantiateCollider(AttackColliderPool list)
    {
        for (int i = 0; i < attackColliderPrefabs.Count; i++)
        {
            if (attackColliderPrefabs[i].tag == list.attackColliderPrefabTag)
            {
                var newCollider = Instantiate(attackColliderPrefabs[i].prefab);
                newCollider.SetPool(list);
                list.pool.Add(newCollider);
                break;
            }
        }
    }
    
    void InstantiateParticle(ParticlesPool list)
    {
        for (int i = 0; i < particlesPrefabs.Count; i++)
        {
            if (particlesPrefabs[i].tag == list.particlePrefabTag)
            {
                var newCollider = Instantiate(particlesPrefabs[i].prefab);
                list.pool.Add(newCollider);
                break;
            }
        }
    }
    
    BaseAttackCollider GetColliderFromPool(AttackColliderPool list)
    {
        BaseAttackCollider coll = null;
        if (list.pool.Count <= 0)
        {
            InstantiateCollider(list);
        }
        coll = list.pool[0];
        list.pool.RemoveAt(0);

        return coll;
    }
    GameObject GetParticleFromPool(ParticlesPool list)
    {
        GameObject coll = null;
        if (list.pool.Count <= 0)
        {
            InstantiateParticle(list);
        }
        coll = list.pool[0];
        list.pool.RemoveAt(0);

        StartCoroutine(ReleaseParticle(coll, list));
        return coll;
    }

    public void ReleaseCollider(BaseAttackCollider coll, AttackColliderPool list)
    {
        for (int i = 0; i < AttackColliderPools.Count; i++)
        {
            if (AttackColliderPools[i].attackColliderPrefabTag == list.attackColliderPrefabTag)
            {
                AttackColliderPools[i].pool.Add(coll);
                break;
            }
        }
        coll.gameObject.SetActive(false);
    }
    IEnumerator ReleaseParticle(GameObject particle, ParticlesPool list)
    {
        yield return new WaitForSeconds(3);
        particle.gameObject.SetActive(false);
        list.pool.Add(particle);
    }
    
}