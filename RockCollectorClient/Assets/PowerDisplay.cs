using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerDisplay : MonoBehaviour
{
    public Submarine submarine;
    public UpAndDownIndicatorArrow powerArrow;

    // Update is called once per frame
    void Update()
    {
        powerArrow.SetHeight(submarine.PowerRemaining() / submarine.maxPower);
    }
}
