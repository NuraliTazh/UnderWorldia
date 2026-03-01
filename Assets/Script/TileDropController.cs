using UnityEngine;

public class TileDropControler : MonoBehaviour 
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            //adds to inventory
            Destroy(this.gameObject);
        }
    }
}

