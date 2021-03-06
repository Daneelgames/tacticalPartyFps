using System.Collections;
using System.Collections.Generic;
using _src.Scripts.Data;
using MrPink;
using MrPink.Health;
using UnityEngine;

public class LevelEventsOnConditions : MonoBehaviour
{
    public static LevelEventsOnConditions Instance;

    public List<LevelEvent> currentEvents;

    public List<LevelEventActor> levelActors = new List<LevelEventActor>();
    void Awake()
    {
        Instance = this;
    }

    public void Init(ProcLevelData levelData)
    {
        currentEvents = new List<LevelEvent>(levelData.levelEvents);


        for (int i = 0; i < currentEvents.Count; i++)
        {
            StartCoroutine(CheckingEvent(currentEvents[i]));   
        }
    }

    public void AddActor(LevelEventActor levelEventActor)
    {
        levelActors.Add(levelEventActor);
    }

    public HealthController GetHcById(int id)
    {
        for (int i = 0; i < levelActors.Count; i++)
        {
            if (levelActors[i].actorId == id)
                return levelActors[i].gameObject.GetComponent<HealthController>();
        }

        return null;
    }
    
    IEnumerator CheckingEvent(LevelEvent levelEvent)
    {
        while (true)
        {
            bool allConditionsMet = true;
            for (int j = 0; j < levelEvent.conditions.Count; j++)
            {
                // IF ALL CONDITIONS MET - RUN EVENTS
                var condition = levelEvent.conditions[j];
                allConditionsMet = IsConditionMet(condition);
                
                if (!allConditionsMet)
                    break;
                
                yield return null;
            }

            if (allConditionsMet)
            {
                // RUN EVENTS
                for (int i = 0; i < levelEvent.events.Count; i++)
                {
                    InteractableEventsManager.Instance.RunEvent(levelEvent.events[i]);
                }
                
                // EVENT IS FINISHED
                yield break;
            }
            yield return null;
        }
    }

    public bool CheckEventOnce(LevelEvent levelEvent)
    {
        bool allConditionsMet = true;
        for (int j = 0; j < levelEvent.conditions.Count; j++)
        {
            var condition = levelEvent.conditions[j];
            allConditionsMet = IsConditionMet(condition);
                
            if (!allConditionsMet)
                break;
        }

        return allConditionsMet;
    }

    public bool IsConditionMet(Condition condition)
    {
        if (condition.transformA == null || condition.transformB == null)
        {
            for (int i = 0; i < levelActors.Count; i++)
            {
                if (levelActors[i].actorId == condition.actorIdA)
                {
                    condition.transformA = levelActors[i].transform;
                    continue;
                }
                        
                if (levelActors[i].actorId == condition.actorIdB)
                {
                    condition.transformB = levelActors[i].transform;
                }
            } 
        }

        bool met = true;
        
        switch (condition.conditionType)
        {
            case Condition.ConditionType.DistanceIsBigger:
                
                if (condition.transformA == null || condition.transformB == null)
                {
                    Debug.LogWarning("Distance comparing. One of transforms is missing. transformA = " + condition.transformA +"; transformB = " + condition.transformB);
                    met = false;
                    break;
                }
                if (Vector3.Distance(condition.transformA.position, condition.transformB.position) <= condition.distanceToCompare)
                {
                    met = false;
                }
                break;
            case Condition.ConditionType.DistanceIsSmaller:
                if (condition.transformA == null || condition.transformB == null)
                {
                    Debug.LogWarning("Distance comparing. One of transforms is missing. transformA = " + condition.transformA +"; transformB = " + condition.transformB);
                    met = false;
                    break;
                }
                if (Vector3.Distance(condition.transformA.position, condition.transformB.position) >= condition.distanceToCompare)
                {
                    met = false;
                }
                break;
            case Condition.ConditionType.PlayerIsDead:
                met =  Game.Player.Health.IsDead;
                break;
            case Condition.ConditionType.PlayerIsDriving:
                met =  Game.Player.VehicleControls.controlledMachine;
                break;
        }
        
        return met;
    }
}