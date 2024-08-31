using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet
{
    public float gravity = 9.8f; // Gravity in m/s^2
}

public class Equipment
{
    public float mass = 0f; // Mass in kilograms
    public float addedVolume = 0f; // Added volume in cubic meters

    public float activationPower = 0f; // Instantaneous power cost to activate
    public float continuousPower = 0f; // Continuous power cost per second

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
    public abstract bool Activate();
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
