using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimefishSpawner : MonoBehaviour
{
    public GameObject timefishPrefab;

    // Start is called before the first frame update
    void Start()
    {
        TimefishSpecies testSpecies = new TimefishSpecies();
        testSpecies.baseSize = 1.0f;
        testSpecies.preySpecies = new List<TimefishSpecies>();
        testSpecies.visionConeArc = 90.0f;
        testSpecies.visionConeMiddle = 0f;
        testSpecies.visionRange = 10.0f;
        testSpecies.biteStrength = 1.0f;
        testSpecies.durability = 10.0f;
        testSpecies.weakSpots = new List<WeakSpot>();
        testSpecies.movementSpeed = 1.0f;
        testSpecies.baseTradeValue = 10f;
        testSpecies.aggressive = true;

        System.Random random = new();
        for (int i = 0; i < 10; i++)
        {
            GameObject newTimefish = Instantiate(timefishPrefab, new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), 0), Quaternion.identity);
            Timefish timefish = newTimefish.GetComponent<Timefish>();
            timefish.Initialize(testSpecies, random);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
