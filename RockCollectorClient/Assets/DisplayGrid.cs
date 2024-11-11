using System.Collections.Generic;
using UnityEngine;

public class DisplayGrid : MonoBehaviour
{
    public const int WIDTH = 240;
    public const int HEIGHT = 135;

    public GameObject baseSquarePrefab;

    private Dictionary<string, List<GameObject>> cachedSprites = new();


    void OnEnable()
    {
        DisplaySprite("Art/UI/plain_white", 0, 0, WIDTH/3, HEIGHT, 0);
    }

    void Update()
    {
        
    }

    /// <summary>
    /// Display a sprite on the grid
    /// </summary>
    /// <param name="spriteName"> Sprite name from resources</param>
    /// <param name="x"> X position in cells (8 px per cell)</param>
    /// <param name="y"> Y position in cells (8 px per cell)</param>
    /// <param name="width"> Width in cells (8 px per cell)</param>
    /// <param name="height"> Height in cells (8 px per cell)</param>
    /// <param name="rotation"> Rotation in 90 degree increments</param>
    public void DisplaySprite(string spriteName, uint x, uint y, uint width, uint height, int rotation)
    {
        // create a new GameObject
        GameObject newSquare = GetCachedSprite(spriteName);
        newSquare.transform.localPosition = new Vector3(x, y, 0);
        newSquare.transform.Rotate(0, 0, rotation*90);
        Sprite sprite = newSquare.GetComponent<SpriteRenderer>().sprite;

        uint spritePixelWidth = (uint)sprite.rect.width;
        uint spritePixelHeight = (uint)sprite.rect.height;

        // pixel width and height must be divisible by 8
        if (spritePixelWidth % 8 != 0 || spritePixelHeight % 8 != 0)
        {
            Debug.LogError("Sprite width and height must be divisible by 8");
            return;
        }

        uint spriteFullWidth = spritePixelWidth / 8;
        uint spriteFullHeight = spritePixelHeight / 8;

        float scaleX = (float)width / spriteFullWidth;
        float scaleY = (float)height / spriteFullHeight;

        // set the sprite
        newSquare.GetComponent<SpriteRenderer>().sprite = sprite;

        // set the scale
        newSquare.transform.localScale = new Vector3(scaleX, scaleY, 1);
    }

    public void Clear()
    {
        foreach (List<GameObject> squares in cachedSprites.Values)
        {
            foreach (GameObject square in squares)
            {
                square.SetActive(false);
            }
        }
    }

    private GameObject GetCachedSprite(string spriteName)
    {
        if (!cachedSprites.ContainsKey(spriteName))
        {
            GameObject newFirstSquare = Instantiate(baseSquarePrefab, transform);
            Sprite firstSprite = Resources.Load<Sprite>(spriteName);
            newFirstSquare.GetComponent<SpriteRenderer>().sprite = firstSprite;
            cachedSprites.Add(spriteName, new List<GameObject> { newFirstSquare });
            return newFirstSquare;
        }

        foreach (GameObject square in cachedSprites[spriteName])
        {
            if (!square.activeSelf)
            {
                square.SetActive(true);
                return square;
            }
        }

        GameObject newSquare = Instantiate(baseSquarePrefab, transform);
        Sprite sprite = Resources.Load<Sprite>(spriteName);
        newSquare.GetComponent<SpriteRenderer>().sprite = sprite;
        cachedSprites[spriteName].Add(newSquare);
        return newSquare;
    }
}
