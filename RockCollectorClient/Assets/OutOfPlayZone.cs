using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfPlayZone : MonoBehaviour
{
    public float horizontalGap = 0.5f;
    public float verticalGap = 0.5f;
    public int piecesPerRow = 4;
    private List<GameObject> pieces = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClearPieces()
    {
        pieces = new List<GameObject>();
    }

    public void AddPiece(GameObject piece)
    {
        pieces.Add(piece);
        int rowCount = pieces.Count / piecesPerRow;
        float startX = gameObject.transform.position.x - pieces.Count % piecesPerRow * horizontalGap;
        float startY = gameObject.transform.position.y - rowCount * horizontalGap;
        for (int i = 0; i < pieces.Count; i++)
        {
            GameObject placeMe = pieces[i];
            placeMe.transform.position = new Vector2(startX + i % piecesPerRow, startY - i / piecesPerRow);
        }
    }
}
