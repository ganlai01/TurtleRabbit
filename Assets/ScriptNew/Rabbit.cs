using System.Collections;
using UnityEngine;
using TMPro;

public class Rabbit : MonoBehaviour
{   
    [Header("References")]
    public Animator animator;
    public TMP_Text tMP_Text;
    public Turtle turtle;
    
    [Header("Conversation Settings")]
    public float lineDuration = 2f;
    public float postConversationDelay = 1f;
    
    [Header("Race Timings")]
    public float firstIdleDuration = 1f;
    public float firstRunDuration = 3f;
    public float firstDeathDuration = 2f;
    public float secondIdleDuration = 1f;
    public float secondRunDuration = 3f;
    public float thirdIdleDuration = 1f;
    
    private string[] hareLines = {
        "Well, well, if it isn't old Slowpoke! Still taking forever to cross the meadow, I see.",
        "Always reach where you're going? Ha! By the time you get anywhere, the seasons have changed! Watch this—",
        "Steady? STEADY?! That's rich coming from someone who takes coffee breaks just to blink!",
        "Warn me? The only warning here is for you to pack a lunch—this race might take you all week!"
    };
    
    private float timer;
    private int sequenceStep = 0;
    private bool isSequenceActive = true;
    private bool conversationComplete = false;
    private bool raceStarted = false;

    private void Start()
    {
        StartCoroutine(StartConversation());
    }

    private IEnumerator StartConversation()
    {
        sequenceStep = -1;
        isSequenceActive = true;
        SetIdle(true);
        
        // Line 1
        UpdateStatusText(hareLines[0]);
        yield return new WaitForSeconds(lineDuration);
        yield return new WaitForSeconds(lineDuration * 2); // Wait for turtle response
        
        // Line 2 with animation
        UpdateStatusText(hareLines[1]);
        yield return new WaitForSeconds(lineDuration);
        SetRunning(true);
        yield return new WaitForSeconds(0.5f);
        SetIdle(true);
        yield return new WaitForSeconds(lineDuration * 2);
        
        // Line 3
        UpdateStatusText(hareLines[2]);
        yield return new WaitForSeconds(lineDuration);
        yield return new WaitForSeconds(lineDuration * 2);
        
        // Line 4
        UpdateStatusText(hareLines[3]);
        yield return new WaitForSeconds(lineDuration);
        
        // Transition to race
        UpdateStatusText("");
        yield return new WaitForSeconds(postConversationDelay);
        
        conversationComplete = true;
        StartRace();
    }

    private void StartRace()
    {
        raceStarted = true;
        sequenceStep = 0;
        timer = firstIdleDuration;
        SetIdle(true);
        UpdateStatusText("HAHA, I'm the fastest here.");
    }

    private void Update()
    {
        if (!isSequenceActive || !raceStarted) return;
        
        timer -= Time.deltaTime;
        
        if (timer <= 0f)
        {
            sequenceStep++;
            
            switch (sequenceStep)
            {
                case 1: // First Run
                    SetRunning(true);
                    timer = firstRunDuration;
                    UpdateStatusText("I'm ahead, I can take a break");
                    break;
                    
                case 2: // First Death
                    TriggerDeath();
                    timer = firstDeathDuration;
                    UpdateStatusText("Zzzzz....");
                    break;
                    
                case 3: // Second Idle
                    ResetAllStates();
                    SetIdle(true);
                    timer = secondIdleDuration;
                    UpdateStatusText("Oh No! I slept late, I'm so far behind.");
                    break;
                    
                case 4: // Second Run
                    SetRunning(true);
                    timer = secondRunDuration;
                    break;
                    
                case 5: // Third Idle
                    SetIdle(true);
                    timer = thirdIdleDuration;
                    UpdateStatusText("");
                    break;
                    
                default:
                    isSequenceActive = false;
                    break;
            }
        }
    }

    public void SetIdle(bool isIdle)
    {
        if (animator != null)
        {
            animator.applyRootMotion = true;
            animator.SetBool("Idle", isIdle);
            if (isIdle) animator.SetBool("Run", false);
        }
    }

    public void SetRunning(bool isRunning)
    {
        if (animator != null)
        {
            animator.applyRootMotion = true;
            animator.SetBool("Run", isRunning);
            if (isRunning) animator.SetBool("Idle", false);
        }
    }

    public void TriggerDeath()
    {
        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.SetTrigger("Dead");
            animator.SetBool("Idle", false);
            animator.SetBool("Run", false);
        }
    }

    private void UpdateStatusText(string message)
    {
        if (tMP_Text != null)
        {
            tMP_Text.text = message;
        }
    }

    public void ResetAllStates()
    {
        if (animator != null)
        {
            animator.ResetTrigger("Dead");
            animator.SetBool("Idle", false);
            animator.SetBool("Run", false);
        }
    }

    public void RestartSequence()
    {
        StopAllCoroutines();
        ResetAllStates();
        sequenceStep = 0;
        conversationComplete = false;
        raceStarted = false;
        StartCoroutine(StartConversation());
    }
}