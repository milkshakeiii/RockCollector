using UnityEngine;

public class Tester : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AddAndRemovePlaceablesOfVariousSizes();
        Debug.Log("Tests finished");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void AddAndRemovePlaceablesOfVariousSizes()
    {
        Map map = new();
        
        // add placeables of various sizes
        Placeable referencedPlaceable = new Placeable(1);
        map.Add(new Placeable(-5), new Vector2Int(0, 0));
        map.Add(new Placeable(0), new Vector2Int(5, 5));
        map.Add(referencedPlaceable, new Vector2Int(10, 10));
        map.Add(new Placeable(2), new Vector2Int(15, 15));

        // check that the placeables were added correctly
        Assert(map.PlaceablesAt(new Vector2Int(10, 10)).Contains(referencedPlaceable), "Referenced placeable not found at position");
        Assert(map.PositionOf(referencedPlaceable) == new Vector2Int(10, 10), "Referenced placeable not found at correct position");
        Assert(map.PlaceablesAt(new Vector2Int(0, 0)).Count == 1, "Placeable of size -5 not found at position");
        Assert(map.PlaceablesAt(new Vector2Int(0, 0))[0].SquaresMinimumOne() == 1, "Placeable of size 1 not found at position");
        Assert(map.PlaceablesAt(new Vector2Int(5, 5)).Count == 1, "Placeable of size 0 not found at position");
        Assert(map.PlaceablesAt(new Vector2Int(15, 15)).Count == 1, "Placeable of size 2 not found at position");
        Assert(map.PlaceablesAt(new Vector2Int(16, 15)).Count == 1, "Placeable of size 2 not found at position");
        Assert(map.PlaceablesAt(new Vector2Int(15, 16)).Count == 1, "Placeable of size 2 not found at position");
        Assert(map.PlaceablesAt(new Vector2Int(16, 16)).Count == 1, "Placeable of size 2 not found at position");

        // remove placeables of various sizes
        map.Remove(map.PlaceablesAt(new Vector2Int(0, 0))[0]);
        map.Remove(map.PlaceablesAt(new Vector2Int(5, 5))[0]);
        map.Remove(referencedPlaceable);
        map.Remove(map.PlaceablesAt(new Vector2Int(15, 15))[0]);

        // check that the placeables were removed correctly
        Assert(map.CountAllPlaceables() == 0, "Placeables not removed correctly");
        Assert(map.CountOccupiedSquares() == 0, "Placeables not removed correctly");
    }

    void Assert(bool condition, string message)
    {
        if (!condition)
        {
            Debug.LogError(message);
        }
    }
}
