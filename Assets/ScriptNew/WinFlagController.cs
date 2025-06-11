using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public class WinFlagController : MonoBehaviour
{
    [Header("UI References")]
    // [SerializeField] private Button toggleFlagRaycastButton;
    [SerializeField] private Button placeFlagButton;
    [SerializeField] private TMP_Text flagButtonText;
    [SerializeField] private TMP_Text flagInstructionText;

    [Header("Flag Settings")]
    [SerializeField] private GameObject flagPrefab;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem flagRaycastParticleEffect;
    [SerializeField] private Image flagButtonGlowEffect;
    [SerializeField] private Color activeFlagButtonColor = Color.blue;
    [SerializeField] private Color inactiveFlagButtonColor = Color.white;

    [Header("AR Components")]
    [SerializeField] private ARRaycastManager raycastManager;

    [Header("Input Settings")]
    [SerializeField]
    XRInputValueReader<Vector2> m_TapStartPositionInput = new XRInputValueReader<Vector2>("Tap Start Position");

    public XRInputValueReader<Vector2> tapStartPositionInput
    {
        get => m_TapStartPositionInput;
        set => XRInputReaderUtility.SetInputProperty(ref m_TapStartPositionInput, value, this);
    }

    // State management
    private bool flagPlacementMode = false;
    private bool isPlacingFlag = false;
    private Vector2 previousTapPosition;
    private static List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();
    private SpawnController spawnController;
    private float buttonClickTime = 0f;
    private const float BUTTON_COOLDOWN = 0.5f;
    private GraphicRaycaster graphicRaycaster;
    private EventSystem eventSystem;

    private void Start()
    {
        // Get reference to SpawnController
        spawnController = FindObjectOfType<SpawnController>();

        // Get UI components for button detection
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
        }
        eventSystem = EventSystem.current;

        // Setup buttons
        placeFlagButton.onClick.AddListener(OnPlaceFlagButtonClicked);

        // Initialize UI
        UpdateUI();

        // Find AR components if not assigned
        if (raycastManager == null)
        {
            raycastManager = FindObjectOfType<ARRaycastManager>();
        }

        // Initialize visual effects
        if (flagRaycastParticleEffect != null)
        {
            flagRaycastParticleEffect.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        tapStartPositionInput.EnableDirectActionIfModeUsed();
    }

    private void OnDisable()
    {
        tapStartPositionInput.DisableDirectActionIfModeUsed();
    }

    private void Update()
    {
        // Don't process any input immediately after button clicks
        if (Time.time - buttonClickTime < BUTTON_COOLDOWN)
        {
            return;
        }

        // Only handle flag placement if we're in flag placement mode AND actively placing
        if (flagPlacementMode && isPlacingFlag)
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                UpdateVisualEffects(touch.position);

                // Only place flag on touch end
                if (touch.phase == TouchPhase.Ended)
                {
                    if (!IsClickOnUI(touch.position))
                    {
                        TryPlaceFlag(touch.position);
                        isPlacingFlag = false;
                        Debug.Log("Flag placement completed via touch");
                    }
                    else
                    {
                        Debug.Log("Touch was on UI, not placing flag");
                    }
                }
            }
        }
    }

    public void ToggleFlagPlacementMode()
    {
        flagPlacementMode = !flagPlacementMode;
        buttonClickTime = Time.time;
        isPlacingFlag = false;
        
        UpdateUI();
        UpdateButtonVisualEffects();

        // Notify SpawnController about our state
        if (spawnController != null)
        {
            spawnController.SetFlagPlacementActive(flagPlacementMode);
        }

        Debug.Log($"Flag placement mode: {(flagPlacementMode ? "ON" : "OFF")}");
    }

    private void UpdateButtonVisualEffects()
    {
        // Update button color/glow effect
        if (flagButtonGlowEffect != null)
        {
            flagButtonGlowEffect.color = flagPlacementMode ? activeFlagButtonColor : inactiveFlagButtonColor;
        }

        // Particle effect for active mode
        if (flagRaycastParticleEffect != null)
        {
            if (flagPlacementMode && isPlacingFlag)
            {
                flagRaycastParticleEffect.gameObject.SetActive(true);
                if (!flagRaycastParticleEffect.isPlaying)
                {
                    flagRaycastParticleEffect.Play();
                }
            }
            else
            {
                flagRaycastParticleEffect.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateVisualEffects(Vector2 screenPosition)
    {
        if (raycastManager.Raycast(screenPosition, raycastHits, TrackableType.Planes))
        {
            Pose hitPose = raycastHits[0].pose;
            
            // Update particle effect position
            if (flagRaycastParticleEffect != null)
            {
                flagRaycastParticleEffect.transform.position = hitPose.position;
            }
        }
    }

    private bool IsClickOnUI(Vector2 screenPosition)
    {
        if (graphicRaycaster == null || eventSystem == null)
            return false;

        PointerEventData pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = screenPosition;

        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponent<Button>() != null)
            {
                buttonClickTime = Time.time;
                Debug.Log($"Button clicked detected: {result.gameObject.name}");
                return true;
            }
        }

        return results.Count > 0;
    }

    private void UpdateUI()
    {
        if (flagButtonText != null)
        {
            flagButtonText.text = flagPlacementMode ? "Flag Raycast Off" : "Flag Raycast On";
        }

        placeFlagButton.interactable = true;

        if (flagInstructionText != null)
        {
            if (!flagPlacementMode)
            {
                flagInstructionText.text = "Turn on Flag Raycast to place\nwin flag for turtle";
            }
            else if (flagPlacementMode && !isPlacingFlag)
            {
                flagInstructionText.text = "Click 'Place Flag' button,\nthen tap on a plane to place flag";
            }
            else
            {
                flagInstructionText.text = "Tap on a plane to place the win flag";
            }
        }
    }

    public void OnPlaceFlagButtonClicked()
    {
        buttonClickTime = Time.time;
        
        if (!flagPlacementMode)
        {
            ToggleFlagPlacementMode();
        }
        
        isPlacingFlag = true;
        UpdateUI();
        UpdateButtonVisualEffects();
        
        Debug.Log("Place Flag button clicked - Ready to place flag");
    }

    private void TryPlaceFlag(Vector2 screenPosition)
    {
        if (raycastManager.Raycast(screenPosition, raycastHits, TrackableType.Planes))
        {
            Pose hitPose = raycastHits[0].pose;

            if (flagPrefab != null)
            {
                GameObject flag = Instantiate(flagPrefab, hitPose.position, Quaternion.identity);

                // Ensure flag has proper tag and name
                flag.tag = "WinFlag";
                flag.name = "PlacedWinFlag";

                // Add collider with proper size
                if (flag.GetComponent<Collider>() == null)
                {
                    BoxCollider collider = flag.AddComponent<BoxCollider>();
                    collider.size = new Vector3(2f, 2f, 2f);
                    collider.isTrigger = false;
                }

                Debug.Log($"Win flag placed at: {hitPose.position} with tag: {flag.tag}");
            }
            else
            {
                Debug.LogWarning("Flag prefab is not set!");
            }
        }
        else
        {
            Debug.LogWarning("No AR plane found at the tap position.");
        }
    }
}