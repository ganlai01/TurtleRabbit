using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class FieldOfViewController : MonoBehaviour
{
    [Header("FOV Settings")]
    public float viewDistance = 5f;
    public float viewAngle = 60f;
    [Range(5, 50)]
    public int rayCount = 20;
    public Color fovColor = Color.red;

    private LineRenderer lineRenderer;
    private bool showFOV = true;
    public static bool GlobalFOVEnabled = false;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = true;
        SetColor(fovColor);
        lineRenderer.enabled = false;
        Debug.Log($"FOV Controller initialized for {gameObject.name} - Ray count: {rayCount}");
    }

    private void Update()
    {
        if (!GlobalFOVEnabled)
        {
            lineRenderer.enabled = false;
            return;
        }

        if (showFOV)
        {
            UpdateFieldOfView();
            lineRenderer.enabled = true;
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }

    private void UpdateFieldOfView()
    {
        Vector3[] points = new Vector3[rayCount + 2];
        points[0] = transform.position;

        float angleStep = viewAngle / (rayCount - 1);
        float startAngle = -viewAngle / 2;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = startAngle + (angleStep * i);
            Vector3 direction = Quaternion.Euler(0, transform.eulerAngles.y + angle, 0) * Vector3.forward;
            Vector3 endPoint = transform.position + direction * viewDistance;

            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, viewDistance))
            {
                endPoint = hit.point;
            }

            points[i + 1] = endPoint;
        }

        points[rayCount + 1] = transform.position;

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);

        Debug.Log($"FOV updated for {gameObject.name} - Points: {points.Length}, RayCount: {rayCount}");
    }

    public void ToggleFOV()
    {
        showFOV = !showFOV;
        Debug.Log($"FOV toggled for {gameObject.name}: {(showFOV ? "ON" : "OFF")}");
    }

    public void SetFOVVisible(bool visible)
    {
        showFOV = visible;
        Debug.Log($"FOV set to {(visible ? "visible" : "hidden")} for {gameObject.name}");
    }

    public void SetColor(Color color)
    {
        fovColor = color;
        if (lineRenderer != null)
        {
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }
    }

    [ContextMenu("Increase Ray Count")]
    public void IncreaseRayCount()
    {
        rayCount = Mathf.Min(rayCount + 5, 50);
        Debug.Log($"Ray count increased to: {rayCount}");
    }

    [ContextMenu("Decrease Ray Count")]
    public void DecreaseRayCount()
    {
        rayCount = Mathf.Max(rayCount - 5, 5);
        Debug.Log($"Ray count decreased to: {rayCount}");
    }
}
