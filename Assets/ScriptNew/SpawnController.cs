using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SpawnController : MonoBehaviour
{
    [Header("Rabbit Settings")]
    [SerializeField] private GameObject rabbitPrefab;
    [SerializeField] private Vector3 rabbitSpawnPoint;
    public GameObject rabbit;

    [Header("Turtle Settings")]
    [SerializeField] private GameObject turtlePrefab;
    [SerializeField] private Vector3 turtleSpawnPoint;
    public GameObject turtle;

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

    public bool animalSpawn = false;
    private bool treePlacementActive = false;
    private bool flagPlacementActive = false;

    private Vector2 previousTapPosition;
    private static List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();
    private GraphicRaycaster graphicRaycaster;
    private EventSystem eventSystem;
    // public static event Action<Bird> OnAnimalSpawned;

    private void OnEnable()
    {
        tapStartPositionInput.EnableDirectActionIfModeUsed();
    }

    private void OnDisable()
    {
        tapStartPositionInput.DisableDirectActionIfModeUsed();
    }
    void Update()
    {

        if (animalSpawn)
            return;

        if (treePlacementActive || flagPlacementActive) // Add flagPlacementActive check
            return;

        // Detect if tap position changed (i.e., a new tap happened)
        if (tapStartPositionInput.TryReadValue(out var tapPosition) && tapPosition != previousTapPosition)
        {
            previousTapPosition = tapPosition;

            if (!IsClickOnUI(tapPosition))
            {
                TrySpawnBirdAt(tapPosition);
            }
            else
            {
                Debug.Log("Clicked on UI, ignoring animal spawn");
            }
        }
    }

    private void TrySpawnBirdAt(Vector2 screenPosition)
    {
        if (raycastManager.Raycast(screenPosition, raycastHits, TrackableType.Planes))
        {
            Pose hitPose = raycastHits[0].pose;
            rabbit = Instantiate(rabbitPrefab, hitPose.position + rabbitSpawnPoint, Quaternion.identity);
            turtle = Instantiate(turtlePrefab, hitPose.position + turtleSpawnPoint, Quaternion.identity);

            Rabbit rabbitScript = rabbit.GetComponent<Rabbit>();
            Turtle turtleScript = turtle.GetComponent<Turtle>();

            if (rabbitScript != null && turtleScript != null)
            {
                rabbitScript.turtle = turtleScript;
            }
            animalSpawn = true;

            Debug.Log("Bird spawned at: " + hitPose.position);
        }
        else
        {
            Debug.LogWarning("No AR plane hit at tapped position.");
        }
    }

    public void SetTreePlacementActive(bool active)
    {
        treePlacementActive = active;
        Debug.Log($"SpawnController: Tree placement set to {active}");
    }

    public void SetFlagPlacementActive(bool active)
    {
        flagPlacementActive = active;
        Debug.Log($"SpawnController: Flag placement set to {active}");
    }

    private bool IsClickOnUI(Vector2 screenPosition)
    {
        if (graphicRaycaster == null || eventSystem == null)
            return false;

        // 創建 PointerEventData
        PointerEventData pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = screenPosition;

        // 執行 raycast
        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEventData, results);

        // 如果有任何UI元素被點擊，返回true
        return results.Count > 0;
    }
}