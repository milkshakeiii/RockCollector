using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModulesSpawner : MonoBehaviour
{
    public GameObject activatableButtonPrefab;
    public GameObject activatableButtonParent;

    public GameObject shootablesImagePrefab;
    public GameObject shootablesImageParent;

    public SubmarineBehaviour submarine;

    private List<GameObject> activatablesImages = new List<GameObject>();
    private int currentActivatableIndex = 0;

    private List<GameObject> shootablesImages = new List<GameObject>();
    private List<GameObject> shootablesWorldObjects = new List<GameObject>();
    private int currentShootableIndex = 0;

    private bool hotSwapMode = false;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Equipments equipment in submarine.AllModules())
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
            ActivatableButton button = activatableButton.GetComponent<ActivatableButton>();
            button.Initialize(submarine, activatables[i]);
            if (i > 0)
            {
                activatableButton.SetActive(false);
            }
            activatablesImages.Add(activatableButton);
        }

        foreach (Equipments equipment in submarine.AllModules())
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
        if (Input.GetButtonUp("button3"))
        {
            Debug.Log("Toggling hot swap mode");
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
        // scroll through the activatables with button2 if hot swap mode is on
        if (hotSwapMode && Input.GetButtonDown("button2") && activatablesImages.Count > 1)
        {
            activatablesImages[currentActivatableIndex].SetActive(false);
            currentActivatableIndex = (currentActivatableIndex + activatablesImages.Count + 1) % activatablesImages.Count;
            activatablesImages[currentActivatableIndex].SetActive(true);
        }
    }
}
