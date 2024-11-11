using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SubmarineType
{
    public string name = "";
    public string description = "A submarine with engines, depth control, and fish catching capabilities.";
    public float basePrice = 10f;
    public float startingDrag = 10f;
    public float startingMass = 3000f;
    public float baseFishStorageCapacity = 1000f;
    public float maxPower = 1000f;
    public float volume = 10f;
    public float turnTime = 1f;
    public string spriteName = "";
    public int maxDurability = 10;

    public SubmarineType()
    {

    }

    public SubmarineType(float startingDrag, float startingMass, float baseFishStorageCapacity, float maxPower, float volume, float turnTime, string spriteName)
    {
        this.startingDrag = startingDrag;
        this.startingMass = startingMass;
        this.baseFishStorageCapacity = baseFishStorageCapacity;
        this.maxPower = maxPower;
        this.volume = volume;
        this.turnTime = turnTime;
        this.spriteName = spriteName;
    }
}

public class Submarine : MonoBehaviour
{
    public static Submarine Instance;

    public delegate void FishStored(Timefish fish, float percentFull);
    public static event FishStored OnFishStored;

    public CameraFollow cameraFollow;
    public SmoothBattleScreen battleScreen;

    private SubmarineType submarineType;
    private int remainingDurability = 10;

    private List<Timefish> fishStorage = new List<Timefish>(); // Fish stored in the submarine

    private bool turning = false; // Whether the submarine is currently turning

    private float drag = 0f; // Drag force in Newtons
    private float mass = 0f; // Base mass in kilograms
    private float powerSpent = 0f; // Power spent in power units

    private List<Equipment> coreModules = new(); // modules that are built into the submarine
    private List<Equipment> internalModules = new(); // modules that are installed inside the submarine
    private List<Equipment> hullMountedModules = new(); // modules that are installed outside the submarine

    public SubmarineType SubmarineType()
    {
        return submarineType;
    }

    public void EndOfRun()
    {
        // reduce the durability of all modules by 1
        foreach (Equipment module in AllModules())
        {
            module.remainingDurability -= 1;
        }

        // reduce the durability of the submarine by 1
        remainingDurability -= 1;

        // remove those modules that have 0 durability
        coreModules.RemoveAll(module => module.remainingDurability <= 0);
        internalModules.RemoveAll(module => module.remainingDurability <= 0);
        hullMountedModules.RemoveAll(module => module.remainingDurability <= 0);

        // create a string of module names separated by commas
        string moduleNames = "";
        foreach (Equipment module in AllModules())
        {
            moduleNames += module.name + ",";
        }
        moduleNames = moduleNames.TrimEnd(',');

        // save the module names in player prefs
        PlayerPrefs.SetString("LastRunModules", moduleNames);

        // create a string of module durabilities separated by commas
        string moduleDurabilities = "";
        foreach (Equipment module in AllModules())
        {
            moduleDurabilities += module.remainingDurability + ",";
        }
        moduleDurabilities = moduleDurabilities.TrimEnd(',');

        // save the module durabilities in player prefs
        PlayerPrefs.SetString("LastRunDurabilities", moduleDurabilities);

        // save the submarine type name in player prefs
        PlayerPrefs.SetString("LastRunSubmarineType", submarineType.name);

        // save the submarine durability in player prefs
        PlayerPrefs.SetInt("LastRunSubmarineDurability", remainingDurability);
    }

    public int RemainingDurability()
    {
        return remainingDurability;
    }

    public void SetRemainingDurability(int remainingDurability)
    {
        this.remainingDurability = remainingDurability;
    }

    public void SetSubmarineType(SubmarineType submarineType)
    {
        this.submarineType = submarineType;
    }

    public void AddEquipment(List<Equipment> equipment)
    {
        foreach (Equipment module in equipment)
        {
            coreModules.Add(module);
        }
    }

    void OnEnable()
    {
        Submarine.Instance = this;

        AddDrag(SubmarineType().startingDrag);
        AddMass(SubmarineType().startingMass);

        DiveExit.OnDiveExitEvent += EndOfRun;
    }

    void OnDisable()
    {
        Submarine.Instance = null;

        DiveExit.OnDiveExitEvent -= EndOfRun;
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
                if (UnityEngine.Random.Range(0, 10) == 0)
                {
                    phase = UnityEngine.Random.Range(-0.1f, 0.1f);
                }
                phaseTime = 0f;
            }
            // every 6 seconds, 10% chance to change the magnitude
            if (magnitudeTime > 6f)
            {
                if (UnityEngine.Random.Range(0, 10) == 0)
                {
                    magnitude = UnityEngine.Random.Range(0.05f, 0.15f);
                }
                magnitudeTime = 0f;
            }
        }
    }

    public float FishStorageCapacity()
    {
        return SubmarineType().baseFishStorageCapacity;
    }

    public float PowerRemaining()
    {
        return SubmarineType().maxPower - powerSpent;
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
            if (PowerRemaining() <= 0)
            {
                Destroy(this.gameObject);
            }
        } // destroy the submarine if out of power
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Timefish>() != null)
        {
            // Timefish hitFish = collision.gameObject.GetComponent<Timefish>();
            cameraFollow.JumpToBattle();
            // pass information to battleScreen
        }
    }

    private void FixedUpdate()
    {
        Planet planet = new();
        {
            if (Input.GetButton("up"))
            {
                GetLighter();
            }
            else if (Input.GetButton("down"))
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
            this.GetComponent<Rigidbody2D>().mass = CurrentMass();
            this.GetComponent<Rigidbody2D>().linearDamping = drag;
        } // update rigidbody mass and drag

        {
            // apply an upward force to the submarine according to the buoyancy
            float verticalForce = Buoyancy(planet);

            // if part of the submarine is above the surface
            float submarineHeight = this.GetComponent<SpriteRenderer>().bounds.size.y;
            if (this.transform.position.y > 0 /* the surface */ - submarineHeight / 2)
            {
                float percentAboveSurface = (this.transform.position.y + submarineHeight / 2) / submarineHeight;
                // reduce the vertical force by the percent above the surface
                verticalForce *= 1 - percentAboveSurface;
            }

            // apply a downward force to the submarine according to gravity
            float totalMass = CurrentMass();
            float gravityForce = totalMass * planet.gravity;

            float netForce = verticalForce - gravityForce;
            this.GetComponent<Rigidbody2D>().AddForce(new Vector2(0, netForce));
        } // apply vertical forces to the submarine

        {
            // if neither A nor D is pressed, do nothing
            if (Input.GetButton("left") || Input.GetButton("right"))
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
    }

    private IEnumerator Turn180Degrees()
    {
        yield return new WaitForSeconds(SubmarineType().turnTime);
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
        float buoyancy = densityOfWater * planet.gravity * SubmarineType().volume; // Buoyancy in Newtons
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
            fish.gameObject.SetActive(false);
            fishStorage.Add(fish);
            float percentFull = (fishMassStored + fish.Mass()) / FishStorageCapacity();
            OnFishStored?.Invoke(fish, percentFull);
            return true;
        }
        else
        {
            return false;
        }
    }

    public List<Timefish> AllCatches()
    {
        List<Timefish> allCatches = new();
        allCatches.AddRange(fishStorage);

        // add fish from harpoon guns to the list
        foreach (HarpoonGunBehaviour harpoonGun in GetComponentsInChildren<HarpoonGunBehaviour>())
        {
            allCatches.AddRange(harpoonGun.HarpoonedFish());
        }

        return allCatches;
    }
}
