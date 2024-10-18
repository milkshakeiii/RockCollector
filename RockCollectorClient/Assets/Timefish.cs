using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timefish : MonoBehaviour
{
    public GameObject body;
    public GameObject fin;
    public GameObject eye;
    public GameObject tail;

    public TimefishSpecies species;
    
    private Behavior currentBehavior;

    private float size; //Size(volume) in cubic meters
    private float damageTaken; //Damage taken in power units

    public void Initialize(TimefishSpecies newSpecies)
    {
        System.Random random = new System.Random();

        this.species = newSpecies;

        // generate size based on species base size
        // size follows a normal distribution with a mean of 40% species base size and a standard deviation of 10% of the base size
        // use the Box-Muller transform to generate a random number from a normal distribution
        float smallSize = species.baseSize * 0.4f;
        float z0 = (float)Environment.NormalDistribution(random);
        size = smallSize + 0.1f * smallSize * z0;

        // randomly, 20% of the time, size instead follows a normal distribution with a mean of 60% species base size
        // and a standard deviation of 15% of the base size
        if (random.Next(0, 100) < 20)
        {
            float mediumSize = species.baseSize * 0.6f;
            z0 = (float)Environment.NormalDistribution(random);
            size = mediumSize + 0.15f * mediumSize * z0;
        }

        // randomly, 5% of the time, size instead follows a normal distribution with a mean of 80% species base size
        // and a standard deviation of 20% of the base size
        if (random.Next(0, 100) < 5)
        {
            float largeSize = species.baseSize * 0.8f;
            z0 = (float)Environment.NormalDistribution(random);
            size = largeSize + 0.2f * largeSize * z0;
        }

        // ensure size is always > 0
        float softMinSize = 0.05f * species.baseSize;
        if (size < softMinSize)
        {
            // reroll to get a size between 0.03 and 0.05 of the species base size (to avoid very small fish)
            size = (float)random.NextDouble() * (0.05f - 0.03f) + 0.03f;
            // just for fun, make the fish 0.025 of the species base size with a 1 in 100 chance
            if (random.Next(0, 100) == 0)
            {
                size = 0.025f * species.baseSize;
            }
        }

        // size determines sprite scale
        transform.localScale = new Vector3(size, size, size);

        // use the species sprites
        body.GetComponent<SpriteRenderer>().sprite = species.bodySprite;
        fin.GetComponent<SpriteRenderer>().sprite = species.finSprite;
        eye.GetComponent<SpriteRenderer>().sprite = species.eyeSprite;
        tail.GetComponent<SpriteRenderer>().sprite = species.tailSprite;
        body.GetComponent<SpriteRenderer>().sortingOrder = this.GetComponent<SpriteRenderer>().sortingOrder;
        fin.GetComponent<SpriteRenderer>().sortingOrder = this.GetComponent<SpriteRenderer>().sortingOrder;
        eye.GetComponent<SpriteRenderer>().sortingOrder = this.GetComponent<SpriteRenderer>().sortingOrder;
        tail.GetComponent<SpriteRenderer>().sortingOrder = this.GetComponent<SpriteRenderer>().sortingOrder;

        // delete self if spawned on top of a collider (other than self)
        float diameter = size * 2;
        if (Physics2D.OverlapCircleAll(transform.position, diameter).Length > 1)
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody2D>().mass = Mass();

        currentBehavior = new StillBehavior();
    }

    // Update is called once per frame  
    void Update()
    {
        currentBehavior.Update(this);
    }

    public void CheckAggro()
    {
        // non aggressive fish don't care about the submarine
        if (!species.aggressive)
        {
            return;
        }

        // check if the submarine is in the vision cone
        if (SubmarineInVisionCone())
        {
            // aggro the fish if the submarine is in the vision cone
            SetBehavior(new AggressiveBehavior(species.chaseTime));
            Debug.Log("Chasing for " + species.chaseTime + " seconds");
        }
    }

    public bool SubmarineInVisionCone()
    {
        if (Submarine.Instance == null)
        {
            return false;
        }
        Vector3 toSub = Submarine.Instance.transform.position - transform.position;
        float angle = Vector3.Angle(transform.right, toSub);
        // Debug.Log("Angle: " + angle + " Magnitude" + toSub.magnitude);
        return angle < species.visionConeArc / 2 && toSub.magnitude < species.visionRange;
    }

    public void FishRotate(float angle)
    {
        this.transform.Rotate(0, 0, angle * Time.deltaTime);
        // make sure the fish doesn't turn upside down by flipping it if it does
        if (this.transform.up.y < 0)
        {
            this.transform.localScale = new Vector3(this.transform.localScale.x, -this.transform.localScale.y, this.transform.localScale.z);
        }
    }

    public void SetBehavior(Behavior newBehavior)
    {
        currentBehavior = newBehavior;
    }

    public void TakeDamage(float damage)
    {
        damageTaken += damage;
        if (IsDisabled())
        {
            // turn the fish upside down
            transform.localScale = new Vector3(transform.localScale.x, -transform.localScale.y, transform.localScale.z);
            // set the behavior to still
            SetBehavior(new StillBehavior());
        }
    }

    public bool IsDisabled()
    {
        return damageTaken > Health();
    }

    public float Health() // max damage in power units that the fish can take
    {
        return species.healthFactor * size;
    }

    public float Mass()
    {
        return size * 1000;
    }

    public float Size()
    {
        return size;
    }

    public float Speed()
    {
        return species.movementSpeed * size;
    }

    public float ReangleSpeed()
    {
        return species.movementSpeed * size;
    }

    public float TradeValue()
    {
        return species.baseTradeValue * size;
    }
}

