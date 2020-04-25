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
        gameUuid = findGameResponse.Split(':')[1].Substring(1);
        StartCoroutine(DoCheckForGamestate());
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
        nextTurnNumber += 1;
    }
}
