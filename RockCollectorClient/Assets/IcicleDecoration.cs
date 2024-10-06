using System.Collections.Generic;
using UnityEngine;

public class IcicleDecoration : MonoBehaviour
{
    public List<Sprite> possibleSprites;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void Initialize(System.Random random)
    {
        this.GetComponent<SpriteRenderer>().sprite = possibleSprites[random.Next(possibleSprites.Count)];

        // place the decoration below the parent
        float height = this.GetComponent<SpriteRenderer>().bounds.size.y;
        float parentHeight = this.transform.parent.GetComponent<SpriteRenderer>().bounds.size.y;
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y - height / 2 - parentHeight / 2, this.transform.position.z);
        // set absolute rotation to 180
        this.transform.rotation = Quaternion.Euler(0, 0, 180);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
