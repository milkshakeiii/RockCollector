using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayGrid : MonoBehaviour
{
    public const int WIDTH = 240;
    public const int HEIGHT = 135;

    public delegate void OnMouseUp(Vector2Int position, int mouseButton);
    public static event OnMouseUp MouseUp;

    public GameObject gridCamera;

    public GameObject baseSquarePrefab;
    public GameObject letterPrefab;
    public GameObject letterCanvas;

    private Dictionary<string, List<GameObject>> cachedSprites = new();
    private List<GameObject> letters = new();

    void OnEnable()
    {

    }

    void Update()
    {
        // check for mouse up
        int mouseButton = -1;
        if (Input.GetMouseButtonUp(0))
        {
            mouseButton = 0;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            mouseButton = 1;
        }
        if (mouseButton != -1)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = HEIGHT/2f;
            Vector3 worldPos = gridCamera.GetComponent<Camera>().ScreenToWorldPoint(mousePos);
            int x = Mathf.FloorToInt(worldPos.x / 4f);
            int y = Mathf.FloorToInt(worldPos.y / 4f);
            MouseUp?.Invoke(new Vector2Int(x, y), mouseButton);
        }
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
    public void DisplaySprite(string spriteName, int x, int y, int width, int height, int rotation, int overlapLayer = 0, bool parentToCamera = false)
    {
        // create a new GameObject
        GameObject newSquare = GetCachedSprite(spriteName);
        if (parentToCamera)
        {
            newSquare.transform.SetParent(gridCamera.transform);
            newSquare.transform.localPosition = new Vector3(x, y, -gridCamera.transform.localPosition.z);
        }
        else
        {
            newSquare.transform.SetParent(transform);
            newSquare.transform.localPosition = new Vector3(x, y, 0);
        }

        newSquare.transform.localRotation = Quaternion.Euler(0, 0, rotation * 90);
        // since the sprites' pivots are in the bottom left corner, we need to adjust the position
        if (rotation == 1)
        {
            newSquare.transform.localPosition += new Vector3(width, 0, 0);
        }
        else if (rotation == 2)
        {
            newSquare.transform.localPosition += new Vector3(width, height, 0);
        }
        else if (rotation == 3)
        {
            newSquare.transform.localPosition += new Vector3(0, height, 0);
        }

        Sprite sprite = newSquare.GetComponent<SpriteRenderer>().sprite;

        uint spritePixelWidth = (uint)sprite.rect.width;
        uint spritePixelHeight = (uint)sprite.rect.height;

        // pixel width and height must be divisible by 8
        if (spritePixelWidth % 8 != 0 || spritePixelHeight % 8 != 0)
        {
            Debug.LogError("Sprite width and height must be divisible by 8: " + spriteName);
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

        // set the sorting layer
        newSquare.GetComponent<SpriteRenderer>().sortingOrder = overlapLayer;
    }

    public void DisplayText(string text, int x, int y)
    {
        for (int i = 0; i < text.Length; i++)
        {
            DisplayLetter(text[i], (x + i), y);
        }
    }

    private void DisplayLetter(char letter, int x, int y)
    {
        GameObject newLetter = GetCachedLetter();

        newLetter.GetComponent<TMP_Text>().text = letter.ToString();
        float xMin = (float)x / WIDTH;
        float yMin = (float)y / HEIGHT;
        float xMax = (float)(x + 1) / WIDTH;
        float yMax = (float)(y + 1) / HEIGHT;
        newLetter.GetComponent<RectTransform>().anchorMin = new Vector2(xMin, yMin);
        newLetter.GetComponent<RectTransform>().anchorMax = new Vector2(xMax, yMax);
        newLetter.GetComponent<RectTransform>().rect.Set(0, 0, 0, 0);
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

        foreach (GameObject letter in letters)
        {
            letter.SetActive(false);
        }
    }

    private GameObject GetCachedSprite(string spriteName)
    {
        if (!cachedSprites.ContainsKey(spriteName))
        {
            GameObject newFirstSquare = Instantiate(baseSquarePrefab, transform);
            Sprite firstSprite = Resources.Load<Sprite>(spriteName);
            if (firstSprite == null)
            {
                Debug.LogError("Sprite not found: " + spriteName);
                return null;
            }
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
        if (sprite == null)
        {
            Debug.LogError("Sprite not found: " + spriteName);
            return null;
        }
        newSquare.GetComponent<SpriteRenderer>().sprite = sprite;
        cachedSprites[spriteName].Add(newSquare);
        return newSquare;
    }

    private GameObject GetCachedLetter()
    {
        foreach (GameObject letter in letters)
        {
            if (!letter.activeSelf)
            {
                letter.SetActive(true);
                return letter;
            }
        }

        GameObject newLetter = Instantiate(letterPrefab, letterCanvas.transform);
        letters.Add(newLetter);
        return newLetter;
    }
}
