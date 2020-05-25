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
    public Sprite circle;
    public Sprite hook;
    public Sprite lump;
    public Sprite star;
    public Sprite square;
    public Sprite triangle;

    void Start()
    {
        HttpCommunicator.OnTakeTurnResponseEvent += TurnTaken;
        HttpCommunicator.OnCheckForGamestateResponseEvent += GamestateFound;
    }

    private void GamestateFound(string response, string username)
    {
        string[] responseLines = response.Split('\n');
        if (responseLines[0].Equals("6"))
        {
            DisplayGamestate(response, username);
        }
    }
    
    private void TurnTaken(string response, string username)
    {

    }

    private void DisplayGamestate(string gamestate_response, string username)
    {

        //clear old board
        int childCount = gameObject.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Destroy(gameObject.transform.GetChild(i).gameObject);
        }
        playerReadyZone.ClearPieces();
        capturedByPlayerZone.ClearPieces();
        enemyReadyZone.ClearPieces();
        capturedByEnemyZone.ClearPieces();
        destroyedZone.ClearPieces();

        //parse board
        string[] responseLines = gamestate_response.Split('\n');
        string player1username = responseLines[2].Split(' ')[0];
        bool iAmPlayer1 = username.Equals(player1username);
        MoveManager.GetInstance().SetUserIsPlayerOne(iAmPlayer1);
        int width = responseLines[5].Length;
        int height = 0;
        for (int i = 5; i < responseLines.Length; i++)
        {
            string line = responseLines[i];
            if (HttpCommunicator.BeforeTheColon(line).Equals("Game status"))
            {
                break;
            }
            height += 1;
        }
        for (int i = 5; i < 5 + height; i++)
        {
            string line = responseLines[i];
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
                newOnesquare.transform.position = LocalGameworldPosition(i-5, j, width, height);
                newOnesquare.GetComponent<BoardSquare>().Initialize(new Vector2Int(i-5, j));
            }
        }

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
            PlacePiece(pieceLine, playerReadyZone, capturedByEnemyZone, true, width, height);
        }
        for (int i = player2piecesLine + 1; i < player2piecesLine + 1 + pieceCount; i++)
        {
            string pieceLine = responseLines[i];
            PlacePiece(pieceLine, enemyReadyZone, capturedByPlayerZone, false, width, height);
        }
    }

    private void PlacePiece(string pieceLine,
                            OutOfPlayZone readyZone,
                            OutOfPlayZone capturedZone,
                            bool isPlayerPiece,
                            float width,
                            float height)
    {
        string[] halves = pieceLine.Split(':');
        string pieceNumber = halves[0];
        string pieceData = halves[1];
        string[] dataParts = pieceData.Split(' ');
        string color = dataParts[1];
        string shape = dataParts[2];
        string material = dataParts[3];
        int positionCount = (dataParts.Length - 6)/2 + 1;

        for (int i = 0; i < positionCount; i++)
        {
            GameObject newPiece = Instantiate(piece);

            string positionLeft = dataParts[4 + i * 2];
            string positionRight = dataParts[5 + i * 2];
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
                if (positionCount > 1 && i != positionCount - 1)
                {
                    y = int.Parse(positionRight.Substring(0, positionRight.Length - 2));
                }
                else
                {
                    y = int.Parse(positionRight.Substring(0, positionRight.Length - 1));
                }
                newPiece.transform.position = LocalGameworldPosition(x, y, width, height);
                newPiece.transform.position = new Vector3(newPiece.transform.position.x,
                                                          newPiece.transform.position.y,
                                                          piece.transform.position.z);
            }

            newPiece.GetComponent<Piece>().Initialize(isPlayerPiece, int.Parse(pieceNumber), new Vector2Int(x, y));
            newPiece.transform.parent = gameObject.transform;

            //color, shape, material
            if (shape.Equals("CIRCULAR"))
            {
                newPiece.GetComponent<SpriteRenderer>().sprite = circle;
            }
            if (shape.Equals("HOOKED"))
            {
                newPiece.GetComponent<SpriteRenderer>().sprite = hook;
            }
            if (shape.Equals("STARSHAPED"))
            {
                newPiece.GetComponent<SpriteRenderer>().sprite = star;
            }
            if (shape.Equals("LUMPY"))
            {
                newPiece.GetComponent<SpriteRenderer>().sprite = lump;
            }
            if (shape.Equals("SQUARE"))
            {
                newPiece.GetComponent<SpriteRenderer>().sprite = square;
            }
            if (shape.Equals("TRIANGULAR"))
            {
                newPiece.GetComponent<SpriteRenderer>().sprite = triangle;
            }
            if (color.Equals("RED"))
            {
                newPiece.GetComponent<SpriteRenderer>().color = Color.red;
            }
            if (color.Equals("GREY"))
            {
                newPiece.GetComponent<SpriteRenderer>().color = Color.white*0.8f;
            }
            if (color.Equals("BLUE"))
            {
                newPiece.GetComponent<SpriteRenderer>().color = Color.blue;
            }
            if (!isPlayerPiece)
            {
                newPiece.GetComponent<SpriteRenderer>().color = newPiece.GetComponent<SpriteRenderer>().color*0.65f;
            }
        }
    }

    private Vector2 LocalGameworldPosition(int xcoord, int ycoord, float width, float height)
    {
        return new Vector2(xcoord + 0.5f, ycoord + 0.5f) + new Vector2(-width / 2, -height / 2);
    }
}

