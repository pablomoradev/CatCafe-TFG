using UnityEngine;
using UnityEngine.Events;

// Zona interactuable que responde a clics del jugador.
[RequireComponent(typeof(BoxCollider2D))]
public class ZonaClicable : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private string nombreZona; // Nombre identificador de la zona.
    [SerializeField] private Color colorResaltado = new Color(1, 1, 1, 0.5f); // Color al pasar el mouse.

    [Header("Eventos")]
    public UnityEvent AlHacerClic; // Evento disparado al hacer clic.

    private SpriteRenderer sr;     // Renderer de la zona.
    private Color colorOriginal;   // Color original del sprite.

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        if (sr != null)
            colorOriginal = sr.color;

        // Asegura que el collider sea trigger para interacción.
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    // Resalta la zona al pasar el mouse.
    void OnMouseEnter()
    {
        if (sr != null)
            sr.color = colorResaltado;
    }

    // Restaura el color al salir el mouse.
    void OnMouseExit()
    {
        if (sr != null)
            sr.color = colorOriginal;
    }

    // Ejecuta la acción al hacer clic.
    void OnMouseDown()
    {
        AlHacerClic?.Invoke();
    }
}