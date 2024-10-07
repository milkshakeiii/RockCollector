using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModulesSpawner : MonoBehaviour
{
    public GameObject activatableButtonPrefab;
    public GameObject activatableButtonParent;

    public GameObject shootablesImagePrefab;
    public GameObject shootablesImageParent;

    public Submarine submarine;

    private List<GameObject> shootablesImages = new List<GameObject>();
    private List<GameObject> shootablesWorldObjects = new List<GameObject>();
    private int currentShootableIndex = 0;

    private bool hotSwapMode = false;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Equipment equipment in submarine.AllModules())
        {
            if (equipment is Shootable shootable)
            {
                GameObject shootablesImage = Instantiate(shootablesImagePrefab, shootablesImageParent.transform);
                shootablesImage.GetComponent<ShootableImage>().Initialize(shootable);
                // deactivate the shootable image if it is not the first one
                if (shootablesImages.Count > 0)
                {
                    shootablesImage.SetActive(false);
                }
                shootablesImages.Add(shootablesImage);

                GameObject shootableWorldObject = shootable.SpawnWorldObject(submarine);
                // deactivate the shootable world object if it is not the first one
                if (shootablesWorldObjects.Count > 0)
                {
                    shootableWorldObject.SetActive(false);
                }
                shootablesWorldObjects.Add(shootableWorldObject);
            }
        }

        List<Activatable> activatables = submarine.ActivatableModules();
        for (int i = 0; i < activatables.Count; i++)
        {
            GameObject activatableButton = Instantiate(activatableButtonPrefab, activatableButtonParent.transform);
            // positions the button in a row from left to right
            activatableButton.transform.localPosition = new Vector3(-150 + i * 40, 0, 0);
            // Initialize the button with the submarine and the activatable module
            ActivatableButton button = activatableButton.GetComponent<ActivatableButton>();
            button.Initialize(submarine, activatables[i]);
        }

        foreach (Equipment equipment in submarine.AllModules())
        {
            if (equipment is Scoop scoop)
            {

            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // toggle hot swap mode with button3
        if (Input.GetButtonDown("button3"))
        {
            hotSwapMode = !hotSwapMode;
        }
        // scroll through the shootables with button1 if hot swap mode is on
        if (hotSwapMode && Input.GetButtonDown("button1") && shootablesImages.Count > 1)
        {
            shootablesImages[currentShootableIndex].SetActive(false);
            shootablesWorldObjects[currentShootableIndex].SetActive(false);
            currentShootableIndex = (currentShootableIndex + shootablesImages.Count + 1) % shootablesImages.Count;
            shootablesImages[currentShootableIndex].SetActive(true);
            shootablesWorldObjects[currentShootableIndex].SetActive(true);
        }
    }
}
