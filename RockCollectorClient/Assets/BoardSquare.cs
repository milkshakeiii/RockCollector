using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardSquare : MonoBehaviour
{
    private Vector2Int boardPosition;

    public void Initialize(Vector2Int newBoardPosition)
    {
        boardPosition = newBoardPosition;
    }

    public Vector2Int GetBoardPosition()
    {
        return boardPosition;
    }

    private void OnMouseDown()
    {
        MoveManager.GetInstance().ReportSquareClick(boardPosition);
    }
}
