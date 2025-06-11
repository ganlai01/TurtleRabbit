using UnityEngine;

public class SimpleLineController : MonoBehaviour
{
    public static SimpleLineController instance;
    public GameObject turtle;

    void Start()
    {
        //spawn turtle to the scene
        if (turtle != null)
        {
            Instantiate(turtle, Vector3.zero, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Turtle GameObject is not assigned in the inspector.");
        }

    }
    
    private void Awake()
    {
        instance = this;
    }
    
    // 最直接的方法：隱藏所有 LineRenderer
    public static void HideAllLines()
    {
        LineRenderer[] allLines = FindObjectsOfType<LineRenderer>();
        Debug.Log($"找到 {allLines.Length} 個 LineRenderer");
        
        foreach (LineRenderer line in allLines)
        {
            line.enabled = false;
            Debug.Log($"隱藏了 {line.gameObject.name} 上的 LineRenderer");
        }
    }
    
    // 最直接的方法：顯示所有 LineRenderer
    public static void ShowAllLines()
    {
        LineRenderer[] allLines = FindObjectsOfType<LineRenderer>();
        Debug.Log($"找到 {allLines.Length} 個 LineRenderer");
        
        foreach (LineRenderer line in allLines)
        {
            line.enabled = true;
            Debug.Log($"顯示了 {line.gameObject.name} 上的 LineRenderer");
        }
    }
    
    // 測試方法 - 您可以在按鈕上直接調用這個
    public void TestHideLines()
    {
        HideAllLines();
    }
    
    public void TestShowLines()
    {
        ShowAllLines();
    }
}