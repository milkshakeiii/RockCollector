using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoardReactor : MonoBehaviour
{
    public GameObject onesquare;
    public GameObject piece;
    public OutOfPlayZone playerReadyZone;
    public OutOfPlayZone capturedByPlayerZone;
    public OutOfPlayZone enemyReadyZone;
    public OutOfPlayZone capturedByEnemyZone;
    public OutOfPlayZone destroyedZone;

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
        int player1piecesLine = -1;
        int player2piecesLine = -1;
        for (int i = 5; i < responseLines.Length; i++)
        {
            string line = responseLines[i];
            if (line.Equals("Player 1 rocks:"))
            {
                player1piecesLine = i;
            }
            if (line.Equals("Player 2 rocks:"))
            {
                player2piecesLine = i;
            }
        }
        int pieceCount = player2piecesLine - player1piecesLine - 1;
        for (int i = player1piecesLine + 1; i < player2piecesLine; i++)
        {
            string pieceLine = responseLines[i];
            PlacePiece(pieceLine, playerReadyZone, capturedByEnemyZone);
        }
        for (int i = player2piecesLine + 1; i < player2piecesLine + 1 + pieceCount; i++)
        {
            string pieceLine = responseLines[i];
            PlacePiece(pieceLine, enemyReadyZone, capturedByPlayerZone);
        }
    }

    private void PlacePiece(string pieceLine, OutOfPlayZone readyZone, OutOfPlayZone capturedZone)
    {
        string[] halves = pieceLine.Split(':');
        string pieceNumber = halves[0];
        string pieceData = halves[1];
        string[] dataParts = pieceData.Split(' ');
        string color = dataParts[1];
        string shape = dataParts[2];
        string material = dataParts[3];
        string positionLeft = dataParts[4];
        string positionRight = dataParts[5];

        GameObject newPiece = Instantiate(piece);

        if (positionLeft.Equals("(ready"))
        {
            readyZone.AddPiece(newPiece);
        }
        if (positionLeft.Equals("(captured"))
        {
            capturedZone.AddPiece(newPiece);
        }
        if (positionLeft.Equals("(destroyed"))
        {
            destroyedZone.AddPiece(newPiece);
        }
    }

    public Vector2 LocalGameworldPosition(int xcoord, int ycoord)
    {
        return new Vector2(xcoord - 5 + 0.5f, ycoord + 0.5f);
    }
}
