using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpAndDownIndicatorArrow : MonoBehaviour
{
    public float minY;
    public float maxY;

    public void SetHeight(float height)
    {
        // height is a value between 0 and 1
        float y = minY + (maxY - minY) * height;
        // if the change is less than 3, don't bother updating the display
        // if (Mathf.Abs(y - transform.localPosition.y) > 3f)
        transform.localPosition = new Vector3(transform.localPosition.x, y, transform.localPosition.z);
    }
}
