using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public CharacterController controller;

    public float speed = 6f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Interaction")]
    [SerializeField] float range;

    Vector3 velocity;
    bool isGrounded;

    Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        WorldInteract();
    }

    void WorldInteract()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            bool chunkHit = Physics.Raycast(ray, out RaycastHit hit, range, groundMask.value);

            Debug.Log(chunkHit);
            if (chunkHit)
                Debug.Log($"Raycast Hit: {hit.collider.gameObject}");

            if (!chunkHit)
                return;

            Vector3 hitPos = hit.point + cam.transform.forward * 0.05f;
            Debug.Log($"Hit Point: {hit.point}, Hit Pos: {hitPos}");

            if (hit.collider.gameObject.TryGetComponent(out Chunk chunk))
            {
                chunk.DestroyBlockAtPos(hitPos);
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            bool chunkHit = Physics.Raycast(ray, out RaycastHit hit, range, groundMask.value);

            Debug.Log(chunkHit);
            if (chunkHit)
                Debug.Log($"Raycast Hit: {hit.collider.gameObject}");

            if (!chunkHit)
                return;

            Vector3 hitPos = hit.point - cam.transform.forward * 0.05f;
            Debug.Log($"Hit Point: {hit.point}, Hit Pos: {hitPos}");

            if (hit.collider.gameObject.TryGetComponent(out Chunk chunk))
            {
                World.Instance.PlaceBlockInChunk(hitPos);
            }
        }    
    }
}
