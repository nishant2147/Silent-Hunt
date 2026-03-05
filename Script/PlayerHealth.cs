using UnityEngine;
using UnityEngine.AI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 50;
    private int currentHealth;
    private NavMeshAgent agent;
    public GameObject bloodEffectPrefab;

    void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        Debug.Log("Player Health: " + currentHealth);

        if (bloodEffectPrefab != null)
        {
            GameObject blood = Instantiate(
                bloodEffectPrefab,
                transform.position,
                Quaternion.identity
            );

            Destroy(blood, 2f);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player Died");

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        GameManager.Instance.isGameStarted = false;
        Time.timeScale = 0f;
        Destroy(gameObject);
    }
}
