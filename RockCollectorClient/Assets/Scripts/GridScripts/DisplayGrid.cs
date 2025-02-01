using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class DisplayGrid : MonoBehaviour
{
    public const int WIDTH = 240;
    public const int HEIGHT = 135;

    public delegate void OnMouse(Vector3 worldPosition, Vector2Int screenPosition, int mouseButton);
    public static event OnMouse MouseUp;
    public static event OnMouse MouseDown;

    public GameObject gridCamera;

    public GameObject baseSquarePrefab;
    public GameObject letterPrefab;
    public GameObject letterCanvas;
    public GameObject letterCanvasOnCamera;

    private Dictionary<string, List<GameObject>> cachedSprites = new();
    private List<GameObject> worldLetters = new();
    private List<GameObject> cameraLetters = new();

    void OnEnable()
    {

    }

    void Update()
    {
        // check for mouse up
        int mouseUpButton = -1;
        if (Input.GetMouseButtonUp(0))
        {
            mouseUpButton = 0;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            mouseUpButton = 1;
        }
        if (mouseUpButton != -1)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = HEIGHT/2f;
            Vector3 worldPos = gridCamera.GetComponent<Camera>().ScreenToWorldPoint(mousePos);
            Vector2Int screenPosition = new(Mathf.RoundToInt(mousePos.x/8f) - WIDTH/2, Mathf.RoundToInt(mousePos.y/8f) - HEIGHT/2 - 1);
            MouseUp?.Invoke(worldPos, screenPosition, mouseUpButton);
        }

        // check for mouse down
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = HEIGHT/2f;
            Vector3 worldPos = gridCamera.GetComponent<Camera>().ScreenToWorldPoint(mousePos);
            Vector2Int screenPosition = new(Mathf.RoundToInt(mousePos.x/8f) - WIDTH/2, Mathf.RoundToInt(mousePos.y/8f) - HEIGHT/2 - 1);
            MouseDown?.Invoke(worldPos, screenPosition, 0);
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
    public void DisplaySprite(string spriteName, float x, float y, int width, int height, int rotation, int overlapLayer = 0, bool parentToCamera = false)
    {
        // create a new GameObject
        GameObject newSquare = GetCachedSprite(spriteName);
        if (parentToCamera)
        {
            newSquare.transform.SetParent(gridCamera.transform);
            newSquare.transform.localPosition = new Vector3(x, y, DisplayGrid.HEIGHT/2f);
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

        // set the scale
        newSquare.transform.localScale = new Vector3(scaleX, scaleY, 1);

        // set the sorting layer
        newSquare.GetComponent<SpriteRenderer>().sortingOrder = overlapLayer;
    }

    public void DisplayText(string text, int x, int y, bool parentToCamera = false)
    {
        DisplayText(text, x, y, Color.white, parentToCamera);
    }

    public void DisplayText(string text, int x, int y, Color color, bool parentToCamera = false)
    {
        bool bold = false;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '<')
            {
                bold = true;
                continue;
            }
            if (text[i] == '>')
            {
                bold = false;
                continue;
            }
            DisplayLetter(text[i], (x + i), y, color, bold, parentToCamera);
        }
    }

    private void DisplayLetter(char letter, int x, int y, Color color, bool bold, bool parentToCamera)
    {
        if (color == null)
        {
            color = Color.white;
        }

        GameObject newLetter = GetCachedLetter(parentToCamera);

        newLetter.GetComponent<TMP_Text>().text = letter.ToString();
        float xMin = (float)x / WIDTH;
        float yMin = (float)y / HEIGHT;
        float xMax = (float)(x + 1) / WIDTH;
        float yMax = (float)(y + 1) / HEIGHT;
        newLetter.GetComponent<RectTransform>().anchorMin = new Vector2(xMin, yMin);
        newLetter.GetComponent<RectTransform>().anchorMax = new Vector2(xMax, yMax);
        newLetter.GetComponent<RectTransform>().rect.Set(0, 0, 0, 0);
        newLetter.GetComponent<TMP_Text>().color = color;
        newLetter.GetComponent<TMP_Text>().fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;

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

        foreach (GameObject letter in worldLetters)
        {
            letter.SetActive(false);
        }

        foreach (GameObject letter in cameraLetters)
        {
            letter.SetActive(false);
        }
    }

    private GameObject GetCachedSprite(string spriteName)
    {
        if (!cachedSprites.ContainsKey(spriteName))
        {
            GameObject newFirstSquare = NewGameObject(spriteName);
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

        GameObject newSquare = NewGameObject(spriteName);
        cachedSprites[spriteName].Add(newSquare);
        return newSquare;
    }

    private GameObject NewGameObject(string spriteName)
    {
        GameObject newSquare = Instantiate(baseSquarePrefab, transform);
        Sprite firstSprite = Resources.Load<Sprite>(spriteName);
        if (firstSprite == null)
        {
            Debug.LogError("Sprite not found: " + spriteName);
            return null;
        }
        newSquare.GetComponent<SpriteRenderer>().sprite = firstSprite;
        return newSquare;
    }

    private GameObject GetCachedLetter(bool parentToCamera)
    {
        GameObject parent = parentToCamera ? letterCanvasOnCamera : letterCanvas;
        List<GameObject> letters = parentToCamera ? cameraLetters : worldLetters;
        foreach (GameObject letter in letters)
        {
            if (!letter.activeSelf)
            {
                letter.SetActive(true);
                return letter;
            }
        }

        GameObject newLetter = Instantiate(letterPrefab, parent.transform);
        letters.Add(newLetter);
        return newLetter;
    }

    public void AnimateTo(string spriteName, float fromX, float fromY, float toX, float toY, int width, int height, float duration)
    {
        StartCoroutine(AnimateToCoroutine(spriteName, fromX, fromY, toX, toY, width, height, duration));
    }

    public IEnumerator AnimateToCoroutine(string spriteName, float fromX, float fromY, float toX, float toY, int width, int height, float duration)
    {
        // create a new GameObject
        GameObject newSquare = NewGameObject(spriteName);
        newSquare.transform.SetParent(transform);
        newSquare.transform.localPosition = new Vector3(fromX, fromY, 0);

        // set the scale
        Sprite sprite = newSquare.GetComponent<SpriteRenderer>().sprite;
        uint spritePixelWidth = (uint)sprite.rect.width;
        uint spritePixelHeight = (uint)sprite.rect.height;
        uint spriteFullWidth = spritePixelWidth / 8;
        uint spriteFullHeight = spritePixelHeight / 8;
        float scaleX = (float)width / spriteFullWidth;
        float scaleY = (float)height / spriteFullHeight;
        newSquare.transform.localScale = new Vector3(scaleX, scaleY, 1);

        // set the sorting layer
        newSquare.GetComponent<SpriteRenderer>().sortingOrder = 10;

        // animate
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            newSquare.transform.localPosition = new Vector3(Mathf.Lerp(fromX, toX, t), Mathf.Lerp(fromY, toY, t), 0);
            yield return null;
        }

        // destroy the GameObject
        Destroy(newSquare);
    }
}
