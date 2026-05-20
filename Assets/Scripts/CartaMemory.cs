using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CartaMemory : MonoBehaviour, IPointerClickHandler
{
    private Sprite icono;
    private Sprite reverso;
    private GestorMemory gestor;
    private Image imagen;
    private bool volteada = false;
    private bool emparejada = false;

    public Sprite Icono => icono;

    public void Inicializar(Sprite icono, Sprite reverso, GestorMemory gestor)
    {
        this.icono = icono;
        this.reverso = reverso;
        this.gestor = gestor;
        imagen = GetComponent<Image>();
        if (imagen != null) imagen.sprite = reverso;
    }

    public void Voltear(bool mostrarIcono)
    {
        volteada = mostrarIcono;
        if (imagen != null)
            imagen.sprite = mostrarIcono ? icono : reverso;
    }

    public void Emparejar()
    {
        emparejada = true;
        volteada = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (emparejada || volteada) return;
        gestor?.CartaSeleccionada(this);
    }
}