using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimefishSpawner : MonoBehaviour
{
    public GameObject timefishPrefab;
    public List<Sprite> bodies;
    public List<Sprite> fins;
    public List<Sprite> eyes;
    public List<Sprite> tails;

    public void Initialize(System.Random random)
    {
        for (int i = 0; i < 3; i++)
        {
            TimefishSpecies testSpecies = new TimefishSpecies();
            testSpecies.baseSize = 1.0f + i * 2f;
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

            testSpecies.bodySprite = bodies[random.Next(bodies.Count)];
            testSpecies.finSprite = fins[random.Next(fins.Count)];
            testSpecies.eyeSprite = eyes[random.Next(eyes.Count)];
            testSpecies.tailSprite = tails[random.Next(tails.Count)];

            // spawn 10 fish
            for (int j = 0; j < 10; j++)
            {
                float y = Random.Range(-10, 10);
                // don't spawn fish above the surface
                if (y > 0)
                {
                    y = -y;
                }
                GameObject newTimefish = Instantiate(timefishPrefab, this.transform.position + new Vector3(Random.Range(-10, 10), y, 0), Quaternion.identity);
                Timefish timefish = newTimefish.GetComponent<Timefish>();
                timefish.Initialize(testSpecies);
            }
        }
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
