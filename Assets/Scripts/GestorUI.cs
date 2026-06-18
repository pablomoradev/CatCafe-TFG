using UnityEngine;
using TMPro;

// Gestiona la UI de la escena principal (texto del dinero)
public class GestorUI : MonoBehaviour
{
    public static GestorUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI textoDinero; // Texto que muestra el dinero actual

    void Awake() { Instance = this; }

    void Start()
    {
        ActualizarDinero(GameManager.Instance != null ? GameManager.Instance.Dinero : 0);
    }

    // Actualiza el texto del dinero en pantalla
    public void ActualizarDinero(int cantidad)
    {
        if (textoDinero != null)
            textoDinero.text = cantidad.ToString();
    }
}