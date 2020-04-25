using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginCanvas : MonoBehaviour
{
    public TMPro.TMP_Text infoText;

    // Start is called before the first frame update
    void Start()
    {
        HttpCommunicator.OnFindGameRequestEvent += ShowWaiting;
        HttpCommunicator.OnFindGameResponseEvent += ShowResponseText;
    }

    private void ChangeInfoText(string newText)
    {
        infoText.text = newText;
    }

    void ShowWaiting()
    {
        ChangeInfoText("Request sent...");
    }

    void ShowResponseText(string response)
    {
        ChangeInfoText(response);
    }
}
