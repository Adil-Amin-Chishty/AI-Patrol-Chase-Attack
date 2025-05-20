using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI_Scripts : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatISGround, whatisPlayer;

    // Patrolling
    public Vector3 walkpoint;
    bool walkpointset;
    public float walkpointRange;

    // Attacking
    public float timeBetweenAttacks;
    bool alreadyattacked;
    public GameObject projectile;

    // States
    public float sightRange, attackRange;
    public bool playerisRange, playerinAttackingRange;

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    private void Update()
    {
        // Check if Player is in sight and in attack range
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

        // If the walk point is reached
        if (distancetoWalkPoint.magnitude < 1f)
            walkpointset = false;
    }

    private void SearchWalkpoint()
    {
        // Calculate random point in range
        float randomZ = Random.Range(-walkpointRange, walkpointRange);
        float randomX = Random.Range(-walkpointRange, walkpointRange);
        walkpoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        // Check if the point is on the ground
        if (Physics.Raycast(walkpoint, -transform.up, 2f, whatISGround))
            walkpointset = true;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        // Make sure enemy doesn't move
        agent.SetDestination(transform.position);
        transform.LookAt(player);

        if (!alreadyattacked)
        {
            // Attack code
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
