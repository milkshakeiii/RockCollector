using UnityEngine;

public class SlowMove : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // move slowly in response to the WASD keys
        if (UnityEngine.Input.GetKey(KeyCode.W))
        {
            transform.position += new Vector3(0, 0.5f * Time.deltaTime, 0);
        }
        if (UnityEngine.Input.GetKey(KeyCode.S))
        {
            transform.position += new Vector3(0, -0.5f * Time.deltaTime, 0);
        }
        if (UnityEngine.Input.GetKey(KeyCode.A))
        {
            transform.position += new Vector3(-0.5f * Time.deltaTime, 0, 0);
        }
        if (UnityEngine.Input.GetKey(KeyCode.D))
        {
            transform.position += new Vector3(0.5f * Time.deltaTime, 0, 0);
        }
    }
}
