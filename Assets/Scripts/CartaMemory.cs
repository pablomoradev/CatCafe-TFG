using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Representa una carta del juego Memory.
public class CartaMemory : MonoBehaviour, IPointerClickHandler
{
    private Sprite icono;          // Imagen frontal de la carta.
    private Sprite reverso;        // Imagen trasera de la carta.
    private GestorMemory gestor;   // Referencia al gestor del juego.
    private Image imagen;          // Componente visual de la carta.

    private bool volteada = false;   // Indica si está descubierta.
    private bool emparejada = false; // Indica si ya fue encontrada.

    public Sprite Icono => icono;

    // Configura la carta y muestra el reverso.
    public void Inicializar(Sprite icono, Sprite reverso, GestorMemory gestor)
    {
        this.icono = icono;
        this.reverso = reverso;
        this.gestor = gestor;

        imagen = GetComponent<Image>();
        if (imagen != null) imagen.sprite = reverso;
    }

    // Muestra u oculta el icono de la carta.
    public void Voltear(bool mostrarIcono)
    {
        volteada = mostrarIcono;
        if (imagen != null)
            imagen.sprite = mostrarIcono ? icono : reverso;
    }

    // Marca la carta como emparejada.
    public void Emparejar()
    {
        emparejada = true;
        volteada = true;
    }

    // Gestiona el clic sobre la carta.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (emparejada || volteada) return;
        gestor?.CartaSeleccionada(this);
    }
}