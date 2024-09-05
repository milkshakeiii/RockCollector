using UnityEngine;

public class DiveExit : MonoBehaviour
{
    public GameObject endOfRunCanvas;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Submarine"))
        {
            endOfRunCanvas.SetActive(true);
        }
    }
}
