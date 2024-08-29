using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatableButtonSpawner : MonoBehaviour
{
    public GameObject activatableButtonPrefab;
    public Submarine submarine;

    // Start is called before the first frame update
    void Start()
    {
        List<Activatable> activatables = submarine.ActivatableModules();
        for (int i = 0; i < activatables.Count; i++)
        {
            GameObject activatableButton = Instantiate(activatableButtonPrefab, transform);
            // positions the button in a row from left to right
            activatableButton.transform.localPosition = new Vector3(-150 + i * 40, 0, 0);
            // Initialize the button with the submarine and the activatable module
            ActivatableButton button = activatableButton.GetComponent<ActivatableButton>();
            button.Initialize(submarine, activatables[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
