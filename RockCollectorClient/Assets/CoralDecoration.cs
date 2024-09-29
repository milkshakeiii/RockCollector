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

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = possibleSprites[random.Next(possibleSprites.Count)];

        // rotate the sprite to face the base direction
        float angle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
        this.transform.rotation = Quaternion.Euler(0, 0, angle+90);
    }
}
