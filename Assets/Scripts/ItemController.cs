using UnityEngine;

public class ItemController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Si el objeto que entra es el Player
        {
            if (GameManager.Instance != null)
            {

                GameManager.Instance.GetItemBig();
            }
            // Elimina el ítem del mapa
            Destroy(gameObject);
        }
    }
}
