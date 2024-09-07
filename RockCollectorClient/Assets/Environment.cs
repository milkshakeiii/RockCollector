using System.Collections;
using UnityEngine;

public class Environment : MonoBehaviour
{
    public Sprite cornerRock;
    public Sprite edgeRock;
    public Sprite fillerRock;
    public Sprite innerCornerRock;

    private const float rockSpacing = 0.32f;

    public IEnumerator Generate(int seed)
    {
        System.Random random = new(seed);

        yield return null;

        int width = random.Next(30, 100);
        int height = random.Next(30, 100);
        int center = random.Next(width/4, 3*width/4);
        Debug.Log("Width: " + width + " Height: " + height + " Center: " + center);
        // make an array of nulls
        Sprite[,] rocks = new Sprite[width, height];
        int[,] rotations = new int[width, height];

        // starting at the bottom left corner, determine sprites
        int currentX = 0;
        int currentY = 0;
        while (currentX < width)
        {
            if (currentY < height && currentY >= 0)
            {
                Debug.Log("xIndex: " + currentX + " yIndex: " + currentY);
                rocks[currentX, currentY] = cornerRock;
            }
            currentX++;
            float slope = (float)(height - currentY) / center;
            if (currentX > center)
            {
                slope = (float)(currentY) / (width - center);
            }
            int jitter = random.Next(-1, 2);
            int jitter2 = random.Next(-1, 2);
            int Ychange = Mathf.RoundToInt(slope + jitter + jitter2);
            if (currentX < center)
            {
               currentY += Ychange;
            }
            else
            {
                currentY -= Ychange;
            }
            yield return null;
        }

        // fill in the rest of the rocks one column at a time
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (rocks[x, y] == null)
                {
                    rocks[x, y] = fillerRock;
                }
                else
                {
                    break;
                }
            }
            yield return null;
        }

        // set filler, edge, inner corner, and inner edge rocks
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (rocks[x, y] == null)
                {
                    continue;
                }
                bool upLeftOccupied = x > 0 && y < height - 1 && rocks[x - 1, y + 1] != null;
                bool upOccupied = y < height - 1 && rocks[x, y + 1] != null;
                bool upRightOccupied = x < width - 1 && y < height - 1 && rocks[x + 1, y + 1] != null;
                bool rightOccupied = x < width - 1 && rocks[x + 1, y] != null;
                bool downRightOccupied = x < width - 1 && y > 0 && rocks[x + 1, y - 1] != null;
                bool downOccupied = y > 0 && rocks[x, y - 1] != null;
                bool downLeftOccupied = x > 0 && y > 0 && rocks[x - 1, y - 1] != null;
                bool leftOccupied = x > 0 && rocks[x - 1, y] != null;
                
                bool topLeftCornerFree = !upLeftOccupied && !upOccupied && !leftOccupied;
                bool topRightCornerFree = !upRightOccupied && !upOccupied && !rightOccupied;
                bool bottomRightCornerFree = !downRightOccupied && !downOccupied && !rightOccupied;
                bool bottomLeftCornerFree = !downLeftOccupied && !downOccupied && !leftOccupied;
                
                bool topLeftCornerOccupied = upLeftOccupied && upOccupied && leftOccupied;
                bool topRightCornerOccupied = upRightOccupied && upOccupied && rightOccupied;
                bool bottomRightCornerOccupied = downRightOccupied && downOccupied && rightOccupied;
                bool bottomLeftCornerOccupied = downLeftOccupied && downOccupied && leftOccupied;

                bool topLeftInnerCorner = !upLeftOccupied && upOccupied && leftOccupied;
                bool topRightInnerCorner = !upRightOccupied && upOccupied && rightOccupied;
                bool bottomRightInnerCorner = !downRightOccupied && downOccupied && rightOccupied;
                bool bottomLeftInnerCorner = !downLeftOccupied && downOccupied && leftOccupied;

                if (topLeftCornerFree && bottomRightCornerOccupied)
                {
                    rocks[x, y] = cornerRock;
                }
                else if (topRightCornerFree && bottomLeftCornerOccupied)
                {
                    rocks[x, y] = cornerRock;
                    rotations[x, y] = 270;
                }
                else if (bottomRightCornerFree && topLeftCornerOccupied)
                {
                    rocks[x, y] = cornerRock;
                    rotations[x, y] = 180;
                }
                else if (bottomLeftCornerFree && topRightCornerOccupied)
                {
                    rocks[x, y] = cornerRock;
                    rotations[x, y] = 90;
                }
                else if (topLeftInnerCorner)
                {
                    rocks[x, y] = innerCornerRock;
                    rotations[x, y] = 180;
                }
                else if (topRightInnerCorner)
                {
                    rocks[x, y] = innerCornerRock;
                    rotations[x, y] = 90;
                }
                else if (bottomRightInnerCorner)
                {
                    rocks[x, y] = innerCornerRock;
                }
                else if (bottomLeftInnerCorner)
                {
                    rocks[x, y] = innerCornerRock;
                    rotations[x, y] = 270;
                }
                else if (!leftOccupied && rightOccupied)
                {
                    rocks[x, y] = edgeRock;
                    rotations[x, y] = 90;
                }
                else if (!downOccupied && upOccupied)
                {
                    rocks[x, y] = edgeRock;
                    rotations[x, y] = 180;
                }
                else if (!rightOccupied && leftOccupied)
                {
                    rocks[x, y] = edgeRock;
                    rotations[x, y] = -90;
                }
                else if (!upOccupied && downOccupied)
                {
                    rocks[x, y] = edgeRock;
                }
                // if none of these is the case, set the sprite to filler and rotate it randomly
                else
                {
                    rocks[x, y] = fillerRock;
                    rotations[x, y] = random.Next(0, 4) * 90;
                }
            }
            yield return null;
        }

        // spawn GameObjects with sprites
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (rocks[x, y] != null)
                {
                    GameObject rock = new("Rock");
                    rock.transform.position = new Vector3((-width/2 + x) * rockSpacing, (-height + y) * rockSpacing, 0);
                    rock.AddComponent<SpriteRenderer>().sprite = rocks[x, y];
                    // also add a collider
                    rock.AddComponent<BoxCollider2D>();
                    // layer is "Terrain"
                    rock.layer = 9;
                    // set the rotation
                    rock.transform.Rotate(Vector3.forward, rotations[x, y]);
                }
            }
        }
    }
}
