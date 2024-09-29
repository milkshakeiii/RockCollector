using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class CoralDecoration : MonoBehaviour
{
    public List<Sprite> possibleSprites;

    public void Initialize(System.Random random, Vector2Int baseDirection)
    {
        // for now, make the scale 0.2f
        this.transform.localScale = new Vector3(0.2f, 0.2f, 1f);

        // layer is "terrain"
        gameObject.layer = 9;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = possibleSprites[random.Next(possibleSprites.Count)];

        // rotate the sprite to face the base direction
        float angle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
        this.transform.rotation = Quaternion.Euler(0, 0, angle+90);

        // place the decoration on top of the parent (coral)
        float height = this.GetComponent<SpriteRenderer>().bounds.size.y;
        float parentHeight = this.transform.parent.GetComponent<SpriteRenderer>().bounds.size.y;
        Vector3 baseDirectionFloat = new(baseDirection.x, baseDirection.y, 0);
        this.transform.position = this.transform.position - (height + parentHeight) * 0.9f * baseDirectionFloat.normalized / 2;
    }

    private float Range(System.Random random, float min, float max)
    {
        return (float)random.NextDouble() * (max - min) + min;
    }
}
