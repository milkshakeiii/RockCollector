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

        // Add a Ballast to the submarine
        Ballast ballast = new();
        ballast.ballastMass = 6000f;
        ballast.ballastDropTime = 1f;
        ballast.ballastRefills = 3;
        ballast.ballastRefillTime = 2f;
        coreModules.Add(ballast);

        // Add several DepthControllers to the submarine
        for (int i = 0; i < 10; i++)
        {
            DepthController depthController = new();
            depthController.depthControlMass = 300f;
            depthController.depthControlTime = 1f;
            depthController.continuousPower = 1f;
            coreModules.Add(depthController);
        }
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
            this.GetComponent<Rigidbody2D>().mass = CurrentMass();
            this.GetComponent<Rigidbody2D>().drag = drag;
        } // update rigidbody mass and drag

        {
            if (PowerRemaining() <= 0)
            {
                Destroy(this.gameObject);
            }
        } // destroy the submarine if out of power
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
                totalMass += ballast.ballastMass;
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
}
