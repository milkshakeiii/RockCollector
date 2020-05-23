using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    private string gameUuid = null;
    private int nextTurnNumber = 0;

    public static GameManager GetInstance()
    {
        return instance;
    }

    void Start()
    {
        instance = this;
        HttpCommunicator.OnFindGameResponseEvent += StartCheckingForGames;
        HttpCommunicator.OnCheckForGamestateResponseEvent += GamestateFound;
    }

    public void TakeTurnRequest(int sourceRockIndex,
                                int targetX,
                                int targetY)
    {
        HttpCommunicator.GetInstance().TakeTurnRequest(gameUuid,
                                                       nextTurnNumber.ToString(),
                                                       sourceRockIndex.ToString(),
                                                       targetX.ToString(),
                                                       targetY.ToString());
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
