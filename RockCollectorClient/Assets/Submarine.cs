using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Submarine : MonoBehaviour
{
    public float startingDrag = 10.0f; // starting drag force in Newtons
    public float startingMass = 2000f; // starting mass in kilograms
    public float volume = 10f; // Base volume in cubic meters
    public float maxPower = 100f; // max power in power units
    public float turnTime = 1f; // Time to turn 180 degrees in seconds
    public float baseFishStorageCapacity = 1000f; // Capacity of fish storage in kilograms

    private List<Timefish> fishStorage = new List<Timefish>(); // Fish stored in the submarine

    private bool turning = false; // Whether the submarine is currently turning

    private float drag = 0f; // Drag force in Newtons
    private float mass = 0f; // Base mass in kilograms
    private float powerSpent = 0f; // Power spent in power units

    private List<Equipment> coreModules = new(); // modules that are built into the submarine
    private List<Equipment> internalModules = new(); // modules that are installed inside the submarine
    private List<Equipment> hullMountedModules = new(); // modules that are installed outside the submarine

    void Awake()
    {
        AddDrag(startingDrag);
        AddMass(startingMass);

        // Add two Ballasts to the submarine
        Ballast ballast = new();
        ballast.ballastMass = 3000f;
        ballast.ballastDropTime = 1f;
        ballast.ballastRefills = 3;
        ballast.ballastRefillTime = 2f;
        coreModules.Add(ballast);
        Ballast ballast2 = new();
        ballast2.ballastMass = 3000f;
        ballast2.ballastDropTime = 1f;
        ballast2.ballastRefills = 3;
        ballast2.ballastRefillTime = 2f;
        coreModules.Add(ballast2);

        // Add several DepthControllers to the submarine
        for (int i = 0; i < 10; i++)
        {
            DepthController depthController = new();
            depthController.depthControlMass = 300f;
            depthController.depthControlTime = 1f;
            depthController.continuousPower = 0.1f;
            coreModules.Add(depthController);
        }

        // Add an engine to the submarine
        Engine engine = new();
        engine.thrust = 8000f;
        engine.continuousPower = 0.1f;
        coreModules.Add(engine);

        // Add a HarpoonGun to the submarine
        HarpoonGun harpoonGun = new();
        harpoonGun.size = 1f;
        harpoonGun.velocity = 10f;
        harpoonGun.activationPower = 2f;
        harpoonGun.continuousPower = 1f;
        harpoonGun.range = 5f;
        harpoonGun.maxHarpoons = 2;
        harpoonGun.reelSpeed = 0.2f;
        harpoonGun.pullStrength = 1500f;
        harpoonGun.ropeElasticity = 0.5f;
        hullMountedModules.Add(harpoonGun);

        // Add a second HarpoonGun to the submarine
        HarpoonGun harpoonGun2 = new();
        harpoonGun2.size = 2f;
        harpoonGun2.velocity = 15f;
        harpoonGun2.activationPower = 2f;
        harpoonGun2.continuousPower = 1f;
        harpoonGun2.range = 7f;
        harpoonGun2.maxHarpoons = 3;
        harpoonGun2.reelSpeed = 0.4f;
        harpoonGun2.pullStrength = 3000f;
        harpoonGun2.ropeElasticity = 0.5f;
        hullMountedModules.Add(harpoonGun2);

        // Add a Scoop to the submarine
        Scoop scoop = new();
        scoop.scoopDiameter = 1f;
        scoop.scoopTime = 1f;
        scoop.activationPower = 1f;
        hullMountedModules.Add(scoop);
    }

    private void Start()
    {
        // StartCoroutine(GentleSway());


    }

    private IEnumerator GentleSway()
    {
        float magnitude = 0.1f;
        float phase = 0.1f;
        float phaseTime = 0f;
        float magnitudeTime = 0f;
        while (true)
        {
            float sway = Mathf.Sin(Time.time + phase) * magnitude;
            this.transform.position += new Vector3(0, sway, 0) * Time.deltaTime;
            yield return null;

            phaseTime += Time.deltaTime;
            magnitudeTime += Time.deltaTime;
            // every 5 seconds, 10% chance to change the phase
            if (phaseTime > 5f)
            {
                if (Random.Range(0, 10) == 0)
                {
                    phase = Random.Range(-0.1f, 0.1f);
                }
                phaseTime = 0f;
            }
            // every 6 seconds, 10% chance to change the magnitude
            if (magnitudeTime > 6f)
            {
                if (Random.Range(0, 10) == 0)
                {
                    magnitude = Random.Range(0.05f, 0.15f);
                }
                magnitudeTime = 0f;
            }
        }
    }

    public float FishStorageCapacity()
    {
        return baseFishStorageCapacity;
    }

    public float PowerRemaining()
    {
        return maxPower - powerSpent;
    }

    public void AddDrag(float drag)
    {
        this.drag += drag;
    }

    public void AddMass(float mass)
    {
        this.mass += mass;
    }

    public void SpendPower(float power)
    {
        powerSpent += power;
    }

    // Update is called once per frame
    void Update()
    {
        Planet planet = new();

        {
            // apply an upward force to the submarine according to the buoyancy
            float verticalForce = Buoyancy(planet);

            // apply a downward force to the submarine according to gravity
            float totalMass = CurrentMass();
            float gravityForce = totalMass * planet.gravity;

            float netForce = verticalForce - gravityForce;
            this.GetComponent<Rigidbody2D>().AddForce(new Vector2(0, netForce));
        } // apply vertical forces to the submarine

        { 
            if (UnityEngine.Input.GetKey(KeyCode.W))
            {
                GetLighter();
            }
            else if (UnityEngine.Input.GetKey(KeyCode.S))
            {
                GetHeavier();
            }
            else
            {
                // if neither W nor S is pressed, aim for zero buoyancy
                float currentMass = CurrentMass();
                float targetMass = Buoyancy(planet) / planet.gravity;
                float massDifference = targetMass - currentMass;
                // if the mass difference is less than 0.1% of the target mass, stop adjusting
                if (Mathf.Abs(massDifference) < 0.001f * targetMass)
                {
                    // do nothing
                }
                else if (massDifference > 0)
                {
                    GetHeavier();
                }
                else if (massDifference < 0)
                {
                    GetLighter();
                }
            }
        } // control the mass with W and S keys

        {
            // if neither A nor D is pressed, do nothing
            if (UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.D))
            {
                bool facingRight = this.transform.localScale.x > 0;
                bool turningRight = UnityEngine.Input.GetKey(KeyCode.D);
                if (facingRight != turningRight && !turning)
                {
                    turning = true;
                    StartCoroutine(Turn180Degrees());
                }
                else if (!turning)
                {
                    // fire all engines
                    foreach (var module in AllModules())
                    {
                        if (module is Engine engine)
                        {
                            int direction = facingRight ? 1 : -1;
                            GetComponent<Rigidbody2D>().AddRelativeForce(new Vector2(direction * engine.thrust, 0));
                            SpendPower(engine.continuousPower * Time.deltaTime);
                        }
                    }
                }
            }

        } // control the propulsion with A and D keys

        {
            this.GetComponent<Rigidbody2D>().mass = CurrentMass();
            this.GetComponent<Rigidbody2D>().linearDamping = drag;
        } // update rigidbody mass and drag

        {
            if (PowerRemaining() <= 0)
            {
                Destroy(this.gameObject);
            }
        } // destroy the submarine if out of power
    }

    private IEnumerator Turn180Degrees()
    {
        yield return new WaitForSeconds(turnTime);
        this.transform.localScale = new Vector3(-this.transform.localScale.x, this.transform.localScale.y, this.transform.localScale.z);
        turning = false;
    }

    public void GetLighter()
    {
        float powerSpent = 0f;
        foreach (var module in AllModules())
        {
            if (module is DepthController depthController)
            {
                if (depthController.AdjustMass(false))// get lighter
                {
                    powerSpent += depthController.continuousPower * Time.deltaTime;
                }
            }
        }
        SpendPower(powerSpent);
    }

    public void GetHeavier()
    {
        float powerSpent = 0f;
        foreach (var module in AllModules())
        {
            if (module is DepthController depthController)
            {
                if (depthController.AdjustMass(true))// get heavier
                {
                    powerSpent += depthController.continuousPower * Time.deltaTime;
                }
            }
        }
        SpendPower(powerSpent);
    }

    public void ActivateModule(Activatable module)
    {
        if (module.Activate())
        {
            SpendPower(module.activationPower);
        }
    }

    /// <summary>
    /// Calculates the buoyancy force in Newtons of the submarine on the given planet
    /// </summary>
    /// <param name="planet"></param>
    /// <returns></returns>
    public float Buoyancy(Planet planet)
    {
        // Calculate the buoyancy force
        float densityOfWater = 1000f; // Density of water in kg/m^3 (approximate)
        float buoyancy = densityOfWater * planet.gravity * volume; // Buoyancy in Newtons
        return buoyancy;
    }

    public List<Equipment> AllModules()
    {
        List<Equipment> allModules = new();
        allModules.AddRange(coreModules);
        allModules.AddRange(internalModules);
        allModules.AddRange(hullMountedModules);
        return allModules;
    }

    public List<Activatable> ActivatableModules()
    {
        return AllModules().FindAll(module => module is Activatable).ConvertAll(module => (Activatable)module);
    }

    public float CurrentMass()
    {
        float totalMass = mass;
        foreach (Equipment module in AllModules())
        {
            totalMass += module.AddedMass();
        }
        return totalMass;
    }

    public float MaximumMass()
    {
        float totalMass = mass;
        foreach (Equipment module in AllModules())
        {
            totalMass += module.mass;
            if (module is DepthController controller)
            {
                totalMass += controller.depthControlMass;
            }
            if (module is Ballast ballast)
            {
                totalMass += ballast.ballastMass;
            }
        }
        return totalMass;
    }

    public float BallastedMass()
    {
        float totalMass = mass;
        foreach (Equipment module in AllModules())
        {
            totalMass += module.mass;
            if (module is Ballast ballast)
            {
                totalMass += ballast.AddedMass();
            }
        }
        return totalMass;
    }

    public float DepthControlMass()
    {
        float totalMass = 0;
        foreach (Equipment module in AllModules())
        {
            if (module is DepthController controller)
            {
                totalMass += controller.depthControlMass;
            }
        }
        return totalMass;
    }

    public float MinimumMass()
    {
        float totalMass = mass;
        foreach (Equipment module in AllModules())
        {
            totalMass += module.mass;
        }
        return totalMass;
    }

    /// <summary>
    /// Returns true if the fish was successfully added to the fish storage, false otherwise
    /// </summary>
    /// <param name="fish"></param>
    /// <returns></returns>
    public bool AddFish(Timefish fish)
    {
        float fishMassStored = 0;
        foreach (Timefish storedFish in fishStorage)
        {
            fishMassStored += storedFish.Mass();
        }

        if (fishMassStored + fish.Mass() <= FishStorageCapacity())
        {
            fishStorage.Add(fish);
            return true;
        }
        else
        {
            return false;
        }
    }
}
