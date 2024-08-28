using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Submarine : MonoBehaviour
{
    public float drag = 0.0f; // Drag force in Newtons
    public float mass = 100f; // Base mass in kilograms
    private float volume = 10f; // Base volume in cubic meters

    private List<Equipment> coreModules = new(); // modules that are built into the submarine
    private List<Equipment> internalModules = new(); // modules that are installed inside the submarine
    private List<Equipment> hullMountedModules = new(); // modules that are installed outside the submarine

    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Rigidbody2D>().drag = drag;
    }

    // Update is called once per frame
    void Update()
    {
        Planet planet = new();

        // apply an upward force to the submarine according to the buoyancy
        this.GetComponent<ConstantForce2D>().force = new Vector2(0, Buoyancy(planet));

        // apply a downward force to the submarine according to gravity
        float totalMass = CurrentMass();
        float gravityForce = totalMass * planet.gravity;
        this.GetComponent<ConstantForce2D>().force += new Vector2(0, -gravityForce);
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
