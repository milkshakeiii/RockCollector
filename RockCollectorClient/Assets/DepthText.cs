using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthText : MonoBehaviour
{
    public Submarine submarine;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float depth = submarine.transform.position.y;
        this.GetComponent<TMPro.TMP_Text>().text = depth.ToString("F2") + "m";
    }
}
