using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float zoomSpeed = 5f;
    public float zoomMax = -25f;

    private float startingZoom;
    private Vector3 lastMouseWorldPosition = -Vector3.one;

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

        // also move with middle mouse button drag
        if (UnityEngine.Input.GetMouseButtonDown(2))
        {
            StartCoroutine(Drag());
        }
    }

    private System.Collections.IEnumerator Drag()
    {
        Vector3 mouseStartPosition = UnityEngine.Input.mousePosition;
        Vector3 startCameraPosition = Camera.main.transform.position;
        
        while (UnityEngine.Input.GetMouseButton(2))
        {
            Vector3 mouseCurrentPosition = UnityEngine.Input.mousePosition;
            Vector3 diff = mouseStartPosition - mouseCurrentPosition;
            Camera.main.transform.position = startCameraPosition + diff/8f;
            yield return null;
        }
    }
}
