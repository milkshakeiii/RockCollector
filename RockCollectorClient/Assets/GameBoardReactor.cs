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
        //clear old board
        int childCount = gameObject.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Destroy(gameObject.transform.GetChild(i).gameObject);
        }

        //parse board
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
            PlacePiece(pieceLine, playerReadyZone, capturedByEnemyZone, true);
        }
        for (int i = player2piecesLine + 1; i < player2piecesLine + 1 + pieceCount; i++)
        {
            string pieceLine = responseLines[i];
            PlacePiece(pieceLine, enemyReadyZone, capturedByPlayerZone, false);
        }
    }

    private GameObject PlacePiece(string pieceLine, OutOfPlayZone readyZone, OutOfPlayZone capturedZone, bool isPlayerPiece)
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

        int x;
        int y;
        if (positionLeft.Equals("(ready"))
        {
            readyZone.AddPiece(newPiece);
            if (isPlayerPiece)
            {
                x = -1;
                y = -1;
            }
            else
            {
                x = -2;
                y = -2;
            }
        }
        else if (positionLeft.Equals("(captured"))
        {
            capturedZone.AddPiece(newPiece);
            if (isPlayerPiece)
            {
                x = -3;
                y = -3;
            }
            else
            {
                x = -4;
                y = -4;
            }
        }
        else if (positionLeft.Equals("(destroyed"))
        {
            destroyedZone.AddPiece(newPiece);
            x = -5;
            y = -5;
        }
        else
        {
            x = int.Parse(positionLeft.Substring(1, positionLeft.Length - 2));
            y = int.Parse(positionRight.Substring(0, positionRight.Length - 1));
            newPiece.transform.position = LocalGameworldPosition(x, y);
            newPiece.transform.position = new Vector3(newPiece.transform.position.x,
                                                      newPiece.transform.position.y,
                                                      piece.transform.position.z);
        }

        newPiece.GetComponent<Piece>().Initialize(isPlayerPiece, int.Parse(pieceNumber), new Vector2Int(x, y));
        newPiece.transform.parent = gameObject.transform;
        return newPiece;
    }

    public Vector2 LocalGameworldPosition(int xcoord, int ycoord)
    {
        return new Vector2(xcoord + 0.5f, ycoord + 0.5f);
    }
}
