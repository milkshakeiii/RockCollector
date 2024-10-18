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

    public void Initialize(System.Random random, float minX, float maxX, float minY, float maxY)
    {
        TimefishSpecies testSpecies = new TimefishSpecies();
        testSpecies.baseSize = Random.Range(0.9f, 6.1f);
        testSpecies.visionConeArc = 90.0f;
        testSpecies.visionConeMiddle = 0f;
        testSpecies.visionRange = 10.0f;
        testSpecies.healthFactor = 10.0f;
        testSpecies.weakSpots = new List<WeakSpot>();
        testSpecies.movementSpeed = 1.0f;
        testSpecies.baseTradeValue = 10f;
        testSpecies.aggressive = true;
        testSpecies.chaseTime = 15.0f;

        testSpecies.bodySprite = bodies[random.Next(bodies.Count)];
        testSpecies.finSprite = fins[random.Next(fins.Count)];
        testSpecies.eyeSprite = eyes[random.Next(eyes.Count)];
        testSpecies.tailSprite = tails[random.Next(tails.Count)];

        // spawn a handful of fish
        int numFish = Mathf.RoundToInt((float)
            Environment.MultimodalDistribution(
                random,
                new List<int>() { 50, 80, 96 },
                new List<double>() { 10f, 5f, 15f },
                new List<double>() { 1f, 0.3f, 2f },
                true // rerandomize
            )
        );
        if (testSpecies.baseSize < 2)
        {
            numFish *= 2;
        }
        if (testSpecies.baseSize > 4.5f)
        {
            numFish /= 2;
        }
        numFish = Mathf.Max(numFish, 1); // at least one fish
        List<Vector3> spawnOffsets = RandomSpawnOffsetsForSpecies(testSpecies, numFish); // note random is not passed
        

        foreach (Vector3 offset in spawnOffsets)
        {
            Vector3 newPosition = this.transform.position + offset;
            if (newPosition.x < minX || newPosition.x > maxX || newPosition.y < minY || newPosition.y > maxY)
            {
                continue;
            }
            GameObject newTimefish = Instantiate(timefishPrefab, newPosition, Quaternion.identity);
            Timefish timefish = newTimefish.GetComponent<Timefish>();
            timefish.GetComponent<SpriteRenderer>().sortingOrder = -1;
            timefish.Initialize(testSpecies);
        }
    }

    private List<Vector3> RandomSpawnOffsetsForSpecies(TimefishSpecies species, int numFish)
    {
        // use a new random because it will be called a variable number of times based on the rerandomized numFish
        System.Random random = new System.Random();

        List<Vector3> spawnOffsets = new List<Vector3>();
        int spawnShape = random.Next(3);
        if (species.baseSize > 4)
        {
            spawnShape = random.Next(2);
        }
        if (species.baseSize > 5)
        {
            spawnShape = 0;
        }

        if (spawnShape == 0)
        {
            // random within a square
            for (int j = 0; j < numFish; j++)
            {
                float squareSize = species.baseSize * 3f;
                float y = Random.Range(-squareSize, squareSize);
                // don't spawn fish above the surface
                if (y > 0)
                {
                    y = -y;
                }
                Vector3 offset = new Vector3(Random.Range(-squareSize, squareSize), y, 0);
                spawnOffsets.Add(offset);
            }
        }
        if (spawnShape == 1)
        {
            // making a half circle
            float radius = species.baseSize * 3f;
            for (int j = 0; j < numFish; j++)
            {
                float angle = j / (float)numFish * Mathf.PI;
                Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
                spawnOffsets.Add(offset);
            }
        }
        if (spawnShape == 2)
        {
            // evenly spaced in a grid
            int numPerRow = Mathf.CeilToInt(Mathf.Sqrt(numFish * 2));
            float spacing = species.baseSize * 0.6f;
            for (int j = 0; j < numFish*2; j++)
            {
                // 50% chance to skip a spot
                if (random.Next(0, 2) == 0)
                {
                    continue;
                }
                float x = j % numPerRow;
                float y = j / numPerRow;
                Vector3 offset = new Vector3(x * spacing, y * spacing, 0);
                spawnOffsets.Add(offset);
            }
        }
        return spawnOffsets;
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
