using UnityEngine;

public class CamControler : MonoBehaviour
{
    [Range(0, 1)]
    public float smoothTime;
    public Transform playerTransform;

    void Update()
    {
        Vector3 pos = GetComponent<Transform>().position;
        
        pos.x = Mathf.Lerp(pos.x, playerTransform.position.x, smoothTime);
        pos.y = Mathf.Lerp(pos.y, playerTransform.position.y, smoothTime);

        GetComponent<Transform>().position = pos;
    }
}
