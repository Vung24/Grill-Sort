using UnityEngine;
using UnityEngine.UI;

public class CanvasScalerFitter : MonoBehaviour
{
    public float height = 1280;
    public float width = 720;
    
    [SerializeField] private RenderMode renderMode;
    
    void Start()
    {
        var canvasScaler = GetComponent<CanvasScaler>();
        
        // var ratio = (float)Screen.height / (float)Screen.width;
        // canvasScaler.matchWidthOrHeight = ratio >= 1.78f ? 1 : 0;
        float screenRatio = (float)Screen.width / (float)Screen.height;
        float targetRatio = (float)width / (float)height;
        
        canvasScaler.matchWidthOrHeight = (screenRatio >= targetRatio) ? 1 : 0;
        
        if (renderMode == RenderMode.ScreenSpaceCamera)
        {
            var canvas = GetComponent<Canvas>();
            canvas.worldCamera = Camera.main;
        }
    }
}
