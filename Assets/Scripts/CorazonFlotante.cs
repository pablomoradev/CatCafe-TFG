using UnityEngine;

// Controla el movimiento y desaparición de un corazón flotante.
public class CorazonFlotante : MonoBehaviour
{
    [SerializeField] private float velocidadSubida = 1f; // Velocidad de ascenso.
    [SerializeField] private float duracion = 1f;        // Tiempo de vida.

    private float timer = 0f;            // Tiempo transcurrido.
    private SpriteRenderer sr;           // Sprite del corazón.

    private void Start()
    {
        // Obtiene el SpriteRenderer.
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Actualiza el tiempo y mueve el corazón hacia arriba.
        timer += Time.deltaTime;
        transform.position += Vector3.up * velocidadSubida * Time.deltaTime;

        // Reduce gradualmente la transparencia.
        float alpha = 1f - (timer / duracion);

        if (sr != null)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }

        // Destruye el objeto al finalizar su duración.
        if (timer >= duracion)
            Destroy(gameObject);
    }
}