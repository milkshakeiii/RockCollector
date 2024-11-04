using UnityEngine;

public class DiveExit : MonoBehaviour
{
    public delegate void OnDiveExit();
    public static event OnDiveExit OnDiveExitEvent;

    public GameObject endOfRunCanvas;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Submarine"))
        {
            OnDiveExitEvent?.Invoke();
            endOfRunCanvas.SetActive(true);
        }
    }
}
