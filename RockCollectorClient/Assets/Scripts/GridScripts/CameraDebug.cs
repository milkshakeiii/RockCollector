using TMPro;
using UnityEngine;

public class CameraDebug : MonoBehaviour
{
    public TMP_Text text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        text.text = Screen.currentResolution.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
