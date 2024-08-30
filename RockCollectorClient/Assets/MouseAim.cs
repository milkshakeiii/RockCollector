using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseAim : MonoBehaviour
{
    public float minAngle = 0f;
    public float maxAngle = 180.0f;
    public bool invert = false;
    public float rotationSpeed = 60.0f;

    private bool facingRight = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        bool currentFacingRight = transform.lossyScale.x > 0;
        if (currentFacingRight != facingRight)
        {
            facingRight = currentFacingRight;
            // reflect the min and max angles about the y-axis
            float temp = minAngle;
            minAngle = 180.0f - maxAngle;
            maxAngle = 180.0f - temp;
        }

        // rotate this object to face the mouse cursor in 2D
        // assume the mouse cursor has a z value of 0
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 0.0f;
        Vector3 objectPos = Camera.main.WorldToScreenPoint(transform.position);
        mousePos.x = mousePos.x - objectPos.x;
        mousePos.y = mousePos.y - objectPos.y;
        float targetAngle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
        bool shouldRotate = targetAngle < maxAngle && targetAngle > minAngle;
        if (invert)
        {
            shouldRotate = !shouldRotate;
        }
        if (shouldRotate)
        {
            float currentAngle = transform.localEulerAngles.z;
            float deltaAngle = Mathf.DeltaAngle(currentAngle, targetAngle);
            if (deltaAngle > 180.0f)
            {
                deltaAngle -= 360.0f;
            }
            float newAngle = currentAngle + Mathf.Sign(deltaAngle) * Mathf.Min(Mathf.Abs(deltaAngle), rotationSpeed * Time.deltaTime);
            transform.localEulerAngles = new Vector3(0, 0, newAngle);
        }
    }
}
