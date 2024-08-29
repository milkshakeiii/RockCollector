using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatableButton : MonoBehaviour
{
    private Submarine submarine;
    private Activatable activatable;

    public void Initialize(Submarine submarine, Activatable activatable)
    {
        this.submarine = submarine;
        this.activatable = activatable;
    }

    public void Click()
    {
        Debug.Log("Activating module: " + activatable);
        submarine.ActivateModule(activatable);
    }
}
