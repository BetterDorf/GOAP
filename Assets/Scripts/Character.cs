using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] public int health;
    [SerializeField] protected int energy;

    public virtual void PlayTurn()
    {

    }
}
