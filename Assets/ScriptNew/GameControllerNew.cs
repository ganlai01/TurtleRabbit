// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.EventSystems;
// using UnityEngine.XR.ARFoundation;
// using UnityEngine.XR.ARSubsystems;
// using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

// public class GameControllerNew : MonoBehaviour
// {
//     public Bird bird;
//     public Bird birdNPC;

//     public PlaneDetectionController planeDetectionController;
//     private Pitcher pitcher; // Reference to the Pitcher script
//     [SerializeField] private ARRaycastManager arRaycastManager; // For AR plane detection

//     [Header("Input Settings")]
//     [SerializeField]
//     private XRInputValueReader<Vector2> tapStartPositionInput = new XRInputValueReader<Vector2>("Tap Start Position");
//     static List<ARRaycastHit> hits = new List<ARRaycastHit>();


//     [Header("Game Settings")]
//     public GameObject rockPrefab;
//     public int numOfRocksToSpawn = 5;
//     public GameObject pitcherPrefab;
//     public GameObject birdNPCPrefab;

//     [Header("Raycast Visualization")]
//     public bool isRayVisible = true; // Toggle for ray visibility

//     private Vector2 prevTapStartPosition;
//     public static GameControllerNew Instance { get; private set; }

//     private void Awake()
//     {
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//         }
//         else
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject); // Keep this instance across scenes
//         }
//     }

//     private void Start()
//     {
//         if (arRaycastManager == null)
//         {
//             arRaycastManager = FindObjectOfType<ARRaycastManager>();
//             if (arRaycastManager == null)
//             {
//                 Debug.LogError("ARRaycastManager not found in scene!");
//             }
//         }
//     }

//     private void OnEnable()
//     {
//         BirdController.OnSizeThresholdMet += HandleSizeThresholdMet;
//         BirdController.OnBirdSpawned += HandleBirdSpawned;
//     }

//     private void OnDisable()
//     {
//         BirdController.OnSizeThresholdMet -= HandleSizeThresholdMet;
//         BirdController.OnBirdSpawned -= HandleBirdSpawned;
//     }

//     private void HandleSizeThresholdMet(bool birdSpawned)
//     {
//         UIController.Instance.EnableSpawnUI(!birdSpawned);
//     }

//     private void HandleBirdSpawned(Bird bird)
//     {
//         this.bird = bird;
//         UIController.Instance.SetDialogueUIHolder(bird.dialogueUIHolder); // Index 0
//         UIController.Instance.EnableObjectiveUI(true);
//         DialogueController.Instance.PlayDialogueSequence(DialogueSequence.StartingDialogue);
//         SpawnRocks();
//         SpawnPitcher();
//         SpawnBirdNPC();
//     }

//     private void Update()
//     {
//         HandleTapInput();
//     }

//     private void HandleTapInput()
//     {
//         Vector2 tapStartPosition;
//         bool tapPerformedThisFrame = tapStartPositionInput.TryReadValue(out tapStartPosition) && tapStartPosition != prevTapStartPosition;

//         var isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1);
//         if (!tapPerformedThisFrame || isPointerOverUI)
//             return;

//         // Use ARRaycastManager to detect hits on AR planes
//         if (arRaycastManager != null && arRaycastManager.Raycast(tapStartPosition, hits, TrackableType.PlaneWithinPolygon | TrackableType.Planes))
//         {
//             var hitPose = hits[0].pose;
//             Collider[] colliders = Physics.OverlapSphere(hitPose.position, 0.01f); // Adjust radius as needed

//             foreach (var collider in colliders)
//             {
//                 GameObject hitObject = collider.gameObject;

//                 if (collider.isTrigger) //Bird collider is set to trigger
//                 {
//                     Debug.Log($"Skipping trigger collider: {hitObject.name}");
//                     continue;
//                 }

//                 //Handle tap on pitcher
//                 if (hitObject.CompareTag("Pitcher"))
//                 {
//                     if (pitcher.birdInRange && bird.IsCarryingPebble())
//                     {
//                         pitcher.AddPebbleToBottle();
//                         bird.SetCarryingPebble(false);
//                         break;
//                     }
//                     else
//                     {
//                         Debug.Log("Bird is not in range or not carrying a pebble.");
//                     }
//                 }

//                 //Handle tap on rock
//                 if (hitObject.CompareTag("Rock"))
//                 {
//                     float distanceToRock = Vector3.Distance(bird.transform.position, hitObject.transform.position);
//                     if (distanceToRock > bird.GetCollectDistance())
//                         continue;

//                     if (bird.IsCarryingPebble())
//                     {
//                         UIController.Instance.SpawnDialogue("I can only carry one pebble at a time!", Color.red, 3f);
//                         break;
//                     }
//                     else if (!UIController.Instance.IsObjectiveCompleted(ObjectiveType.AskForHelp))
//                     {
//                         UIController.Instance.SpawnDialogue("A beautiful pebble!\nBut I don't need it right now.", Color.black, 3f);
//                         break;
//                     }
//                     else if (UIController.Instance.IsObjectiveCompleted(ObjectiveType.AskForHelp))
//                     {
//                         bird.CollectPebble(hitObject);
//                         break;
//                     }
//                 }

//                 // If no specific object was hit, fly to the plane position
//                 if (bird != null)
//                 {
//                     bird.FlyToTarget(hitPose.position);
//                     Debug.Log($"Bird flying to plane position: {hitPose.position}");
//                 }

//             }
//             prevTapStartPosition = tapStartPosition;
//         }
//     }

//     //Spwan some rocks on the AR plane
//     public void SpawnRocks()
//     {
//         planeDetectionController.SpawnRocks(numOfRocksToSpawn, rockPrefab);
//     }

//     public void SpawnPitcher()
//     {
//         GameObject pitcherInstance = planeDetectionController.SpawnPitcher(pitcherPrefab);
//         pitcher = pitcherInstance.GetComponent<Pitcher>();
//     }

//     public void SpawnBirdNPC()
//     {
//         GameObject birdNPCInstance = planeDetectionController.SpawnBirdNPC(birdNPCPrefab);
//         birdNPC = birdNPCInstance.GetComponent<Bird>();
//         UIController.Instance.SetDialogueUIHolder(birdNPC.dialogueUIHolder); //Index 1
//     }
// }