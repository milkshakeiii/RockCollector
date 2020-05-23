using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveManager : MonoBehaviour
{
    private static MoveManager instance;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    public static MoveManager GetInstance()
    {
        return instance;
    }

    public void ReportPieceClick(Piece piece)
    {
        Debug.Log(piece);
        Debug.Log(piece.PieceNumber());
    }

    public void ReportSquareClick(BoardSquare square)
    {
        Debug.Log(square);
        Debug.Log(square.GetBoardPosition());
    }
}
