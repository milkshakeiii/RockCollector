using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoardReactor : MonoBehaviour
{
    public GameObject onesquare;
    public GameObject piece;

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

        //parse board
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
                newOnesquare.transform.position = LocalGameworldPosition(i, j);
            }
        }
        gameObject.transform.position = new Vector2(-width / 2, -height / 2);

        //parse pieces


    }

    public Vector2 LocalGameworldPosition(int xcoord, int ycoord)
    {
        return new Vector2(xcoord - 5 + 0.5f, ycoord + 0.5f);
    }
}
