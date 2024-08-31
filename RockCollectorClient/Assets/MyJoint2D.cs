using UnityEngine;

public class MyJoint2D : MonoBehaviour
{
    public Rigidbody2D originBody;
    public Rigidbody2D connectedBody;
    public float distance;
    public float strength;

    void FixedUpdate()
    {
        float currentDistance = Vector2.Distance(originBody.transform.position, connectedBody.transform.position);
        
        // if the current distance is greater than the desired distance, pull both objects towards each other
        if (currentDistance > distance)
        {
            Vector2 direction = connectedBody.transform.position - originBody.transform.position;
            originBody.AddForce(direction.normalized * (currentDistance - distance) * strength);
            connectedBody.AddForce(-direction.normalized * (currentDistance - distance) * strength);
        }
    }

    public bool CheckBreak(float elasticity, GameObject ropeSprite)
    {
        float currentDistance = Vector2.Distance(originBody.transform.position, connectedBody.transform.position);
        float stretchFactor = Mathf.Abs(currentDistance - distance) / distance;
        bool stretched = stretchFactor > elasticity;
        bool withinGraceDistance = currentDistance < 1f;

        // color the rope red if it's about to break, otherwise color it white
        float howCoseToBreak = (elasticity - stretchFactor) / (elasticity);
        ropeSprite.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.red, Color.white, howCoseToBreak);

        return stretched && !withinGraceDistance;
    }
}
