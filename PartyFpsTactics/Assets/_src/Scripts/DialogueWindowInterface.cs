using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using MrPink.PlayerSystem;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DialogueWindowInterface : MonoBehaviour
{
    public static DialogueWindowInterface Instance;
    public GameObject phoneVisual;
    public GameObject logoEye;
    public GameObject playerAnswerButtons;
    
    public Text nameText;
    public Text messageText;
    public Text playerAnswerText;

    public Transform phoneInactiveTransform;
    public Transform phoneActiveTransform;

    public bool dialogueWindowActive = false;

    public AudioSource phoneAu;
    public AudioClip messageNotificationClip;
    public AudioClip auClipYes;
    public AudioClip auClipNo;

    private void Start()
    {
        Instance = this;
        TogglePlayerAnswerButtons(false);
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!dialogueWindowActive)
                NewMessage(String.Empty, String.Empty, true);
            ToggleDialogueWindow(!dialogueWindowActive);
        }
        if (!dialogueWindowActive)
            return;
        
        if (Input.GetKeyDown(KeyCode.E))
            PlayerAnswered(true);
        if (Input.GetKeyDown(KeyCode.Q))
            PlayerAnswered(false);
    }

    public void TogglePlayerAnswerButtons(bool active)
    {
        playerAnswerButtons.SetActive(active);
    }
    public void NewMessage(string _nameText, string _messageText, bool clearPlayerAnswer)
    {
        nameText.text = _nameText;
        messageText.text = _messageText;
        phoneAu.clip = messageNotificationClip;
        phoneAu.Play();
        
        if (clearPlayerAnswer)
        {
            playerAnswerText.text = "";
            playerAnswerText.color = Color.white;
        }
    }

    private void PlayerAnswered(bool positiveAnswer)
    {
        if (!ProceduralCutscenesManager.Instance.CanAnswer)
            return;
        
        if (positiveAnswer)
        {
            phoneAu.clip = auClipYes;
            playerAnswerText.text = ":-)";
            playerAnswerText.color = new Color(0.86f, 0.37f, 0.57f);
        }
        else
        {
            phoneAu.clip = auClipNo;
            playerAnswerText.text = ":-(";
            playerAnswerText.color = new Color(0.38f, 0.42f, 0.58f);
        }

        phoneAu.pitch = Random.Range(0.9f, 1.1f);
        phoneAu.Play();
        ProceduralCutscenesManager.Instance.PlayerAnswered(positiveAnswer);
    }

    public void ToggleDialogueWindow(bool active)
    {
        if (toggleDialogueWIndowCoroutine != null)
            StopCoroutine(toggleDialogueWIndowCoroutine);

        if (!active)
        {
            if (CheckDistanceToSpeakerCoroutine != null)
                StopCoroutine(CheckDistanceToSpeakerCoroutine);
        }

        toggleDialogueWIndowCoroutine = StartCoroutine(ToggleDialogueWindowIEnumerator(active));
    }

    public void StartCheckingDistanceToSpeaker(HealthController hc, float maxDistance)
    {
        if (CheckDistanceToSpeakerCoroutine != null)
            StopCoroutine(CheckDistanceToSpeakerCoroutine);
        
        CheckDistanceToSpeakerCoroutine = StartCoroutine(CheckDistanceToSpeaker(hc, maxDistance));
    }

    private Coroutine CheckDistanceToSpeakerCoroutine;
    IEnumerator CheckDistanceToSpeaker(HealthController hc, float maxDistance)
    {
        while (true)
        {
            if (Vector3.Distance(Player.Movement.transform.position, hc.npcInteraction.transform.position) > maxDistance)
            {
                ToggleDialogueWindow(false);
                yield break;
            }
            yield return null;
        }
    }

    private Coroutine toggleDialogueWIndowCoroutine;
    IEnumerator ToggleDialogueWindowIEnumerator(bool active)
    {
        Debug.Log("5");
        Transform targetTransform = null;
        targetTransform = active ? phoneActiveTransform : phoneInactiveTransform;

        dialogueWindowActive = active;
        float t = 0;

        if (active)
        {
            logoEye.SetActive(false);
            phoneVisual.SetActive(true);
            yield return null;
            logoEye.SetActive(true);
            
        }
        
        while (t < 1)
        {
            yield return null;
            phoneVisual.transform.position = Vector3.Lerp(phoneVisual.transform.position, targetTransform.position, t / 1);
            phoneVisual.transform.rotation = Quaternion.Slerp(phoneVisual.transform.rotation, targetTransform.rotation, t / 1);
            t += Time.deltaTime;
        }
        
        if (!active)
        {
            phoneVisual.SetActive(false);
            PlayerAnswered(false);
            ProceduralCutscenesManager.Instance.CloseCutscene();
        }
        
        toggleDialogueWIndowCoroutine = null;
    }
}