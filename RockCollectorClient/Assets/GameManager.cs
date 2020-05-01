using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private string gameUuid = null;
    private bool gameActive = false;
    private int nextTurnNumber = 0;

    void Start()
    {
        HttpCommunicator.OnFindGameResponseEvent += StartCheckingForGames;
        HttpCommunicator.OnCheckForGamestateResponseEvent += GamestateFound;
    }

    private void StartCheckingForGames(string findGameResponse)
    {
        string[] responseLines = findGameResponse.Split('\n');
        if (responseLines[0].Equals("4") || 
            responseLines[0].Equals("3") ||
            responseLines[0].Equals("2"))
        {
            string secondLine = responseLines[1];
            gameUuid = secondLine.Split(':')[1].Substring(1);
            StartCoroutine(DoCheckForGamestate());
        }
    }

    private IEnumerator DoCheckForGamestate()
    {
        while (true)
        {
            HttpCommunicator.GetInstance().CheckForGamestateRequest(gameUuid, nextTurnNumber.ToString());
            yield return new WaitForSeconds(5f);
        }
    }

    private void GamestateFound(string response)
    {
        Debug.Log(response);
        string[] responseLines = response.Split('\n');
        if (responseLines[0].Equals("6"))
        {
            nextTurnNumber += 1;
        }
    }
}
