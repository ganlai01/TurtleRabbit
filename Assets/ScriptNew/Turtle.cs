using System.Collections;
using UnityEngine;
using TMPro;

public class Turtle : MonoBehaviour
{
    [Header("References")]
    public TMP_Text statusText;

    [Header("Conversation Settings")]
    public float lineDuration = 2f;
    public float postConversationDelay = 1f;

    [Header("Race Settings")]
    public float moveSpeed = 1f;
    public float firstIdleDuration = 2f;
    public float firstRunForwardDuration = 3f;
    public float secondIdleDuration = 2f;
    public float secondRunForwardDuration = 3f;
    public float finalIdleDuration = 2f;

    [Header("Win Flag Detection")]
    public float detectionRadius = 2f;
    public LayerMask flagLayerMask = -1;
    public float proximityCheckInterval = 0.5f;

    private string[] turtleLines = {
        "Good morning, Hare. I may be slow, but I always reach where I'm going.",
        "Speed isn't everything, my friend. Sometimes steady wins the day.",
        "Very well, Hare. If you insist. But don't say I didn't warn you."
    };

    private Animator animator;
    public float timer;
    public int currentState = -1;
    private bool conversationComplete = false;
    public bool raceStarted = false;
    private bool hasWon = false;
    private Coroutine proximityCheckCoroutine;
    public FieldOfViewController fovController;


    private void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(ConversationRoutine());
    }

    public void ToggleFieldOfView()
    {
        if (fovController != null)
            fovController.ToggleFOV();
    }

    private IEnumerator ConversationRoutine()
    {
        currentState = -1;
        SetIdle();

        // Wait for hare's first line
        yield return new WaitForSeconds(lineDuration);

        // Line 1
        UpdateStatusText(turtleLines[0]);
        yield return new WaitForSeconds(lineDuration);
        yield return new WaitForSeconds(lineDuration); // Wait for hare's animation

        // Line 2
        UpdateStatusText(turtleLines[1]);
        yield return new WaitForSeconds(lineDuration);
        yield return new WaitForSeconds(lineDuration);

        // Line 3
        UpdateStatusText(turtleLines[2]);
        yield return new WaitForSeconds(lineDuration);

        // Transition to race
        UpdateStatusText("");
        yield return new WaitForSeconds(postConversationDelay);

        conversationComplete = true;
        StartRace();
    }

    public void StartRace()
    {
        raceStarted = true;
        currentState = 0;
        timer = 0f;
        UpdateStatusText("I have to cheer up!");

        // Start proximity checking for win flag
        if (proximityCheckCoroutine != null)
        {
            StopCoroutine(proximityCheckCoroutine);
        }
        proximityCheckCoroutine = StartCoroutine(CheckWinFlagProximityRoutine());
    }

    private void Update()
    {
        if (!raceStarted || hasWon) return;

        timer += Time.deltaTime;

        switch (currentState)
        {
            case 0: // First idle
                if (timer >= firstIdleDuration)
                {
                    TransitionToNextState();
                    SetRunForward();
                    UpdateStatusText("I can't give up!");
                }
                break;

            case 1: // First run forward
                MoveForward();
                if (timer >= firstRunForwardDuration)
                {
                    TransitionToNextState();
                    SetIdle();
                    UpdateStatusText("Why is it sleeping here?");
                }
                break;

            case 2: // Second idle
                if (timer >= secondIdleDuration)
                {
                    TransitionToNextState();
                    SetRunForward();
                    UpdateStatusText("The finish line is just ahead, come on!");
                }
                break;

            case 3: // Second run forward
                MoveForward();
                if (timer >= secondRunForwardDuration)
                {
                    TransitionToNextState();
                    SetIdle();
                    UpdateStatusText("I win!!!");
                }
                break;

            case 4: // Final idle
                if (timer >= finalIdleDuration)
                {
                    timer = 0f;
                }
                break;
        }
    }

    private IEnumerator CheckWinFlagProximityRoutine()
    {
        while (raceStarted && !hasWon)
        {
            CheckForWinFlag();
            yield return new WaitForSeconds(proximityCheckInterval);
        }
    }

    private void CheckForWinFlag()
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, detectionRadius, flagLayerMask);

        Debug.Log($"Turtle checking for win flag - Found {nearbyObjects.Length} objects within {detectionRadius} units");

        foreach (Collider obj in nearbyObjects)
        {
            Debug.Log($"Found object: {obj.name}, Tag: {obj.tag}");

            if (obj.CompareTag("WinFlag") || obj.name.Contains("Flag"))
            {
                Debug.Log($"Win flag detected! Turtle wins! Object: {obj.name}");
                TriggerWin();
                return;
            }
        }
    }

    private void TriggerWin()
    {
        if (hasWon) return;

        hasWon = true;
        currentState = 4; // Jump to final idle state
        timer = 0f;
        SetIdle();
        UpdateStatusText("I win!!! I reached the flag!");

        Debug.Log("Turtle has won the race!");
    }

    private void TransitionToNextState()
    {
        currentState++;
        timer = 0f;
    }

    public void SetIdle()
    {
        if (animator != null)
        {
            animator.SetBool("IsRunning", false);
        }
    }

    private void SetRunForward()
    {
        if (animator != null)
        {
            animator.SetBool("IsRunning", true);
        }
    }

    public void MoveForward()
    {
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }

    public void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    public void RestartSequence()
    {
        StopAllCoroutines();
        currentState = -1;
        conversationComplete = false;
        raceStarted = false;
        hasWon = false;

        if (proximityCheckCoroutine != null)
        {
            StopCoroutine(proximityCheckCoroutine);
            proximityCheckCoroutine = null;
        }

        StartCoroutine(ConversationRoutine());
    }
}