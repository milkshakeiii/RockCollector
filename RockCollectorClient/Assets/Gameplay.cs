using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

public class Planet
{
    public float gravity = 9.8f; // Gravity in m/s^2
}

public enum EquipmentSlot
{
    Core,
    Internal,
    HullMounted
}

public class Equipment
{
    public int maxDurability = 3; // Maximum durability of the equipment
    public int remainingDurability = 3; // Current durability of the equipment

    public string name = ""; // Name of the equipment
    public string description = ""; // Description of the equipment

    public EquipmentSlot slot = EquipmentSlot.Core; // Slot used by the equipment

    public float mass = 0f; // Mass in kilograms
    public float activationPower = 0f; // Instantaneous power cost to activate
    public float continuousPower = 0f; // Continuous power cost per second

    public virtual Equipment Copy()
    {
        return (Equipment)MemberwiseClone();
    }

    public virtual float AddedMass()
    {
        return mass;
    }
}

public abstract class Shootable : Equipment
{
    public abstract GameObject SpawnWorldObject(Submarine submarine);
}

public class HarpoonGun : Shootable
{
    public float size = 1f;// visual scale of the harpoon gun
    public float velocity = 10f; // Velocity of the harpoon in m/s
    public float range = 7f; // Range of the harpoon in meters
    public int maxHarpoons = 2; // Number of harpoons at once
    public float reelSpeed = 1f; // Speed to reel in the harpoon in m/s
    public float pullStrength = 1f; // Strength of the rope's pull
    public float ropeElasticity = 0.5f; // portion beyond the distance that the rope can stretch before breaking

    public override GameObject SpawnWorldObject(Submarine submarine)
    {
        // find prefab with name "HarpoonGun"
        GameObject harpoonGunPrefab = Resources.Load<GameObject>("HarpoonGun");

        // instantiate the prefab
        GameObject harpoonGun = GameObject.Instantiate(harpoonGunPrefab, submarine.transform);
        harpoonGun.GetComponent<HarpoonGunBehaviour>().Initialize(this);
        return harpoonGun;
    }
}

public abstract class Activatable : Equipment
{
    // returns true if power should be deducted, false otherwise
    public abstract bool Activate();
}

public class Scoop : Shootable
{
    public float scoopDiameter = 0f; // Diameter of the scoop in meters
    public float scoopTime = 1f; // Time to scoop in seconds

    private List<Timefish> capturedFish = new List<Timefish>();

    public override GameObject SpawnWorldObject(Submarine submarine)
    {
        // find prefab with name "Scoop"
        GameObject scoopPrefab = Resources.Load<GameObject>("Scoop");

        // instantiate the prefab
        GameObject scoop = GameObject.Instantiate(scoopPrefab, submarine.transform);
        scoop.GetComponent<ScoopBehaviour>().Initialize(this);
        return scoop;
    }
}

public class Ballast : Activatable
{
    public delegate void BallastChange();
    public static event BallastChange OnBallastChanged;

    public float ballastMass = 0f; // Ballast mass in kilograms
    public float ballastDropTime = 0f; // Time to drop ballast in seconds
    public float ballastRefills = 0; // Number of times ballast can be refilled
    public float ballastRefillTime = 0f; // Time to refill ballast in seconds

    private bool ballastDropped = false; // Whether the ballast has been dropped
    private int ballastRefillsUsed = 0; // Number of times the ballast has been refilled

    public override bool Activate()
    {
        // it is always possible to drop the ballast
        if (!ballastDropped)
        {
            ballastDropped = true;
            OnBallastChanged?.Invoke();
            return true;
        }
        // it is only possible to refill the ballast if there are refills left
        else if (ballastRefillsUsed < ballastRefills)
        {
            ballastDropped = false;
            ballastRefillsUsed++;
            OnBallastChanged?.Invoke();
            return true;
        }
        return false;
    }

    public override float AddedMass()
    {
        return base.AddedMass() + (ballastDropped ? 0 : ballastMass);
    }
}

public class DepthController : Equipment
{
    public float depthControlMass = 0f; // Mass for depth control in kilograms
    public float depthControlTime = 0f; // Time to apply or remove full depth control mass in seconds

    private float currentDepthControlMass = 0f; // Current depth control mass in kilograms

    // returns true if the mass was adjusted, false otherwise
    public bool AdjustMass(bool adjustHeavier)
    {
        if (adjustHeavier && currentDepthControlMass >= depthControlMass)
        {
            return false;
        }
        if (!adjustHeavier && currentDepthControlMass <= 0)
        {
            return false;
        }
        float deltaMass = depthControlMass * Time.deltaTime / depthControlTime;
        if (adjustHeavier)
        {
            currentDepthControlMass += deltaMass;
        }
        else
        {
            currentDepthControlMass -= deltaMass;
        }
        return true;
    }

    public override float AddedMass()
    {
        return base.AddedMass() + currentDepthControlMass;
    }
}

public class Engine : Equipment
{
    public float thrust = 0f; // Thrust in newtons
}
