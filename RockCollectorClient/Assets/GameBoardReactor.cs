using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoardReactor : MonoBehaviour
{
    public GameObject onesquare;

    void Start()
    {
        HttpCommunicator.OnTakeTurnResponseEvent += TurnTaken;
        HttpCommunicator.OnCheckForGamestateResponseEvent += GamestateFound;
    }

    private void GamestateFound(string response)
    {
        string[] responseLines = response.Split('\n');
        if (responseLines[0].Equals("6"))
        {
            DisplayGamestate(response);
        }
    }
    
    private void TurnTaken(string response)
    {

    }

    private void DisplayGamestate(string gamestate_response)
    {
        string[] responseLines = gamestate_response.Split('\n');
        float width = responseLines[5].Length;
        float height = 0f;
        for (int i = 5; i < responseLines.Length; i++)
        {
            string line = responseLines[i];
            if (HttpCommunicator.BeforeTheColon(line).Equals("Game status"))
            {
                break;
            }
            height += 1f;
            for (int j = 0; j < line.Length; j++)
            {
                char squareno = line[j];
                GameObject newOnesquare = Instantiate(onesquare);
                if (((i + j) % 2) == 0)
                {
                    SpriteRenderer squareRenderer = newOnesquare.GetComponent<SpriteRenderer>();
                    squareRenderer.material.color = Color.gray;
                }
                newOnesquare.transform.parent = gameObject.transform;
                newOnesquare.transform.position = new Vector2(i - 5 + 0.5f, j + 0.5f);
            }
        }
        gameObject.transform.position = new Vector2(-width / 2, -height / 2);
    }
}
