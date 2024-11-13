using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform submarine;
    public Transform battleEnvironment;

    private Transform target;

    // Start is called before the first frame update
    void Start()
    {
        target = submarine;
    }

    public void JumpToBattle()
    {
        target = battleEnvironment;
    }

    public void JumpToSubmarine()
    {
        target = submarine;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = new Vector3(target.position.x, target.position.y, this.transform.position.z);
    }
}
