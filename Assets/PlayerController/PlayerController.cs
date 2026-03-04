using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public TileClass selectedTile;

    public int playerRange;
    public Vector2Int mousePos;

    public float moveSpeed;
    public float jumpForce;
    public bool onGrownd;

    private Rigidbody2D rb;
    private Animator anim;

    public float horizontal;
    public bool hit;
    public bool place;

    [HideInInspector]
    public Vector2 spawnPos;
    public TerrainGeneration terrainGenerator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    public void Spawn()
    {
        transform.position = spawnPos;
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.CompareTag("Grownd"))
            onGrownd = true;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Grownd"))
            onGrownd = false;
    }

    private void FixedUpdate()
    {
        horizontal = Input.GetAxis("Horizontal");
        float jump = Input.GetAxisRaw("Jump");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(horizontal * moveSpeed,  rb.linearVelocity.y);

        if (horizontal > 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (horizontal < 0)
            transform.localScale = new Vector3(1, 1, 1);
        
        if (vertical > 0.1f || jump > 0.1f)
        {
            if (onGrownd)
                movement.y = jumpForce;
        }

        rb.linearVelocity = movement;
    }

    private void Update()
    {
        hit = Input.GetMouseButtonDown(0);
        place = Input.GetMouseButton(1);

        if (Vector2.Distance(transform.position, mousePos) <= playerRange && Vector2.Distance(transform.position, mousePos) > 1f)
        {
            if (place)
                terrainGenerator.CheckTile(selectedTile, mousePos.x, mousePos.y, false);
        }

        if (Vector2.Distance(transform.position, mousePos) <= playerRange)
        {
            if (hit)
                terrainGenerator.RemoveTile(mousePos.x, mousePos.y);
        }

        mousePos.x = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).x - 0.5f); 
        mousePos.y = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).y - 0.5f); 

        anim.SetFloat("horizontal", horizontal);
        anim.SetBool("hit", hit || place);
    }
}
