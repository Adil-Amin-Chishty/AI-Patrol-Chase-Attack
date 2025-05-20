# Unity AI: Patrolling, Chasing & Attacking

This Unity project demonstrates a simple enemy AI system with patrolling, chasing, and attacking behaviors, alongside a basic player movement and camera control setup.

---

## Project Overview

The enemy AI uses Unity’s `NavMeshAgent` to patrol randomly within a defined area, detect the player within sight range, chase the player, and attack by shooting projectiles when in close range. The player can move freely using WASD controls, jump, and look around using the mouse.

---

## Features

- **Enemy AI States:** Patrol → Chase → Attack
- **Player Movement:** WASD movement, jumping, gravity
- **Mouse Look:** First-person camera control with clamped vertical rotation
- **Projectile Attack:** Enemy fires projectiles with cooldown

---

## Scripts Overview

### 1. AI_Scripts.cs

This script manages enemy behavior using a finite state approach:

- **Patrolling:** Randomly moves to walkpoints within a set range on the ground.
- **Chasing:** Pursues the player when detected within sight range.
- **Attacking:** Stops moving and fires a projectile when the player is within attack range.
- **Detection:** Uses sphere checks to detect player presence.
- **Attack Cooldown:** Prevents continuous firing by adding delay between attacks.
- **Visual Debug:** Gizmos show sight and attack ranges in the editor.

### 2. PlayerMovement.cs

This script controls the player’s movement and camera:

- **Movement:** Uses `CharacterController` for smooth movement with gravity.
- **Jumping:** Allows jumping only when grounded.
- **Ground Check:** Checks for ground using a sphere collider to prevent jumping in mid-air.
- **Mouse Look:** Rotates the player body horizontally and the camera vertically with clamping.
- **Cursor Lock:** Locks and hides the cursor for better control.

---

## How to Use

1. **Set Up Scene:**

   - Ensure your player GameObject is named `"Player"`.
   - Attach `NavMeshAgent` to your enemy GameObject.
   - Bake a NavMesh on your ground so enemies can navigate.
   - Assign appropriate layers for ground and player to `whatISGround` and `whatisPlayer`.

2. **Assign Scripts:**

   - Attach `AI_Scripts.cs` to your enemy GameObject.
   - Attach `PlayerMovement.cs` to your player GameObject.
   - Assign the necessary references in the Inspector (e.g., `groundCheck`, `playerBody`, `projectile` prefab).

3. **Projectile Setup:**

   - Create a projectile prefab with a Rigidbody component.
   - Assign the prefab to the `projectile` field in the enemy’s `AI_Scripts` component.

4. **Play:**

   - Press play and control the player using WASD and mouse.
   - Observe the enemy patrolling, chasing, and attacking based on your movement.

---

## AI_Scripts.cs

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI_Scripts : MonoBehaviour
{
    public NavMeshAgent agent;                 // NavMeshAgent for pathfinding
    public Transform player;                   // Reference to player transform
    public LayerMask whatISGround, whatisPlayer; // Ground and player layers

    // Patrolling variables
    public Vector3 walkpoint;                  
    bool walkpointset;                        
    public float walkpointRange;               

    // Attacking variables
    public float timeBetweenAttacks;           
    bool alreadyattacked;                     
    public GameObject projectile;              

    // Detection ranges
    public float sightRange, attackRange;      
    public bool playerisRange, playerinAttackingRange; 

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        playerisRange = Physics.CheckSphere(transform.position, sightRange, whatisPlayer);
        playerinAttackingRange = Physics.CheckSphere(transform.position, attackRange, whatisPlayer);

        if (!playerisRange && !playerinAttackingRange) Patrolling();
        if (playerisRange && !playerinAttackingRange) ChasePlayer();
        if (playerisRange && playerinAttackingRange) AttackPlayer();
    }

    private void Patrolling()
    {
        if (!walkpointset) SearchWalkpoint();

        if (walkpointset)
            agent.SetDestination(walkpoint);

        Vector3 distancetoWalkPoint = transform.position - walkpoint;

        if (distancetoWalkPoint.magnitude < 1f)
            walkpointset = false;
    }

    private void SearchWalkpoint()
    {
        float randomZ = Random.Range(-walkpointRange, walkpointRange);
        float randomX = Random.Range(-walkpointRange, walkpointRange);
        walkpoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkpoint, -transform.up, 2f, whatISGround))
            walkpointset = true;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        transform.LookAt(player);

        if (!alreadyattacked)
        {
            Rigidbody rb = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * 32f, ForceMode.Impulse);
            rb.AddForce(transform.up * 8f, ForceMode.Impulse);

            alreadyattacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyattacked = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}

PlayerMovement.cs
csharp
Copy
Edit
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public LayerMask groundMask;

    private float groundDistance = 0.4f;
    private Vector3 velocity;

    private bool isGrounded;

    public Transform playerBody;
    public float mouseSensitivity = 100f;
    public float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 direction = transform.right * horizontal + transform.forward * vertical;

        controller.Move(direction * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        playerBody.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        Camera.main.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
Notes
Make sure to adjust NavMesh, layers, and prefab references before running the game.

Tweak variables like speeds, ranges, and cooldowns in the inspector for desired gameplay.

You can extend the AI by adding more states like Fleeing, Searching, or Idle behaviors.

License
This project is open source and free to use for learning and personal projects.

Happy coding! 🚀

yaml
Copy
Edit

---

You can just **copy everything inside the markdown block** and paste it into your README.md on GitHub.

If you want me to generate a shorter version or add sections like screenshots, instructions for building, or credits, just say!






