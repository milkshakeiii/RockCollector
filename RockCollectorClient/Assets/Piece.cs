using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    private bool isPlayerPiece;
    private int pieceNumber;

    public void Initialize(bool newIsPlayerPiece, int newPieceNumber)
    {
        isPlayerPiece = newIsPlayerPiece;
        pieceNumber = newPieceNumber;
    }

    public bool IsPlayerPiece()
    {
        return isPlayerPiece;
    }

    public int PieceNumber()
    {
        return pieceNumber;
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
    }
}
