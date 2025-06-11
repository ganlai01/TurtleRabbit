using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public class TreePlacementManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button toggleRaycastButton;
    [SerializeField] private Button placeTreeButton;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private TMP_Text instructionText;

    [Header("Tree Settings")]
    [SerializeField] private GameObject treePrefab;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem raycastParticleEffect;
    [SerializeField] private Image buttonGlowEffect;
    [SerializeField] private Color activeButtonColor = Color.green;
    [SerializeField] private Color inactiveButtonColor = Color.white;

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
    private bool treePlacementMode = false;
    private bool isPlacingTree = false;
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

        // Setup button
        toggleRaycastButton.onClick.AddListener(ToggleTreePlacementMode);
        placeTreeButton.onClick.AddListener(OnPlaceTreeButtonClicked);

        // Initialize UI
        UpdateUI();

        // Find AR components if not assigned
        if (raycastManager == null)
        {
            raycastManager = FindObjectOfType<ARRaycastManager>();
        }

        // Initialize visual effects
        if (raycastParticleEffect != null)
        {
            raycastParticleEffect.gameObject.SetActive(false);
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

        // Only handle tree placement if we're in tree placement mode AND actively placing
        if (treePlacementMode && isPlacingTree)
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                UpdateVisualEffects(touch.position);

                // Only place tree on touch end to avoid immediate placement
                if (touch.phase == TouchPhase.Ended)
                {
                    if (!IsClickOnUI(touch.position))
                    {
                        TryPlaceTree(touch.position);
                        isPlacingTree = false; // Reset after placement
                        Debug.Log("Tree placement completed via touch");
                    }
                    else
                    {
                        Debug.Log("Touch was on UI, not placing tree");
                    }
                }
            }
        }
    }

    public void ToggleTreePlacementMode()
    {
        treePlacementMode = !treePlacementMode;
        buttonClickTime = Time.time; // Record when button was clicked
        isPlacingTree = false;

        UpdateUI();
        UpdateButtonVisualEffects();

        FieldOfViewController.GlobalFOVEnabled = treePlacementMode;
        Debug.Log($"Global FOV enabled: {treePlacementMode}");

        // Notify SpawnController about our state
        if (spawnController != null)
        {
            spawnController.SetTreePlacementActive(treePlacementMode);
        }

        Debug.Log($"Tree placement mode: {(treePlacementMode ? "ON" : "OFF")}, Ready to place: {isPlacingTree}");
    }

    private void UpdateButtonVisualEffects()
    {
        // Update button color/glow effect
        if (buttonGlowEffect != null)
        {
            buttonGlowEffect.color = treePlacementMode ? activeButtonColor : inactiveButtonColor;
        }

        // Update button image color
        Image buttonImage = toggleRaycastButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = treePlacementMode ? activeButtonColor : inactiveButtonColor;
        }

        // Particle effect for active mode
        if (raycastParticleEffect != null)
        {
            if (treePlacementMode && isPlacingTree)
            {
                raycastParticleEffect.gameObject.SetActive(true);
                if (!raycastParticleEffect.isPlaying)
                {
                    raycastParticleEffect.Play();
                }
            }
            else
            {
                raycastParticleEffect.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateVisualEffects(Vector2 screenPosition)
    {
        if (raycastManager.Raycast(screenPosition, raycastHits, TrackableType.Planes))
        {
            Pose hitPose = raycastHits[0].pose;

            // Update particle effect position
            if (raycastParticleEffect != null)
            {
                raycastParticleEffect.transform.position = hitPose.position;
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
                buttonClickTime = Time.time; // Update button click time
                Debug.Log($"Button clicked detected: {result.gameObject.name}");
                return true;
            }
        }

        return results.Count > 0;
    }

    private void UpdateUI()
    {
        if (buttonText != null)
        {
            buttonText.text = treePlacementMode ? "Raycast Off" : "Raycast On";
        }

        if (instructionText != null)
        {
            if (!treePlacementMode)
            {
                instructionText.text = "Turn on Raycast to place trees\nfor sleeping hare";
            }
            else if (treePlacementMode && !isPlacingTree)
            {
                instructionText.text = "Click 'Place Tree' button,\nthen tap on a plane to place tree";
            }
            else
            {
                instructionText.text = "Tap on a plane to place the tree";
            }
        }
    }

    public void OnPlaceTreeButtonClicked()
    {
        buttonClickTime = Time.time; // Record button click time

        if (!treePlacementMode)
        {
            // If raycast mode is off, turn it on first
            ToggleTreePlacementMode();
        }

        // Now enable tree placement
        isPlacingTree = true;
        UpdateUI();
        UpdateButtonVisualEffects();

        Debug.Log("Place Tree button clicked - Ready to place tree");
    }

    private void TryPlaceTree(Vector2 screenPosition)
    {
        if (raycastManager.Raycast(screenPosition, raycastHits, TrackableType.Planes))
        {
            Pose hitPose = raycastHits[0].pose;

            if (treePrefab != null)
            {
                GameObject tree = Instantiate(treePrefab, hitPose.position, Quaternion.identity);

                // Ensure tree has proper tag and name
                tree.tag = "Tree";
                tree.name = "PlacedTree";

                // Add collider with proper size
                if (tree.GetComponent<Collider>() == null)
                {
                    BoxCollider collider = tree.AddComponent<BoxCollider>();
                    // Set collider size (adjust based on your tree model)
                    collider.size = new Vector3(2f, 2f, 2f);
                    collider.isTrigger = false;
                }

                Debug.Log($"Tree placed at: {hitPose.position} with tag: {tree.tag}");
            }
            else
            {
                Debug.LogWarning("Tree prefab is not set!");
            }
        }
        else
        {
            Debug.LogWarning("No AR plane found at the tap position.");
        }
    }
}