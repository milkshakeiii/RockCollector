using UnityEngine;

public class DisplayGrid : MonoBehaviour
{
    public const int WIDTH = 240;
    public const int HEIGHT = 135;

    public GameObject baseSquarePrefab;


    void OnEnable()
    {
        DisplaySprite("Art/UI/plain_white", 0, 0, WIDTH/3, HEIGHT);
    }

    void Update()
    {
        
    }

    public void DisplaySprite(string spriteName, uint x, uint y, uint width, uint height)
    {
        // create a new GameObject
        GameObject newSquare = Instantiate(baseSquarePrefab, this.transform.position, Quaternion.identity, this.transform);
        newSquare.transform.localPosition = new Vector3(x, y, 0);
        Sprite sprite = Resources.Load<Sprite>(spriteName);

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

        uint scaleX = width / spriteFullWidth;
        uint scaleY = height / spriteFullHeight;

        // set the sprite
        newSquare.GetComponent<SpriteRenderer>().sprite = sprite;

        // set the scale
        newSquare.transform.localScale = new Vector3(scaleX, scaleY, 1);
    }
}
