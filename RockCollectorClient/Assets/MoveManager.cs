using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveManager : MonoBehaviour
{
    public GameObject cursorObject;

    private static MoveManager instance;
    private bool moveClickActive = false;
    private bool squareClicked = false;
    private Vector2Int lastSquareClicked;
    private bool userIsPlayer1 = true;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    public void SetUserIsPlayerOne(bool isPlayerOne)
    {
        userIsPlayer1 = isPlayerOne;
    }

    public static MoveManager GetInstance()
    {
        return instance;
    }

    public void ReportPieceClick(Piece piece)
    {
        if (!moveClickActive && (piece.IsPlayerPiece() == userIsPlayer1))
        {
            StartCoroutine(DoMoveClick(piece));
        }
    }

    public void ReportSquareClick(Vector2Int position)
    {
        if (moveClickActive)
            squareClicked = true;
        lastSquareClicked = position;
    }

    private IEnumerator DoMoveClick(Piece piece)
    {
        yield return null;
        moveClickActive = true;
        UnityEngine.Cursor.visible = false;
        cursorObject.SetActive(true);
        while (!squareClicked && !Input.GetMouseButtonDown(1))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
            cursorObject.transform.position = new Vector3(mousePos.x,
                                                          mousePos.y,
                                                          cursorObject.transform.position.z);
            yield return null;
        }
        if (squareClicked)
        {
            squareClicked = false;
            GameManager.GetInstance().TakeTurnRequest(piece.PieceNumber(),
                                                      lastSquareClicked.x,
                                                      lastSquareClicked.y);
        }
        UnityEngine.Cursor.visible = true;
        cursorObject.SetActive(false);
        moveClickActive = false;
    }
}
