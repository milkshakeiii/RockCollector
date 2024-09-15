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

        int width = random.Next(30, 100);
        int height = random.Next(30, 100);
        int center = random.Next(width/4, 3*width/4);
        Debug.Log("Width: " + width + " Height: " + height + " Center: " + center);
        
        // start with a grid to make into a mountain shape
        bool[,] grid = new bool[width, height];

        // each grid cell has 4 sprites with 4 rotations
        Sprite[,] rocks = new Sprite[width*2, height*2];
        int[,] rotations = new int[width*2, height*2];

        // starting at the bottom left corner, determine sprites
        int currentX = 0;
        int currentY = 0;
        int conservativeHeight = height - 7;
        while (currentX < width)
        {
            if (currentY < height && currentY >= 0)
            {
                Debug.Log("xIndex: " + currentX + " yIndex: " + currentY);
                grid[currentX, currentY] = true;
            }
            currentX++;
            float slope = (float)(conservativeHeight - currentY) / (center - currentX);
            if (currentX >= center)
            {
                slope = (float)(-currentY) / (width - currentX);
            }
            int jitter = random.Next(-1, 2);
            int jitter2 = random.Next(-1, 2);
            int Ychange = Mathf.RoundToInt(slope + jitter + jitter2);
            currentY += Ychange;
            yield return null;
        }

        // fill in the rest of the rocks one column at a time
        for (int x = 0; x < width; x++)
        {
            bool fill = false;
            for (int y = height-1; y >= 0; y--)
            {
                if (grid[x, y])
                {
                    fill = true;
                }
                grid[x, y] = fill;
            }
            yield return null;
        }

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
                rocks[x*2, y*2] = bottomLeft;
                rocks[x*2 + 1, y*2] = bottomRight;
                rocks[x*2, y*2 + 1] = topLeft;
                rocks[x*2 + 1, y*2 + 1] = topRight;
                rotations[x*2, y*2] = bottomLeftRotation;
                rotations[x*2 + 1, y*2] = bottomRightRotation;
                rotations[x*2, y*2 + 1] = topLeftRotation;
                rotations[x*2 + 1, y*2 + 1] = topRightRotation;
            }
            yield return null;
        }

        // spawn GameObjects with sprites
        for (int x = 0; x < width*2; x++)
        {
            for (int y = 0; y < height*2; y++)
            {
                if (rocks[x, y] != null)
                {
                    GameObject rock = new("Rock");
                    rock.transform.position = new Vector3((-width/2 + x) * rockSpacing + 5, (-height + y) * rockSpacing - 10, 0);
                    rock.AddComponent<SpriteRenderer>().sprite = rocks[x, y];
                    // also add a collider if this is not a filler rock
                    if (rocks[x, y] != fillerRock)
                        rock.AddComponent<BoxCollider2D>();
                    // layer is "Terrain"
                    rock.layer = 9;
                    // set the rotation
                    rock.transform.Rotate(Vector3.forward, rotations[x, y]);
                }
            }
        }

        // place some inner props randomly
        for (int i = 0; i < 200; i++)
        {
            // randomly choose a prop list
            List<Sprite> propList = random.Next(0, 2) == 0 ? inner_prop_1 : inner_prop_2;
            // randomly choose up, down, left, or right
            List<Vector2Int> directions = new() { new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(1, 0) };
            Vector2Int direction = directions[random.Next(0, 4)];

            // randomly choose a position
            int x = random.Next(0, width*2);
            int y = random.Next(0, height*2);

            // add direction to get the positions of all the sprites
            List<Vector2Int> positions = new() { new Vector2Int(x, y) };
            for (int j = 0; j < propList.Count - 1; j++)
            {
                positions.Add(positions[j] + direction);
            }

            // only continue if all positions are within bounds and are currently filler rock
            bool valid = true;
            foreach (Vector2Int position in positions)
            {
                if (position.x < 0 || position.x >= width*2 || position.y < 0 || position.y >= height*2 || rocks[position.x, position.y] != fillerRock)
                {
                    valid = false;
                    break;
                }
            }
            if (!valid)
            {
                continue;
            }

            // spawn the game objects
            for (int j = 0; j < propList.Count; j++)
            {
                GameObject prop = new("Prop");
                prop.transform.position = new Vector3((-width/2 + positions[j].x) * rockSpacing + 5, (-height + positions[j].y) * rockSpacing - 10, 0);
                prop.AddComponent<SpriteRenderer>().sprite = propList[j];
                prop.GetComponent<SpriteRenderer>().sortingOrder = 1;
                prop.layer = 9;
                // set the rotation according to the direction
                if (direction == new Vector2Int(0, -1))
                {
                    prop.transform.Rotate(Vector3.forward, 270f);
                }
                else if (direction == new Vector2Int(1, 0))
                {
                    prop.transform.Rotate(Vector3.forward, 0f);
                }
                else if (direction == new Vector2Int(0, 1))
                {
                    prop.transform.Rotate(Vector3.forward, 90f);
                }
                else if (direction == new Vector2Int(-1, 0))
                {
                    prop.transform.Rotate(Vector3.forward, 180f);
                }
            }
        }

        lastSeed = seed;
    }
}
