using System.Collections;
using UnityEngine;
using TMPro;

public class Rabbit : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public TMP_Text tMP_Text;
    public Turtle turtle;

    [Header("Field of View")]
    public float viewDistance = 5f;
    public float viewAngle = 60f;
    public int rayCount = 20;
    public bool showFieldOfView = true;
    public Color fovColor = Color.red;

    [Header("Conversation Settings")]
    public float lineDuration = 2f;
    public float postConversationDelay = 1f;

    [Header("Race Timings")]
    public float firstIdleDuration = 1f;
    public float firstRunDuration = 3f;
    public float firstDeathDuration = 5f;
    public float secondIdleDuration = 1f;
    public float secondRunDuration = 3f;
    public float thirdIdleDuration = 1f;

    [Header("Proximity Settings")]
    public float detectionRadius = 0.1f;
    public LayerMask interactableLayerMask = -1;
    public float proximityCheckInterval = 2f;


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
    private bool isNearInteractable = false;
    private bool isSleepingFromProximity = false;
    private bool hasSleptFromProximity = false;
    private Coroutine proximityCheckCoroutine;
    public FieldOfViewController fovController;

    private void Start()
    {
        StartCoroutine(StartConversation());
    }

    public void ToggleFieldOfView()
    {
        if (fovController != null)
            fovController.ToggleFOV();
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

        if (proximityCheckCoroutine != null)
        {
            StopCoroutine(proximityCheckCoroutine);
        }
        proximityCheckCoroutine = StartCoroutine(CheckProximityRoutine());
    }

    private void Update()
    {
        if (!isSequenceActive || !raceStarted)
        {
            return;
        }
        if (isSleepingFromProximity) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            sequenceStep++;

            switch (sequenceStep)
            {
                case 1: // First Run
                    SetRunning(true);
                    timer = firstRunDuration;
                    UpdateStatusText("I'm ahead, I can take a break. Let's sleep under the tree.");
                    break;

                case 2: // First Death
                    if (!hasSleptFromProximity && !isSleepingFromProximity)
                    {
                        TriggerDeath();
                        timer = firstDeathDuration;
                        UpdateStatusText("Zzzzz....");
                    }
                    else
                    {
                        // Skip this step if we've already slept from proximity
                        sequenceStep++;
                        timer = secondIdleDuration;
                        UpdateStatusText("Oh No! I slept late, I'm so far behind.");
                        SetIdle(true);
                    }
                    break;

                case 3: // Second Idle
                    ResetAllStates();
                    SetIdle(true);
                    timer = secondIdleDuration;
                    UpdateStatusText("I need to hurry up.");
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

            // Set animation time to 5 seconds
            animator.SetFloat("DeathAnimationSpeed", animator.GetCurrentAnimatorStateInfo(0).length / 10f);
        }
    }

    private IEnumerator CheckProximityRoutine()
    {
        while (raceStarted && isSequenceActive)
        {
            CheckForNearbyInteractables();
            yield return new WaitForSeconds(proximityCheckInterval);
        }
    }

    private void CheckForNearbyInteractables()
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, detectionRadius, interactableLayerMask);

        bool foundInteractable = false;

        foreach (Collider obj in nearbyObjects)
        {
            if (obj.CompareTag("Tree") || obj.name.Contains("Tree"))
            {
                foundInteractable = true;
                break;
            }
        }

        if (foundInteractable && !isNearInteractable && !isSleepingFromProximity)
        {
            isNearInteractable = true;
            TriggerProximitySleep();
        }
        else if (!foundInteractable && isNearInteractable)
        {
            isNearInteractable = false;
        }
    }

    private void TriggerProximitySleep()
    {
        if (isSleepingFromProximity) return;

        isSleepingFromProximity = true;

        TriggerDeath();
        UpdateStatusText("Wow! A tree! Let me rest for a while... Zzzzz");

        StartCoroutine(ProximitySleepRoutine());
    }

    private IEnumerator ProximitySleepRoutine()
    {
        // Sleep duration (longer than normal sleep)
        yield return new WaitForSeconds(36f);

        if (isSleepingFromProximity)
        {
            yield return new WaitForSeconds(3f);
            // Wake up
            ResetAllStates();
            SetIdle(true);
            UpdateStatusText("Oh no! I overslept!");
            isSleepingFromProximity = false;
            hasSleptFromProximity = true;

            yield return new WaitForSeconds(5f);
        }
    }

    public void SetFieldOfViewColor(Color color)
    {
        fovColor = color;
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
        isNearInteractable = false;
        isSleepingFromProximity = false;
        hasSleptFromProximity = false;

        if (proximityCheckCoroutine != null)
        {
            StopCoroutine(proximityCheckCoroutine);
            proximityCheckCoroutine = null;
        }

        StartCoroutine(StartConversation());
    }
}