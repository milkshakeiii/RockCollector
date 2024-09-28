using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class RockDecoration : MonoBehaviour
{
    public List<Sprite> possibleSprites;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void Initialize(System.Random random)
    {
        this.GetComponent<SpriteRenderer>().sprite = possibleSprites[random.Next(possibleSprites.Count)];

        // randomize the size
        this.transform.localScale = new Vector3(Range(random, 0.4f, 0.8f), Range(random, 0.4f, 0.8f), 1);

        // layer is "terrain"                      
        gameObject.layer = 9;

        //some decorations are placed on top of the rocks, some behind
        this.GetComponent<SpriteRenderer>().sortingOrder = random.Next(-100, 100);

        // place the decoration on top of the parent (rock)
        float height = this.GetComponent<SpriteRenderer>().bounds.size.y;
        float parentHeight = this.transform.parent.GetComponent<SpriteRenderer>().bounds.size.y;
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + height / 2 + parentHeight / 2, this.transform.position.z);
        // slightly randomize the y position
        this.transform.position += new Vector3(0, Range(random, -0.1f, 0f), 0);
    }

    private float Range(System.Random random, float min, float max)
    {
        return (float)random.NextDouble() * (max - min) + min;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
