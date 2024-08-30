using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timefish : MonoBehaviour
{
    private TimefishSpecies species;
    
    private Behavior currentBehavior;

    private float size; //Size(volume) in cubic meters

    public void Initialize(TimefishSpecies newSpecies)
    {
        this.species = newSpecies;

        // generate size based on species base size
        // size follows a normal distribution with a mean of 40% species base size and a standard deviation of 10% of the base size
        // use the Box-Muller transform to generate a random number from a normal distribution
        float smallSize = species.baseSize * 0.4f;
        float u1 = Random.Range(0f, 1f);
        float u2 = Random.Range(0f, 1f);
        float z0 = Mathf.Sqrt(-2 * Mathf.Log(u1)) * Mathf.Cos(2 * Mathf.PI * u2);
        size = smallSize + 0.1f * smallSize * z0;

        // randomly, 5% of the time, size instead follows a normal distribution with a mean of 80% species base size
        // and a standard deviation of 20% of the base size
        if (Random.Range(0, 20) == 0)
        {
            float largeSize = species.baseSize * 0.8f;
            u1 = Random.Range(0f, 1f);
            u2 = Random.Range(0f, 1f);
            z0 = Mathf.Sqrt(-2 * Mathf.Log(u1)) * Mathf.Cos(2 * Mathf.PI * u2);
            size = largeSize + 0.2f * largeSize * z0;
        }

        // ensure size is always > 0
        float softMinSize = 0.05f * species.baseSize;
        if (size < softMinSize)
        {
            // reroll to get a size between 0.03 and 0.05 of the species base size (to avoid very small fish)
            size = Random.Range(0.03f, 0.05f) * species.baseSize;
            // just for fun, make the fish 0.025 of the species base size with a 1 in 100 chance
            if (Random.Range(0, 100) == 0)
            {
                size = 0.025f * species.baseSize;
            }
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        currentBehavior = new IdleBehavior();
    }

    // Update is called once per frame
    void Update()
    {
        currentBehavior.Update(this);
    }

    public float GetSpeed()
    {
        return species.movementSpeed * size;
    }

    public float ReangleSpeed()
    {
        return species.movementSpeed * size;
    }
}

public abstract class Behavior
{
    public abstract void Update(Timefish fish);
}

public class IdleBehavior : Behavior
{
    private float maximumSwimForwardTime = 5f;
    private float maximumReangleArc = 90f;

    private float stillTime = 0f;

    public override void Update(Timefish fish)
    {
        //every second
        stillTime += Time.deltaTime;
        if (stillTime > 1f)
        {
            // randomly with a 1 in 10 chance, reposition
            if (Random.Range(0, 10) == 0)
            {
                fish.StartCoroutine(SwimForward(fish));
            }
            // randomly with a 1 in 10 chance, reangle
            if (Random.Range(0, 10) == 0)
            {
                fish.StartCoroutine(Reangle(fish));
            }
            // randomly with a 1 in 10 chance, turn 180 degrees
            if (Random.Range(0, 10) == 0)
            {
                fish.StartCoroutine(Turn180(fish));
            }
            stillTime = 0f;
        }
    }

    IEnumerator SwimForward(Timefish fish)
    {
        float speed = fish.GetSpeed();

        // swim forward for a random amount of time
        float swimTime = Random.Range(0, maximumSwimForwardTime);
        float time = 0f;
        while (time < swimTime)
        {
            fish.transform.position += fish.transform.right * speed * Time.deltaTime;
            time += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator Reangle(Timefish fish)
    {
        float speed = fish.ReangleSpeed();

        // reangle by a random amount, making sure not to turn upside down
        float arc = Random.Range(-maximumReangleArc, maximumReangleArc);
        float time = 0f;
        while (time < 1f)
        {
            fish.transform.Rotate(0, 0, arc * Time.deltaTime);
            // make sure the fish doesn't turn upside down by reversing direction if it does
            if (fish.transform.up.y < 0)
            {
                arc = -arc;
            }
            time += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator Turn180(Timefish fish)
    {
        fish.transform.localScale = new Vector3(-fish.transform.localScale.x, fish.transform.localScale.y, fish.transform.localScale.z);
        yield return null;
    }
}

public class TimefishSpecies
{
    public float baseSize; //Base size(volume) (greater with depth) in cubic meters
    public List<TimefishSpecies> preySpecies; //Prey species(usually smaller species found nearby, but can also be larger)
    public float visionConeArc; //Vision cone arc in degrees
    public float visionConeMiddle; //Vision cone middle in degrees
    public float visionRange; //Vision range factor in meters
    public float biteStrength; //Bite strength(greater with size and with depth, separately) factor in power units
    public float durability; //Durability factor in power units
    public List<WeakSpot> weakSpots; //Weak spots
    public float movementSpeed; //Movement speed(greater with size) factor in meters per second
    public float baseTradeValue; //Base trade value factor (for robot versions only) (greater with depth) in trade value units
    public bool aggressive; //Aggressive/not aggressive towards submarines (more likely with depth)
}

public class  WeakSpot
{
    
}