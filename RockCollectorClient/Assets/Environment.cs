using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Environment : MonoBehaviour
{
    public Sprite leftCornerRock;
    public Sprite rightCornerRock;
    public List<Sprite> edgeRocks;
    public List<Sprite> topEdgeRocks;
    public List<Sprite> fillerRocks;
    public List<Sprite> inner_prop_1;
    public List<Sprite> inner_prop_2;

    public float rockSpacing = 0.240f;
    private int lastSeed = 0;

    public int GetLastSeed()
    {
        return lastSeed;
    }

    public IEnumerator Generate(int seed)
    {
        System.Random random = new(seed);

        yield return null;

        int width = random.Next(150, 400);
        int height = random.Next(150, 400);
        Debug.Log("Width: " + width + " Height: " + height);

        // start with a grid to make into the terrain shape
        bool[,] terrainGrid = new bool[width, height];
        // and a corresponding grid to keep track of danger levels,
        // which will be used to spawn timefish and other dangers
        float[,] dangerGrid = new float[width, height];

        // create horizontal and vertical imaginary lines to divide the terrain into sectors
        int numberOfHorizontalLines = random.Next(1, 3);
        int numberOfVerticalLines = random.Next(1, 3);
        List<int> horizontalLines = new();
        List<int> verticalLines = new();
        for (int i = 0; i < numberOfHorizontalLines; i++)
        {
            horizontalLines.Add(random.Next(0, height));
        }
        for (int i = 0; i < numberOfVerticalLines; i++)
        {
            verticalLines.Add(random.Next(0, width));
        }
        horizontalLines.Add(height);
        verticalLines.Add(width);
        horizontalLines.Add(0);
        verticalLines.Add(0);
        horizontalLines.Sort();
        verticalLines.Sort();
        List<Vector2Int> sectorStarts = new();
        List<Vector2Int> sectorSizes = new();
        for (int i = 0; i < horizontalLines.Count - 1; i++)
        {
            for (int j = 0; j < verticalLines.Count - 1; j++)
            {
                int sectorStartX = verticalLines[j];
                int sectorStartY = horizontalLines[i];
                int sectorWidth = verticalLines[j + 1] - verticalLines[j];
                int sectorHeight = horizontalLines[i + 1] - horizontalLines[i];
                sectorStarts.Add(new Vector2Int(sectorStartX, sectorStartY));
                sectorSizes.Add(new Vector2Int(sectorWidth, sectorHeight));
            }
        }
        

        // determine the terrain for each sector
        for (int i = 0; i < sectorStarts.Count; i++)
        {
            int sectorWidth = sectorSizes[i].x;
            int sectorHeight = sectorSizes[i].y;
            int sectorStartX = sectorStarts[i].x;
            int sectorStartY = sectorStarts[i].y;
            Debug.Log("Sector start: " + sectorStartX + ", " + sectorStartY);
            Debug.Log("Sector size: " + sectorWidth + ", " + sectorHeight);
            if (sectorStartY == 0)
            {
                IEnumerator sectorEnumator = MakeMountainSector(random, sectorWidth, sectorHeight, sectorStartX, sectorStartY, terrainGrid, 3, 100, 100, 100);
                while (sectorEnumator.MoveNext())
                {
                    yield return null;
                }
            }
            else if (sectorStartY == height - sectorHeight)
            {
                IEnumerator sectorEnumator = MakeInvertedMountainSector(random, sectorWidth, sectorHeight, sectorStartX, sectorStartY, terrainGrid);
                while (sectorEnumator.MoveNext())
                {
                    yield return null;
                }
            }
        }

        // make the left, right, and bottom edges solid
        for (int x = 0; x < width; x++)
        {
            terrainGrid[x, 0] = true;
        }
        for (int y = 0; y < height; y++)
        {
            terrainGrid[0, y] = true;
            terrainGrid[width - 1, y] = true;
        }

        // make sure there's space for the return ship
        // make sure the terrain grid is false for a rectangle at the top center
        for (int x = width / 2 - 5; x < width / 2 + 5; x++)
        {
            for (int y = height - 40; y < height; y++)
            {
                terrainGrid[x, y] = false;
            }
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

        // TODO spawn timefish


        lastSeed = seed;
    }

    private IEnumerator MakeInvertedMountainSector(System.Random random, int sectorWidth, int sectorHeight, int sectorStartX, int sectorStartY, bool[,] terrainGrid)
    {
        IEnumerator mountainEnumerator = MakeMountainSector(random, sectorWidth, sectorHeight, sectorStartX, sectorStartY, terrainGrid, 6, 50, 100, 50);
        while (mountainEnumerator.MoveNext())
        {
            yield return null;
        }
        FlipSector(sectorWidth, sectorHeight, sectorStartX, sectorStartY, terrainGrid);
    }

    private IEnumerator MakeMountainSector(
        System.Random random,
        int sectorWidth,
        int sectorHeight,
        int sectorStartX,
        int sectorStartY,
        bool[,] terrainGrid,
        int mountainCount,
        int maximumSpacing,
        int maximumWidth,
        int maximumHeight)
    {
        int mountainStartX = sectorStartX + random.Next(0, maximumSpacing); ;
        for (int i = 0; i < mountainCount; i++)
        { // make mountainCount many mountains
            // if we're already past the width, break
            if (mountainStartX >= sectorStartX + sectorWidth)
            {
                break;
            }

            int mountainStartY = sectorStartY;
            int mountainEndX = Mathf.Min(mountainStartX + random.Next(30, maximumWidth), sectorStartX + sectorWidth - 1);
            int mountainEndY = Mathf.Min(mountainStartY + random.Next(30, maximumHeight), sectorStartY + sectorHeight - 1);
            int mountainWidth = mountainEndX - mountainStartX;
            int mountainCenter = random.Next(mountainStartX + mountainWidth / 4, mountainEndX - mountainWidth / 4);
            IEnumerator mountainEnumerator = AddMountain(random, mountainStartX, mountainStartY, mountainEndX, mountainEndY, mountainCenter, terrainGrid);
            while (mountainEnumerator.MoveNext())
            {
                yield return null;
            }
            mountainStartX = mountainStartX + random.Next(30, maximumSpacing);
        }
    }

    private void FlipSector(int sectorWidth, int sectorHeight, int sectorStartX, int sectorStartY, bool[,] terrainGrid)
    {
        bool[,] sectorCopy = new bool[sectorWidth, sectorHeight];
        for (int x = 0; x < sectorWidth; x++)
        {
            for (int y = 0; y < sectorHeight; y++)
            {
                sectorCopy[x, y] = terrainGrid[sectorStartX + x, sectorStartY + y];
            }
        }
        for (int x = 0; x < sectorWidth; x++)
        {
            for (int y = 0; y < sectorHeight; y++)
            {
                terrainGrid[sectorStartX + x, sectorStartY + y] = sectorCopy[x, sectorHeight - y - 1];
            }
        }
    }

    private IEnumerator AddMountain(System.Random random, int startX, int startY, int endX, int endY, int center, bool[,] grid)
    {

        int currentX = startX;
        int currentY = startY;
        // starting at the bottom left corner, create a mountain contour
        int conservativeHeight = endY - 7;
        while (currentX < endX)
        {
            if (currentY < endY && currentY >= startY)
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
        for (int x = startX; x < endX; x++)
        {
            bool fill = false;
            for (int y = endY - 1; y >= startY; y--)
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
                Sprite topLeft = fillerRocks[random.Next(fillerRocks.Count)];
                Sprite topRight = fillerRocks[random.Next(fillerRocks.Count)];
                Sprite bottomRight = fillerRocks[random.Next(fillerRocks.Count)];
                Sprite bottomLeft = fillerRocks[random.Next(fillerRocks.Count)];
                // begin with random rotations
                int topLeftRotation = random.Next(0, 4) * 90;
                int topRightRotation = random.Next(0, 4) * 90;
                int bottomRightRotation = random.Next(0, 4) * 90;
                int bottomLeftRotation = random.Next(0, 4) * 90;
                if (topLeftCornerFree)
                {
                    topLeft = leftCornerRock;
                    topLeftRotation = 0;
                }
                if (topRightCornerFree)
                {
                    topRight = rightCornerRock;
                    topRightRotation = 270;
                }
                if (bottomRightCornerFree)
                {
                    bottomRight = edgeRocks[random.Next(edgeRocks.Count)];
                    bottomRightRotation = 270;
                }
                if (bottomLeftCornerFree)
                {
                    bottomLeft = edgeRocks[random.Next(edgeRocks.Count)];
                    bottomLeftRotation = 90;
                }
                if (upOccupied && !leftOccupied)
                {
                    topLeft = edgeRocks[random.Next(edgeRocks.Count)];
                    topLeftRotation = 90;
                }
                if (rightOccupied && !upOccupied)
                {
                    topRight = topEdgeRocks[random.Next(topEdgeRocks.Count)];
                    topRightRotation = 0;
                }
                if (downOccupied && !rightOccupied)
                {
                    bottomRight = edgeRocks[random.Next(edgeRocks.Count)];
                    bottomRightRotation = 270;
                }
                //if (leftOccupied && !downOccupied)
                //{
                //    bottomLeft = edgeRocks[random.Next(edgeRocks.Count)];
                //    bottomLeftRotation = 270;
                //}
                if (downOccupied && !leftOccupied)
                {
                    bottomLeft = edgeRocks[random.Next(edgeRocks.Count)];
                    bottomLeftRotation = 90;
                }
                if (leftOccupied && !upOccupied)
                {
                    topLeft = topEdgeRocks[random.Next(topEdgeRocks.Count)];
                    topLeftRotation = 0;
                }
                if (upOccupied && !rightOccupied)
                {
                    topRight = edgeRocks[random.Next(edgeRocks.Count)];
                    topRightRotation = 270;
                }
                //if (rightOccupied && !downOccupied)
                //{
                //    bottomRight = edgeRock;
                //    bottomRightRotation = 180;
                //}
                //if (topLeftInnerCorner)
                //{
                //    topLeft = innerCornerRock;
                //    topLeftRotation = 0;
                //}
                //if (topRightInnerCorner)
                //{
                //    topRight = innerCornerRock;
                //    topRightRotation = 270;
                //}
                //if (bottomRightInnerCorner)
                //{
                //    bottomRight = innerCornerRock;
                //    bottomRightRotation = 180;
                //}
                //if (bottomLeftInnerCorner)
                //{
                //    bottomLeft = innerCornerRock;
                //    bottomLeftRotation = 90;
                //}
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
                    rock.transform.position = new Vector3((-width / 2 + x) * rockSpacing, (-height + y) * rockSpacing, 0);
                    rock.AddComponent<SpriteRenderer>().sprite = sprites[x, y];
                    // also add a collider if this is not a filler rock
                    if (!fillerRocks.Contains(sprites[x, y]))
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
