using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class FieldOfViewController : MonoBehaviour
{
    [Header("FOV Settings")]
    public float viewDistance = 5f;
    public float viewAngle = 60f;
    public int rayCount = 20;
    public Color fovColor = Color.red;

    private LineRenderer lineRenderer;
    private bool showFOV = true;
    public static bool GlobalFOVEnabled = true;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = rayCount + 1;
        lineRenderer.useWorldSpace = true;
        SetColor(fovColor);

        Debug.Log($"FOV Controller initialized for {gameObject.name}");
    }

    private void Update()
    {
        if (!FieldOfViewController.GlobalFOVEnabled)
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

    public void ToggleFOV()
    {
        showFOV = !showFOV;
        UpdateVisibility();
        Debug.Log($"FOV toggled for {gameObject.name}: {(showFOV ? "ON" : "OFF")}");
    }

    public void SetFOVVisible(bool visible)
    {
        showFOV = visible;
        UpdateVisibility();
        Debug.Log($"FOV set to {(visible ? "visible" : "hidden")} for {gameObject.name}");
    }

    private void UpdateVisibility()
    {
        if (GlobalFOVEnabled && showFOV)
        {
            lineRenderer.enabled = true;
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }

    public bool IsVisible()
    {
        return GlobalFOVEnabled && showFOV && (lineRenderer != null && lineRenderer.enabled);
    }

    public void SetColor(Color color)
    {
        fovColor = color;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    private void UpdateFieldOfView()
    {
        float angleStep = viewAngle / rayCount;
        float startAngle = -viewAngle / 2;
        Vector3[] points = new Vector3[rayCount + 1];
        points[0] = transform.position;

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

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }

    public static void SetGlobalFOVEnabled(bool enabled)
    {
        GlobalFOVEnabled = enabled;
        Debug.Log($"Global FOV set to: {enabled}");

        // 立即更新所有現存的 FOV
        FieldOfViewController[] allFOVs = FindObjectsOfType<FieldOfViewController>();
        foreach (FieldOfViewController fov in allFOVs)
        {
            fov.UpdateVisibility();
        }
    }
}