using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarpoonGunBehaviour : MonoBehaviour
{
    private HarpoonGun harpoonGun;

    public void Initialize(HarpoonGun harpoonGun)
    {
        this.harpoonGun = harpoonGun;
        transform.localScale = new Vector3(harpoonGun.size * 0.2f, harpoonGun.size * 0.05f, 1);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
