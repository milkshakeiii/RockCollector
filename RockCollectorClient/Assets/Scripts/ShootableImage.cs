using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootableImage : MonoBehaviour
{
    private Shootable shootable;

    public void Initialize(Shootable shootable)
    {
        this.shootable = shootable;
        // randomize the color of the image
        GetComponent<UnityEngine.UI.Image>().color = new Color(Random.value, Random.value, Random.value);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
