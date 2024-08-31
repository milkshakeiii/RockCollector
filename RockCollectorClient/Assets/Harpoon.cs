using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Harpoon : MonoBehaviour
{
    public GameObject harpoonRopePrefab;

    private HarpoonGunBehaviour harpoonGunBehaviour;
    private bool stuck = false;

    public void Initialize(HarpoonGunBehaviour newHarpoonGunBehavior, float velocity)
    {
        harpoonGunBehaviour = newHarpoonGunBehavior;
        GetComponent<Rigidbody2D>().velocity = this.transform.right * velocity;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // destroy the harpoon if it goes out of range
        if (!stuck && Vector2.Distance(this.transform.position, harpoonGunBehaviour.transform.position) > harpoonGunBehaviour.harpoonGun.range)
        {
            Destroy(this.gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!stuck && collision.gameObject.layer == LayerMask.NameToLayer("Fish"))
        {
            // the harpoon sticks to the fish
            this.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            this.GetComponent<Rigidbody2D>().isKinematic = true;
            this.transform.SetParent(collision.transform);
            stuck = true;

            // make the fish still
            collision.gameObject.GetComponent<Timefish>().SetBehavior(new StillBehavior());

            // create the rope sprite
            GameObject ropeSprite = Instantiate(harpoonRopePrefab, this.transform.position, Quaternion.identity);

            // get the fixedjoint2d component and connect it to the fish
            FixedJoint2D fixedJoint = this.gameObject.GetComponent<FixedJoint2D>();
            fixedJoint.connectedBody = collision.gameObject.GetComponent<Rigidbody2D>();
            fixedJoint.enabled = true;

            // get the distancejoint2d component and connect it to the harpoon gun
            DistanceJoint2D distanceJoint = this.gameObject.GetComponent<DistanceJoint2D>();
            distanceJoint.connectedBody = harpoonGunBehaviour.GetComponentInParent<Rigidbody2D>();
            distanceJoint.anchor = harpoonGunBehaviour.transform.localPosition;
            distanceJoint.enabled = true;
            distanceJoint.distance = Vector2.Distance(this.transform.position, harpoonGunBehaviour.transform.position);

            // add a shortener script to this and initialize it
            Shortener shortener = this.AddComponent<Shortener>();
            shortener.Initialize(harpoonGunBehaviour.gameObject, ropeSprite, harpoonGunBehaviour.harpoonGun.reelSpeed);
        }
    }
}
