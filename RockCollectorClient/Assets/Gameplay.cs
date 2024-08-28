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

public class Ballast : Equipment
{
    public float ballastMass = 0f; // Ballast mass in kilograms
    public float ballastDropTime = 0f; // Time to drop ballast in seconds
    public float ballastRefills = 0; // Number of times ballast can be refilled
    public float ballastRefillTime = 0f; // Time to refill ballast in seconds

    private bool ballastDropped = false; // Whether the ballast has been dropped
    private int ballastRefillsUsed = 0; // Number of times the ballast has been refilled

    public bool ActivateBallast()
    {
        // it is always possible to drop the ballast
        if (!ballastDropped)
        {
            ballastDropped = true;
            return true;
        }
        // it is only possible to refill the ballast if there are refills left
        else if (ballastRefillsUsed < ballastRefills)
        {
            ballastDropped = false;
            ballastRefillsUsed++;
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

    public float AdjustMass(bool adjustHeavier)
    {
        float deltaMass = depthControlMass * Time.deltaTime / depthControlTime;
        if (adjustHeavier)
        {
            currentDepthControlMass += deltaMass;
        }
        else
        {
            currentDepthControlMass -= deltaMass;
        }
        currentDepthControlMass = Mathf.Clamp(currentDepthControlMass, 0, depthControlMass);
        return currentDepthControlMass;
    }

    public override float AddedMass()
    {
        return base.AddedMass() + currentDepthControlMass;
    }
}
