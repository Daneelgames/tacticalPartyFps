using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using MrPink.Tools;
using MrPink.WeaponsSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _src.Scripts.Data
{
    [CreateAssetMenu(fileName = "LevelEvents", menuName = "ScriptableObjects/LevelEvents", order = 1)]
    public class LevelEvents : ScriptableObject
    {
        public List<ScriptedEvent> eventsList;
    }

    [Serializable]
    public class ScriptedEvent
    {
        public ScriptedEventType scriptedEventType = ScriptedEventType.SpawnObject;

        [ShowIf("scriptedEventType", ScriptedEventType.AddScore)]
        public int scoreToAdd;
        
        
        [ShowIf("scriptedEventType", ScriptedEventType.SpawnObject)]     
        [Tooltip("If no spawnInsideCam and noCustomSpawnPOint, spawns at")]
        public GameObject prefabToSpawn;
        [ShowIf("scriptedEventType", ScriptedEventType.SpawnObject)]
        [Tooltip("Spawns object inside player's camera at zero local coordinates")]
        public bool spawnInsideCamera = false;
        [ShowIf("scriptedEventType", ScriptedEventType.SpawnObject)]
        public Transform customSpawnPoint;

        
        [ShowIf("scriptedEventType", ScriptedEventType.StartDialogue)]
        public Dialogue dialogueToStart;

        [ShowIf("scriptedEventType", ScriptedEventType.StartDialogue)]
        public InteractiveObject destroyInteractorAfterDialogueCompleted;
        [ShowIf("scriptedEventType", ScriptedEventType.StartDialogue)]
        public int scoreToAddOnDialogueCompleted = 0;
        [ShowIf("scriptedEventType", ScriptedEventType.StartDialogue)]
        public bool setNextLevelOnDialogueCompleted = false;

        [ShowIf("scriptedEventType", ScriptedEventType.StartDialogue)]
        public float maxDistanceToSpeaker = -1;
        


        
        [ShowIf("scriptedEventType", ScriptedEventType.SetCurrentLevel)]
        public int currentLevelToSet;

        
        [ShowIf("scriptedEventType", ScriptedEventType.PlaySound)]
        public AudioClip soundToPlay;
        [ShowIf("scriptedEventType", ScriptedEventType.PlaySound)]
        public Vector2 auPitchMinMax = new Vector2(1, 1);

        [ShowIf("scriptedEventType", ScriptedEventType.RideVehicle)]
        public ControlledMachine controlledMachine;

        [ShowIf("scriptedEventType", ScriptedEventType.AddTool)]
        public Tool toolToAdd;
        [ShowIf("scriptedEventType", ScriptedEventType.AddWeapon)]
        public WeaponController weaponToAdd;
        public HealthController ActorNpc;
        public int addToStatAmount = 0;

        [Header("If Id >= 0, game will try to find NpcHc by Id")]
        public int actorId = -1;
        
    }
    
    public enum ScriptedEventType
    {
        StartDialogue, SpawnObject, DestroyOnInteraction, StartProcScene, StartFlatScene, SetCurrentLevel, AddScore, PlaySound, RideVehicle, AddTool, AddWeapon,
        AddHealth, AddToFood, AddWater, AddSleep
    }
}