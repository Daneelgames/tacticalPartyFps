using System.Collections;
using System.Collections.Generic;
using MrPink;
using MrPink.PlayerSystem;
using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    private Transform rotateVisual;
    private bool collected = false;
    void Start()
    {
        rotateVisual = transform.GetChild(0);
    }

    // Update is called once per frame
    void Update()
    {
        rotateVisual.transform.localEulerAngles += Vector3.up * 100 * Time.deltaTime;
    }

    void OnTriggerEnter(Collider coll)
    {
        if (collected)
            return;
        
        if (coll.gameObject == Game.Player.GameObject)
        {
            collected = true;
            ProgressionManager.Instance.SetCurrentLevel(ProgressionManager.Instance.currentLevelIndex + 1);
            GameManager.Instance.StartProcScene();
        }
    }
}