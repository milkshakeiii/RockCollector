using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeSpriteStretcher : MonoBehaviour
{
    private GameObject otherSide;
    private GameObject ropeSprite;

    public void Initialize(GameObject newOtherSide, GameObject newRopeSprite)
    {
        otherSide = newOtherSide;
        ropeSprite = newRopeSprite;
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector2.Distance(this.transform.position, otherSide.transform.position);
        ropeSprite.transform.localScale = new Vector3(distance, ropeSprite.transform.localScale.y, ropeSprite.transform.localScale.z);

        // put the rope sprite in the middle of the two objects
        ropeSprite.transform.position = (this.transform.position + otherSide.transform.position) / 2;
        
        // rotate the rope sprite to face the target
        Vector3 direction = otherSide.transform.position - this.transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        ropeSprite.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
