using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindGameButton : MonoBehaviour
{
    public TMPro.TMP_InputField usernameInput;
    public TMPro.TMP_InputField wordpassInput;

    public void FindGame()
    {
        if (usernameInput.text.Length > 0 && wordpassInput.text.Length > 0)
            HttpCommunicator.GetInstance().FindGameRequest(usernameInput.text, wordpassInput.text);
        else
            wordpassInput.text = "dog";
    }
}
