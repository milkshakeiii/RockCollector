using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActivatableButton : MonoBehaviour
{
    private Submarine submarine;
    private Activatable activatable;

    public void Initialize(Submarine submarine, Activatable activatable)
    {
        this.submarine = submarine;
        this.activatable = activatable;

        // randomize the button color
        GetComponent<UnityEngine.UI.Image>().color = new Color(Random.value, Random.value, Random.value);
    }

    public void Click()
    {
        Debug.Log("Activating module: " + activatable);
        submarine.ActivateModule(activatable);
    }

    void Update()
    {
        // also activate the module when button 2 is pressed
        if (Input.GetButtonDown("button2"))
        {
            Click();
        }
    }
}