public abstract class Behavior
{
    public abstract void Update(Timefish fish);
}

public class RoamBehavior : Behavior
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
        float speed = fish.Speed();

        // swim forward for a random amount of time
        float swimTime = Random.Range(0, maximumSwimForwardTime);
        float time = 0f;
        while (time < swimTime)
        {
            fish.transform.position += fish.transform.right * speed * Time.deltaTime;
            // turn around if reaching the surface (at most once per call)
            if (fish.transform.position.y > 0)
            {
                speed = -speed;
            }
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
            fish.FishRotate(arc * Time.deltaTime);
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

public class StillBehavior : Behavior
{
    public override void Update(Timefish fish)
    {
        // float gently up and down
        fish.transform.position += new Vector3(0, Mathf.Sin(Time.time * fish.species.baseSize) * 0.0005f * fish.species.movementSpeed, 0);

        // check if the submarine is in vision range
        fish.CheckAggro();
    }
}

public class AggressiveBehavior : Behavior
{
    private float chaseTime;
    private float timeChasing;

    public AggressiveBehavior(float chaseTime)
    {
        this.chaseTime = chaseTime;
        timeChasing = 0f;
    }

    public override void Update(Timefish fish)
    {
        // chase the submarine for a limited amount of time
        timeChasing += Time.deltaTime;
        if (timeChasing > chaseTime)
        {
            fish.SetBehavior(new StillBehavior());
        }

        // if the submarine is still in vision range, rotate towards it and swim forward
        if (fish.SubmarineInVisionCone())
        {
            Vector3 toSub = Submarine.Instance.transform.position - fish.transform.position;
            float angle = Vector3.SignedAngle(fish.transform.up, toSub, Vector3.forward);
            fish.FishRotate(angle);

            fish.transform.position += fish.Speed() * Time.deltaTime * fish.transform.right;
        }
    }
}

public class TimefishSpecies
{
    public string name = "test species";
    public float baseSize; //Base size(volume) (greater with depth) in cubic meters
    
    public float healthFactor; //Durability factor in power units per cubic meter
    public List<WeakSpot> weakSpots; //Weak spots
    public float movementSpeed; //Movement speed(greater with size) factor in meters per second
    public float baseTradeValue; //Base trade value factor (for robot versions only) (greater with depth) in trade value units

    public float visionConeArc; //Vision cone arc in degrees
    public float visionConeMiddle; //Vision cone middle in degrees
    public float visionRange; //Vision range factor in meters

    public bool aggressive; // does the fish become aggrivated when submarine enters vision range
    public bool school; // does the fish school with others of its species
    public float chaseTime; // how long does the fish remain aggrivated after the submarine leaves vision range

    public int maneuverability;
    public int stealth;
    public int armor;

    public Sprite bodySprite;
    public Sprite finSprite;
    public Sprite eyeSprite;
    public Sprite tailSprite;
}

public class  WeakSpot
{
    
}