using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{
    public Sprite cornerRock;
    public Sprite edgeRock;
    public Sprite fillerRock;
    public Sprite innerCornerRock;
    public List<Sprite> inner_prop_1;
    public List<Sprite> inner_prop_2;

    private const float rockSpacing = 0.159f;
    private int lastSeed = 0;

    public int GetLastSeed()
    {
        return lastSeed;
    }

    public IEnumerator Generate(int seed)
    {
        System.Random random = new(seed);

        yield return null;

        int width = random.Next(90, 300);
        int height = random.Next(90, 300);

        // start with a grid to make into the terrain shape
        bool[,] terrainGrid = new bool[width, height];
        
        int mountainStartX = random.Next(30, 100);
        int mountainStartY = 0;
        int mountainEndX = Mathf.Min(mountainStartX + random.Next(30, 100), width-1);
        int mountainEndY = Mathf.Min(mountainStartY + random.Next(30, 100), height-1);
        int mountainWidth = mountainEndX - mountainStartX;
        int mountainCenter = random.Next(mountainStartX + mountainWidth / 4, mountainEndX - mountainWidth / 4);
        Debug.Log("Mountain start: " + mountainStartX + ", " + mountainStartY);
        Debug.Log("Mountain end: " + mountainEndX + ", " + mountainEndY);
        Debug.Log("Mountain center: " + mountainCenter);
        IEnumerator mountainEnumerator = AddMountain(random, mountainStartX, mountainStartY, mountainEndX, mountainEndY, mountainCenter, terrainGrid);
        while (mountainEnumerator.MoveNext())
        {
            yield return null;
        }

        // each grid cell covers 4 sprites with 4 rotations
        Sprite[,] sprites = new Sprite[width * 2, height * 2];
        int[,] rotations = new int[width * 2, height * 2];
        IEnumerator spritesEnumerator = DecideSpritesAndRotations(random, terrainGrid, sprites, rotations);
        while (spritesEnumerator.MoveNext())
        {
            yield return null;
        }

        // spawn the GameObjects
        IEnumerator spawnEnumerator = SpawnGameObjects(sprites, rotations);
        while (spawnEnumerator.MoveNext())
        {
            yield return null;
        }

        lastSeed = seed;
    }

    private IEnumerator AddMountain(System.Random random, int startX, int startY, int endX, int endY, int center, bool[,] grid)
    {

        int currentX = startX;
        int currentY = startY;
        // starting at the bottom left corner, determine sprites
        int conservativeHeight = endY - 7;
        while (currentX < endX)
        {
            if (currentY < endY && currentY >= 0)
            {
                //Debug.Log("xIndex: " + currentX + " yIndex: " + currentY);
                grid[currentX, currentY] = true;
            }
            currentX++;
            float slope = (float)(conservativeHeight - currentY) / (center - currentX);
            if (currentX >= center)
            {
                slope = (float)(-currentY) / (endX - currentX);
            }
            int jitter = random.Next(-1, 2);
            int jitter2 = random.Next(-1, 2);
            int Ychange = Mathf.RoundToInt(slope + jitter + jitter2);
            currentY += Ychange;
            yield return null;
        }

        // fill in the rest of the rocks one column at a time
        for (int x = 0; x < endX; x++)
        {
            bool fill = false;
            for (int y = endY - 1; y >= 0; y--)
            {
                if (grid[x, y])
                {
                    fill = true;
                }
                grid[x, y] = fill;
            }
            yield return null;
        }
    }

    private IEnumerator DecideSpritesAndRotations(System.Random random, bool[,] grid, Sprite[,] sprites, int[,] rotations)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);
        // set filler, edge, inner corner, and inner edge rocks
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!grid[x, y])
                {
                    continue;
                }
                bool upLeftOccupied = x > 0 && y < height - 1 && grid[x - 1, y + 1];
                bool upOccupied = y < height - 1 && grid[x, y + 1];
                bool upRightOccupied = x < width - 1 && y < height - 1 && grid[x + 1, y + 1];
                bool rightOccupied = x < width - 1 && grid[x + 1, y];
                bool downRightOccupied = x < width - 1 && y > 0 && grid[x + 1, y - 1];
                bool downOccupied = y > 0 && grid[x, y - 1];
                bool downLeftOccupied = x > 0 && y > 0 && grid[x - 1, y - 1];
                bool leftOccupied = x > 0 && grid[x - 1, y];

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

                // based on the grid neighbors, set 4 sprites per grid cell
                Sprite topLeft = fillerRock;
                Sprite topRight = fillerRock;
                Sprite bottomRight = fillerRock;
                Sprite bottomLeft = fillerRock;
                // begin with random rotations
                int topLeftRotation = random.Next(0, 4) * 90;
                int topRightRotation = random.Next(0, 4) * 90;
                int bottomRightRotation = random.Next(0, 4) * 90;
                int bottomLeftRotation = random.Next(0, 4) * 90;
                if (topLeftCornerFree)
                {
                    topLeft = cornerRock;
                    topLeftRotation = 0;
                }
                if (topRightCornerFree)
                {
                    topRight = cornerRock;

                    topRightRotation = 270;
                }
                if (bottomRightCornerFree)
                {
                    bottomRight = cornerRock;
                    bottomRightRotation = 180;
                }
                if (bottomLeftCornerFree)
                {
                    bottomLeft = cornerRock;
                    bottomLeftRotation = 90;
                }
                if (upOccupied && !leftOccupied)
                {
                    topLeft = edgeRock;
                    topLeftRotation = 90;
                }
                if (rightOccupied && !upOccupied)
                {
                    topRight = edgeRock;
                    topRightRotation = 0;
                }
                if (downOccupied && !rightOccupied)
                {
                    bottomRight = edgeRock;
                    bottomRightRotation = 270;
                }
                if (leftOccupied && !downOccupied)
                {
                    bottomLeft = edgeRock;
                    bottomLeftRotation = 180;
                }
                if (downOccupied && !leftOccupied)
                {
                    bottomLeft = edgeRock;
                    bottomLeftRotation = 90;
                }
                if (leftOccupied && !upOccupied)
                {
                    topLeft = edgeRock;
                    topLeftRotation = 0;
                }
                if (upOccupied && !rightOccupied)
                {
                    topRight = edgeRock;
                    topRightRotation = 270;
                }
                if (rightOccupied && !downOccupied)
                {
                    bottomRight = edgeRock;
                    bottomRightRotation = 180;
                }
                if (topLeftInnerCorner)
                {
                    topLeft = innerCornerRock;
                    topLeftRotation = 0;
                }
                if (topRightInnerCorner)
                {
                    topRight = innerCornerRock;
                    topRightRotation = 270;
                }
                if (bottomRightInnerCorner)
                {
                    bottomRight = innerCornerRock;
                    bottomRightRotation = 180;
                }
                if (bottomLeftInnerCorner)
                {
                    bottomLeft = innerCornerRock;
                    bottomLeftRotation = 90;
                }
                sprites[x * 2, y * 2] = bottomLeft;
                sprites[x * 2 + 1, y * 2] = bottomRight;
                sprites[x * 2, y * 2 + 1] = topLeft;
                sprites[x * 2 + 1, y * 2 + 1] = topRight;
                rotations[x * 2, y * 2] = bottomLeftRotation;
                rotations[x * 2 + 1, y * 2] = bottomRightRotation;
                rotations[x * 2, y * 2 + 1] = topLeftRotation;
                rotations[x * 2 + 1, y * 2 + 1] = topRightRotation;
            }
            yield return null;
        }
    }

    private IEnumerator SpawnGameObjects(Sprite[,] sprites, int[,] rotations)
    {
        int width = sprites.GetLength(0);
        int height = sprites.GetLength(1);
        // spawn GameObjects with sprites
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (sprites[x, y] != null)
                {
                    GameObject rock = new("Rock");
                    rock.transform.position = new Vector3((-width / 2 + x) * rockSpacing + 5, (-height + y) * rockSpacing - 10, 0);
                    rock.AddComponent<SpriteRenderer>().sprite = sprites[x, y];
                    // also add a collider if this is not a filler rock
                    if (sprites[x, y] != fillerRock)
                        rock.AddComponent<BoxCollider2D>();
                    // layer is "Terrain"
                    rock.layer = 9;
                    // set the rotation
                    rock.transform.Rotate(Vector3.forward, rotations[x, y]);
                }
            }
            yield return null;
        }
    }
}
