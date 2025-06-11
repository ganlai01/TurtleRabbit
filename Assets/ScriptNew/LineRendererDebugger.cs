using UnityEngine;

public class LineRendererDebugger : MonoBehaviour
{
    public static LineRendererDebugger instance;
    private bool shouldHideLines = false;
    
    private void Awake()
    {
        instance = this;
    }
    
    private void Update()
    {
        // 每幀檢查並強制隱藏 LineRenderer（如果需要）
        if (shouldHideLines)
        {
            ForceHideAllLines();
        }
    }
    
    public static void SetLineVisibility(bool visible)
    {
        if (instance != null)
        {
            instance.shouldHideLines = !visible;
            if (visible)
            {
                instance.ShowAllLines();
            }
            else
            {
                instance.ForceHideAllLines();
            }
        }
    }
    
    private void ForceHideAllLines()
    {
        // 找到所有 LineRenderer，包括剛生成的
        LineRenderer[] allLines = FindObjectsOfType<LineRenderer>();
        
        foreach (LineRenderer line in allLines)
        {
            if (line.enabled)
            {
                line.enabled = false;
                Debug.Log($"強制隱藏: {line.gameObject.name}");
            }
        }
        
        // 同時停止所有 FieldOfViewController 的更新
        FieldOfViewController[] allFOVs = FindObjectsOfType<FieldOfViewController>();
        foreach (FieldOfViewController fov in allFOVs)
        {
            fov.enabled = false; // 停止 Update() 方法
            Debug.Log($"停止 FOV 更新: {fov.gameObject.name}");
        }
    }
    
    private void ShowAllLines()
    {
        LineRenderer[] allLines = FindObjectsOfType<LineRenderer>();
        
        foreach (LineRenderer line in allLines)
        {
            line.enabled = true;
            Debug.Log($"顯示: {line.gameObject.name}");
        }
        
        // 重新啟動 FieldOfViewController
        FieldOfViewController[] allFOVs = FindObjectsOfType<FieldOfViewController>();
        foreach (FieldOfViewController fov in allFOVs)
        {
            fov.enabled = true; // 重新啟動 Update() 方法
            Debug.Log($"重新啟動 FOV 更新: {fov.gameObject.name}");
        }
    }
    
    // 測試方法
    public void TestHide()
    {
        Debug.Log("=== 測試隱藏所有線條 ===");
        SetLineVisibility(false);
    }
    
    public void TestShow()
    {
        Debug.Log("=== 測試顯示所有線條 ===");
        SetLineVisibility(true);
    }
    
    // 診斷方法
    [ContextMenu("診斷場景中的 LineRenderer")]
    public void DiagnoseLineRenderers()
    {
        Debug.Log("=== LineRenderer 診斷 ===");
        
        LineRenderer[] allLines = FindObjectsOfType<LineRenderer>();
        Debug.Log($"找到 {allLines.Length} 個 LineRenderer");
        
        for (int i = 0; i < allLines.Length; i++)
        {
            LineRenderer lr = allLines[i];
            Debug.Log($"{i+1}. {lr.gameObject.name} - 啟用: {lr.enabled}, 位置數: {lr.positionCount}");
        }
        
        FieldOfViewController[] allFOVs = FindObjectsOfType<FieldOfViewController>();
        Debug.Log($"找到 {allFOVs.Length} 個 FieldOfViewController");
        
        for (int i = 0; i < allFOVs.Length; i++)
        {
            FieldOfViewController fov = allFOVs[i];
            Debug.Log($"{i+1}. {fov.gameObject.name} - 組件啟用: {fov.enabled}");
        }
    }
}