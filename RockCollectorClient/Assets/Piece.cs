using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    private bool isPlayerPiece;
    private int pieceNumber;
    private Vector2Int position;

    public void Initialize(bool newIsPlayerPiece, int newPieceNumber, Vector2Int newPosition)
    {
        isPlayerPiece = newIsPlayerPiece;
        pieceNumber = newPieceNumber;
        position = newPosition;
    }

    public bool IsPlayerPiece()
    {
        return isPlayerPiece;
    }

    public int PieceNumber()
    {
        return pieceNumber;
    }

    Vector2Int Position()
    {
        return position;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        MoveManager.GetInstance().ReportPieceClick(this);
        MoveManager.GetInstance().ReportSquareClick(position);
    }
}
