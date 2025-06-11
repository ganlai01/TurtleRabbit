using UnityEngine;

public class ViewToggleButton : MonoBehaviour
{
    public static ViewToggleButton instance;
    public Rabbit rabbit;
    public Turtle turtle;

    private void Awake()
    {
        // Set up singleton
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ToggleFOVs()
    {
        Debug.Log("ToggleFOVs called");
        if (rabbit != null) 
        {
            rabbit.ToggleFieldOfView();
            Debug.Log("Rabbit FOV toggled");
        }
        else
        {
            Debug.LogWarning("Rabbit reference is null");
        }
        
        if (turtle != null) 
        {
            turtle.ToggleFieldOfView();
            Debug.Log("Turtle FOV toggled");
        }
        else
        {
            Debug.LogWarning("Turtle reference is null");
        }
    }

    public void SetFOVsVisible(bool visible)
    {
        Debug.Log($"SetFOVsVisible called with: {visible}");
        if (rabbit != null && rabbit.fovController != null) 
        {
            rabbit.fovController.SetFOVVisible(visible);
            Debug.Log($"Rabbit FOV set to {(visible ? "visible" : "hidden")}");
        }
        else
        {
            Debug.LogWarning("Rabbit or its FOV controller is null");
        }
        
        if (turtle != null && turtle.fovController != null) 
        {
            turtle.fovController.SetFOVVisible(visible);
            Debug.Log($"Turtle FOV set to {(visible ? "visible" : "hidden")}");
        }
        else
        {
            Debug.LogWarning("Turtle or its FOV controller is null");
        }
    }
}