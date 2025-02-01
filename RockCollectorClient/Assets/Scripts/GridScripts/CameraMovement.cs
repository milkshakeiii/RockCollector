using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float zoomSpeed = 5f;
    public float zoomMax = -25f;

    private float startingZoom;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startingZoom = -Camera.main.transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        // zoom with scroll wheel
        float scroll = UnityEngine.Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            Camera.main.transform.position += new Vector3(0, 0, scroll * zoomSpeed);
            if (Camera.main.transform.position.z > zoomMax)
            {
                Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, zoomMax);
            }
        }
        // reset zoom with tab
        if (UnityEngine.Input.GetKeyDown(KeyCode.Tab))
        {
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, startingZoom);
        }

        // move with WASD or arrow keys
        if (UnityEngine.Input.GetKey(KeyCode.W) || UnityEngine.Input.GetKey(KeyCode.UpArrow))
        {
            Camera.main.transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);
        }
        if (UnityEngine.Input.GetKey(KeyCode.S) || UnityEngine.Input.GetKey(KeyCode.DownArrow))
        {
            Camera.main.transform.position += new Vector3(0, -moveSpeed * Time.deltaTime, 0);
        }
        if (UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow))
        {
            Camera.main.transform.position += new Vector3(-moveSpeed * Time.deltaTime, 0, 0);
        }
        if (UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow))
        {
            Camera.main.transform.position += new Vector3(moveSpeed * Time.deltaTime, 0, 0);
        }
    }
}
