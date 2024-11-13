using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoopBehaviour : MonoBehaviour
{
    private Scoop scoop;

    private bool scooping = false;

    public void Initialize(Scoop newScoop)
    {
        scoop = newScoop;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // scoop on left mouse click
        if (!scooping && Input.GetButton("button1"))
        {
            StartCoroutine(DoScoop());
        }
    }

    private IEnumerator DoScoop()
    {
        scooping = true;

        // get all timefish overlapping with this scoop's CircleCollider2D
        CircleCollider2D scoopCollider = GetComponent<CircleCollider2D>();
        // use scoopCollider.Overlap
        List<Collider2D> overlappingColliders = new();
        ContactFilter2D contactFilter = new();
        contactFilter.NoFilter();
        scoopCollider.Overlap(contactFilter, overlappingColliders);
        List<Timefish> overlappingFish = new();
        foreach (Collider2D collider in overlappingColliders)
        {
            if (collider.TryGetComponent<Timefish>(out var fish))
            {
                overlappingFish.Add(fish);
            }
        }
        // get the fish that are wholly contained within the scoop
        List<Timefish> capturedFish = new();
        foreach (Timefish fish in overlappingFish)
        {
            float scoopRadius = scoopCollider.radius;
            scoopRadius *= Mathf.Abs(transform.lossyScale.x); // scale the radius by the scoop's scale
            CapsuleCollider2D fishCollider = fish.GetComponent<CapsuleCollider2D>();
            float fishXDimension = fishCollider.size.x * fish.transform.lossyScale.x;
            float fishYDimension = fishCollider.size.y * fish.transform.lossyScale.y;
            float fishLongDimension = Mathf.Max(fishXDimension, fishYDimension);
            if (Vector2.Distance(transform.position, fish.transform.position) + fishLongDimension < scoopRadius * 2)
            {
                capturedFish.Add(fish);
            }
            // any fish with size greater than the scoop's diameter will prevent the scoop from capturing any fish
            else if (fish.Size() > scoop.scoopDiameter)
            {
                capturedFish.Clear();
                break;
            }
        }

        // pay the activation power cost
        Submarine submarine = GetComponentInParent<Submarine>();
        submarine.SpendPower(scoop.activationPower);

        // TODO: visually close the scoop
        GetComponent<SpriteRenderer>().color = Color.black;

        // disable the captured fish and add them to the submarine's cargo
        foreach (Timefish fish in capturedFish)
        {
            submarine.AddFish(fish);
        }

        // if there are no captured fish, skip waiting
        if (capturedFish.Count != 0)
        {
            yield return new WaitForSeconds(scoop.scoopTime);
        }
        else
        {
            yield return new WaitForSeconds(0.1f);
        }

        // TODO: visually open the scoop
        GetComponent<SpriteRenderer>().color = Color.white;

        scooping = false;
    }
}
