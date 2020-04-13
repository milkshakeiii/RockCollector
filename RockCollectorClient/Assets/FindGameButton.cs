using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindGameButton : MonoBehaviour
{
    public TMPro.TMP_InputField usernameInput;
    public TMPro.TMP_InputField wordpassInput;

    public void FindGame()
    {
        HttpCommunicator.GetInstance().FindGameRequest(usernameInput.text, wordpassInput.text);
    }
}
