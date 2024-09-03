using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerDisplay : MonoBehaviour
{
    public Submarine submarine;
    public UpAndDownIndicatorArrow powerArrow;
    public TMPro.TMP_Text powerText;

    // Update is called once per frame
    void Update()
    {
        if (!submarine.gameObject.activeSelf)
        {
            return;
        }
        powerArrow.SetHeight(submarine.PowerRemaining() / submarine.SubmarineType().maxPower);
        // display power percentage remaining in the text
        powerText.text = (submarine.PowerRemaining() / submarine.SubmarineType().maxPower * 100).ToString("F0") + "%";
    }
}
