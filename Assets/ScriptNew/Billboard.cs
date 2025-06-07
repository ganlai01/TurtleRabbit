using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.forward, mainCamera.transform.up);
            // // Calculate the direction to the camera
            // Vector3 directionToCamera = mainCamera.transform.position - transform.position;

            // // Ensure the canvas faces the camera without mirroring
            // transform.rotation = Quaternion.LookRotation(-directionToCamera, Vector3.up);
        }
    }
}