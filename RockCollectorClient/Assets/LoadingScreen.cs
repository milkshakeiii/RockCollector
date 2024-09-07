using System.Collections;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    public GameObject cameraObject;

    public IEnumerator DoLoadingDisplay()
    {
        // spin the camera around slowly
        while (true)
        {
            cameraObject.transform.Rotate(Vector3.forward, 10f * Time.deltaTime);
            yield return null;
        }
    }

    public void Stop()
    {
        // put the camera back to its original position
        cameraObject.transform.rotation = Quaternion.identity;
    }
}
