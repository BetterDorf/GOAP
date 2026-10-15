using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] public int health;
    [SerializeField] protected int energy;

    public virtual void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("took " + damage + " damage");
        if (health <= 0)
        {
            Debug.Log("Died");
            GameDirector.GameDirectorInstance.EndGame();
        }
    }

    public virtual void PlayTurn()
    {

    }
}
