using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class FishTank : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Submarine.OnFishStored += DisplayFish;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayFish(Timefish timefish, float percentFull)
    {
        timefish.gameObject.SetActive(true);
        timefish.enabled = false;

        // also set to a different layer so that the fish are not affected by the physics of the game
        timefish.gameObject.layer = LayerMask.NameToLayer("Tank");

        // reparent the fish to the tank
        timefish.transform.SetParent(transform);

        // set the fish to a random position in the tank
        float leftBound = transform.position.x - transform.localScale.x / 3;
        float rightBound = transform.position.x + transform.localScale.x / 3;
        float topBound = transform.position.y + transform.localScale.y / 3;
        float bottomBound = transform.position.y - transform.localScale.y / 3;
        timefish.transform.position = new Vector3(Random.Range(leftBound, rightBound), Random.Range(topBound, bottomBound), 0);

        TMP_Text percentFullText = GetComponentInChildren<TMP_Text>();
        percentFullText.text = (percentFull).ToString("P0");
    }
}
