using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shortener : MonoBehaviour
{
    private GameObject shortenTarget;
    private GameObject ropeSprite;

    private float reelSpeed = 1f;

    public void Initialize(GameObject newShortenTarget, GameObject newRopeSprite, float newReelSpeed)
    {
        shortenTarget = newShortenTarget;
        ropeSprite = newRopeSprite;
        reelSpeed = newReelSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector2.Distance(this.transform.position, shortenTarget.transform.position);
        ropeSprite.transform.localScale = new Vector3(distance, ropeSprite.transform.localScale.y, ropeSprite.transform.localScale.z);

        // put the rope sprite in the middle of the two objects
        ropeSprite.transform.position = (this.transform.position + shortenTarget.transform.position) / 2;
        
        // rotate the rope sprite to face the target
        Vector3 direction = shortenTarget.transform.position - this.transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        ropeSprite.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // if the right mouse button is pressed, shorten the rope
        if (Input.GetMouseButton(1))
        {
            GetComponent<DistanceJoint2D>().distance -= reelSpeed * Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {

    }
}
