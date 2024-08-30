using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootablesManager : MonoBehaviour
{
    public GameObject shootablesImagePrefab;
    public Submarine submarine;

    private List<GameObject> images = new List<GameObject>();
    private List<GameObject> worldObjects = new List<GameObject>();
    private int currentShootableIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Equipment equipment in submarine.AllModules())
        {
            if (equipment is Shootable shootable)
            {
                GameObject shootablesImage = Instantiate(shootablesImagePrefab, transform);
                shootablesImage.GetComponent<ShootableImage>().Initialize(shootable);
                // deactivate the shootable image if it is not the first one
                if (images.Count > 0)
                {
                    shootablesImage.SetActive(false);
                }
                images.Add(shootablesImage);

                GameObject shootableWorldObject = shootable.SpawnWorldObject(submarine);
                // deactivate the shootable world object if it is not the first one
                if (worldObjects.Count > 0)
                {
                    shootableWorldObject.SetActive(false);
                }
                worldObjects.Add(shootableWorldObject);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // scroll through the shootables with the mouse wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            images[currentShootableIndex].SetActive(false);
            worldObjects[currentShootableIndex].SetActive(false);
            currentShootableIndex = (currentShootableIndex + images.Count + (scroll > 0 ? 1 : -1)) % images.Count;
            images[currentShootableIndex].SetActive(true);
            worldObjects[currentShootableIndex].SetActive(true);
        }
    }
}
