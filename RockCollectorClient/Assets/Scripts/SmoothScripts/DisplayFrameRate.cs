using TMPro;
using UnityEngine;

public class DisplayFrameRate : MonoBehaviour
{
    public TMP_Text displayText;
    
    private int frameCounter = 0;
    private float lastUpdateTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // display the frame rate every second
        if (Time.time - lastUpdateTime >= 1f)
        {
            displayText.text = "FPS: " + frameCounter;
            frameCounter = 0;
            lastUpdateTime = Time.time;
        }
        frameCounter++;
    }
}
