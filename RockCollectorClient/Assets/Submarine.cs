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
            depthController.continuousPower = 1f;
            coreModules.Add(depthController);
        }

        // Add an engine to the submarine
        Engine engine = new();
        engine.thrust = 2000f;
        engine.continuousPower = 1f;
        coreModules.Add(engine);
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
                else
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
            this.GetComponent<Rigidbody2D>().drag = drag;
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
}
