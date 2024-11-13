using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Harpoon : MonoBehaviour
{
    public GameObject harpoonRopePrefab;

    private HarpoonGunBehaviour harpoonGunBehaviour;
    private bool stuck = false;
    private GameObject ropeSprite;

    public void Initialize(HarpoonGunBehaviour newHarpoonGunBehavior, float velocity)
    {
        harpoonGunBehaviour = newHarpoonGunBehavior;
        GetComponent<Rigidbody2D>().linearVelocity = this.transform.right * velocity;
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

        // destroy the harpoon if the rope is stretched too far
        if (stuck && GetComponent<MyJoint2D>().CheckBreak(harpoonGunBehaviour.harpoonGun.ropeElasticity, ropeSprite))
        {
            Destroy(this.gameObject);
        }

        // if the right mouse button is pressed, shorten the rope
        if (Input.GetMouseButton(1))
        {
            GetComponent<MyJoint2D>().distance -= harpoonGunBehaviour.harpoonGun.reelSpeed * Time.deltaTime;
            GetComponent<MyJoint2D>().distance = Mathf.Max(0, GetComponent<MyJoint2D>().distance);
            // pay the continuous cost of reeling in the harpoon
            SubmarineBehaviour submarine = harpoonGunBehaviour.GetComponentInParent<SubmarineBehaviour>();
            submarine.SpendPower(harpoonGunBehaviour.harpoonGun.continuousPower * Time.deltaTime);
        }
    }

    void OnDestroy()
    {
        if (ropeSprite != null)
        {
            Destroy(ropeSprite);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!stuck && collision.gameObject.layer == LayerMask.NameToLayer("Fish"))
        {
            // the harpoon sticks to the fish
            this.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            this.transform.SetParent(collision.transform);
            stuck = true;

            // make the fish still
            collision.gameObject.GetComponent<Timefish>().SetBehavior(new StillBehavior());

            // damage the fish
            collision.gameObject.GetComponent<Timefish>().TakeDamage(harpoonGunBehaviour.harpoonGun.damage);

            // create the rope sprite
            GameObject newRopeSprite = Instantiate(harpoonRopePrefab, this.transform.position, Quaternion.identity);
            this.ropeSprite = newRopeSprite;

            // get the fixedjoint2d component and connect it to the fish
            FixedJoint2D fixedJoint = this.gameObject.GetComponent<FixedJoint2D>();
            fixedJoint.connectedBody = collision.gameObject.GetComponent<Rigidbody2D>();
            fixedJoint.enabled = true;

            // get the MyJoint2D component and connect it to the harpoon gun
            MyJoint2D myJoint = this.gameObject.GetComponent<MyJoint2D>();
            myJoint.originBody = this.GetComponent<Rigidbody2D>();
            myJoint.connectedBody = harpoonGunBehaviour.GetComponentInParent<Rigidbody2D>();
            myJoint.enabled = true;
            myJoint.distance = Vector2.Distance(this.transform.position, harpoonGunBehaviour.transform.position);
            myJoint.strength = harpoonGunBehaviour.harpoonGun.pullStrength;

            // add a shortener script to this and initialize it
            RopeSpriteStretcher shortener = this.AddComponent<RopeSpriteStretcher>();
            shortener.Initialize(harpoonGunBehaviour.gameObject, newRopeSprite);
        }
    }
}
