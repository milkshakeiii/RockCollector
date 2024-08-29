using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeightDisplay : MonoBehaviour
{
    public Submarine submarine;
    public GameObject barPrefab;
    public UpAndDownIndicatorArrow weightArrow;
    public UpAndDownIndicatorArrow waterArrow;

    // Start is called before the first frame update
    void Start()
    {
        ResetBars();

        Planet planet = new();
        float volume = submarine.volume;
        float waterMass = 1000 * volume;
        float submarineMaxMass = submarine.MaximumMass();
        float waterMassFraction = waterMass / submarineMaxMass;
        waterArrow.SetHeight(waterMassFraction);

        Ballast.OnBallastChanged += ResetBars;
    }

    private void Update()
    {
        float currentMass = submarine.CurrentMass();
        float newHeight = currentMass / submarine.MaximumMass();
        weightArrow.SetHeight(newHeight);
    }

    private void ResetBars()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }


        float minimumMass = submarine.MinimumMass();
        float ballastedMass = submarine.BallastedMass();
        float maximumMass = submarine.MaximumMass();
        Debug.Log("Minimum Mass: " + minimumMass);
        Debug.Log("Ballasted Mass: " + ballastedMass);
        Debug.Log("Maximum Mass: " + maximumMass);

        GameObject minimumMassBar = Instantiate(barPrefab, this.transform);
        // set minimum bass bar anchors so that it takes up the bottom part of the bar, up to minimumMass/maxMass
        minimumMassBar.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        minimumMassBar.GetComponent<RectTransform>().anchorMax = new Vector2(1, minimumMass / maximumMass);

        GameObject ballastedMassBar = Instantiate(barPrefab, this.transform);
        // set ballasted mass bar anchors so that it takes up the middle part of the bar, from minimumMass/maxMass to ballastedMass/maxMass
        ballastedMassBar.GetComponent<RectTransform>().anchorMin = new Vector2(0, minimumMass / maximumMass);
        ballastedMassBar.GetComponent<RectTransform>().anchorMax = new Vector2(1, ballastedMass / maximumMass);

        GameObject maximumMassBar = Instantiate(barPrefab, this.transform);
        // set maximum mass bar anchors so that it takes up the top part of the bar, from ballastedMass to (ballastedMass + depthControlMass)
        maximumMassBar.GetComponent<RectTransform>().anchorMin = new Vector2(0, ballastedMass / maximumMass);
        maximumMassBar.GetComponent<RectTransform>().anchorMax = new Vector2(1, (ballastedMass + submarine.DepthControlMass()) / maximumMass);

        // reset each bar's rect transform so that it takes up just the area between the anchors
        minimumMassBar.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        minimumMassBar.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        ballastedMassBar.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        ballastedMassBar.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        maximumMassBar.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        maximumMassBar.GetComponent<RectTransform>().offsetMax = Vector2.zero;

        // set the color of each bar
        minimumMassBar.GetComponent<UnityEngine.UI.Image>().color = Color.red;
        ballastedMassBar.GetComponent<UnityEngine.UI.Image>().color = Color.green;
        maximumMassBar.GetComponent<UnityEngine.UI.Image>().color = Color.cyan;
    }
}
