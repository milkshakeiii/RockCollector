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
                newOnesquare.transform.position = LocalGameworldPosition(i-5, j);
                newOnesquare.GetComponent<BoardSquare>().Initialize(new Vector2Int(i-5, j));
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
            GameObject newPiece = PlacePiece(pieceLine, playerReadyZone, capturedByEnemyZone);
            newPiece.GetComponent<Piece>().Initialize(true, i - player1piecesLine - 1);
        }
        for (int i = player2piecesLine + 1; i < player2piecesLine + 1 + pieceCount; i++)
        {
            string pieceLine = responseLines[i];
            GameObject newPiece = PlacePiece(pieceLine, enemyReadyZone, capturedByPlayerZone);
            newPiece.GetComponent<Piece>().Initialize(false, i - player2piecesLine - 1);
        }
    }

    private GameObject PlacePiece(string pieceLine, OutOfPlayZone readyZone, OutOfPlayZone capturedZone)
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
        else if (positionLeft.Equals("(captured"))
        {
            capturedZone.AddPiece(newPiece);
        }
        else if (positionLeft.Equals("(destroyed"))
        {
            destroyedZone.AddPiece(newPiece);
        }
        else
        {
            int x = int.Parse(positionLeft.Substring(1));
            int y = int.Parse(positionRight.Substring(0, positionRight.Length - 1));
            newPiece.transform.position = LocalGameworldPosition(x, y);
        }

        return newPiece;
    }

    public Vector2 LocalGameworldPosition(int xcoord, int ycoord)
    {
        return new Vector2(xcoord + 0.5f, ycoord + 0.5f);
    }
}
