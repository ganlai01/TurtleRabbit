using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

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
    private Vector2 previousTapPosition;
    private static List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();
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

        // Detect if tap position changed (i.e., a new tap happened)
        if (tapStartPositionInput.TryReadValue(out var tapPosition) && tapPosition != previousTapPosition)
        {
            previousTapPosition = tapPosition;
            TrySpawnBirdAt(tapPosition);
        }
    }

    private void TrySpawnBirdAt(Vector2 screenPosition)
    {
        if (raycastManager.Raycast(screenPosition, raycastHits, TrackableType.Planes))
        {
            Pose hitPose = raycastHits[0].pose;
            rabbit = Instantiate(rabbitPrefab, hitPose.position + rabbitSpawnPoint, Quaternion.identity);
            turtle = Instantiate(turtlePrefab, hitPose.position + turtleSpawnPoint, Quaternion.identity);
            animalSpawn = true;

            Debug.Log("Bird spawned at: " + hitPose.position);
        }
        else
        {
            Debug.LogWarning("No AR plane hit at tapped position.");
        }
    }
}