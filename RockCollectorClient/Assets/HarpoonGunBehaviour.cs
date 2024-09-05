using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarpoonGunBehaviour : MonoBehaviour
{
    public GameObject harpoonPrefab;
    public HarpoonGun harpoonGun;

    private List<GameObject> harpoons = new();

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
        // spawn harpoon on left mouse click if harpoons are available
        // clear destroyed harpoons
        harpoons.RemoveAll(harpoon => harpoon == null);
        bool harpoonsAvailable = harpoons.Count < harpoonGun.maxHarpoons;
        if (harpoonsAvailable && Input.GetMouseButtonDown(0))
        {
            // pay the activation power cost
            Submarine submarine = transform.parent.GetComponent<Submarine>();
            submarine.SpendPower(harpoonGun.activationPower);

            GameObject harpoon = Instantiate(harpoonPrefab, transform.position, transform.rotation);
            bool facingRight = transform.parent.localScale.x > 0;
            float velocity = harpoonGun.velocity;
            if (!facingRight)
            {
                harpoon.transform.localScale = new Vector3(harpoon.transform.localScale.x * -1, harpoon.transform.localScale.y, harpoon.transform.localScale.z);
                velocity *= -1;
            }
            harpoon.GetComponent<Harpoon>().Initialize(this, velocity);
            harpoons.Add(harpoon);
        }
    }

    private void OnDisable()
    {
        // destroy all harpoons when the harpoon gun is disabled
        foreach (GameObject harpoon in harpoons)
        {
            Destroy(harpoon);
        }
    }

    public List<Timefish> HarpoonedFish()
    {
        List<Timefish> harpoonedFish = new();
        foreach (GameObject harpoon in harpoons)
        {
            // look for Timefish component in parents of the harpoon
            Timefish timefish = harpoon.GetComponentInParent<Timefish>();
            if (timefish != null)
            {
                harpoonedFish.Add(timefish);
            }
        }
        return harpoonedFish;
    }
}
