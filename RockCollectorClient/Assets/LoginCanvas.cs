using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginCanvas : MonoBehaviour
{
    public TMPro.TMP_Text infoText;
    public GameObject findGameButton;
    public GameObject greyButton;

    // Start is called before the first frame update
    void Start()
    {
        HttpCommunicator.OnFindGameRequestEvent += HandleFindGameRequest;
        HttpCommunicator.OnFindGameResponseEvent += HandleFindGameResponse;
        HttpCommunicator.OnCheckForGamestateResponseEvent += GamestateFound;
    }

    private void GamestateFound(string response)
    {
        string[] responseLines = response.Split('\n');
        if (responseLines[0].Equals("6"))
        {
            gameObject.SetActive(false);
        }
    }

    private void ChangeInfoText(string newText)
    {
        infoText.text = newText;
    }

    void HandleFindGameRequest()
    {
        ChangeInfoText("Request sent...");
        findGameButton.SetActive(false);
        greyButton.SetActive(true);
    }

    void HandleFindGameResponse(string response)
    {
        ChangeInfoText(response);
        string[] responseLines = response.Split('\n');
        if (responseLines[0].Equals("0"))
        {
            findGameButton.SetActive(true);
            greyButton.SetActive(false);
        }
    }
}
