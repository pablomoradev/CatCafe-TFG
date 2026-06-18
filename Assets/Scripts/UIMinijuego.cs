using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Gestiona la interfaz del minijuego.
public class UIMinijuego : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI textoTimer;     // Tiempo restante.
    [SerializeField] private TextMeshProUGUI textoContador;  // Progreso.
    [SerializeField] private TextMeshProUGUI textoMensaje;   // Mensajes de victoria/derrota.
    [SerializeField] private Button botonReintentar;         // Botón reintentar.
    [SerializeField] private Button botonVolver;             // Botón volver.

    private void Start()
    {
        // Oculta elementos al inicio.
        textoMensaje?.gameObject.SetActive(false);
        botonReintentar?.gameObject.SetActive(false);
        botonVolver?.gameObject.SetActive(false);

        // Asigna eventos a botones.
        botonReintentar?.onClick.AddListener(Reintentar);
        botonVolver?.onClick.AddListener(Volver);
    }

    // Actualiza el temporizador.
    public void ActualizarTimer(float segundos)
    {
        if (textoTimer != null)
            textoTimer.text = $"{Mathf.CeilToInt(segundos)}s";
    }

    // Actualiza el contador de objetivos.
    public void ActualizarContador(int atrapados, int necesarios)
    {
        if (textoContador != null)
            textoContador.text = $"{atrapados} / {necesarios}";
    }

    // Muestra mensaje de victoria.
    public void MostrarMensajeGanado(string mensaje)
    {
        if (textoMensaje != null)
        {
            textoMensaje.text = mensaje;
            textoMensaje.gameObject.SetActive(true);
        }

        botonVolver?.gameObject.SetActive(true);
        botonReintentar?.gameObject.SetActive(false);
    }

    // Muestra mensaje de derrota.
    public void MostrarMensajePerdido(string mensaje)
    {
        if (textoMensaje != null)
        {
            textoMensaje.text = mensaje;
            textoMensaje.gameObject.SetActive(true);
        }

        botonReintentar?.gameObject.SetActive(true);
        botonVolver?.gameObject.SetActive(true);
    }

    // Reinicia el minijuego.
    private void Reintentar()
    {
        GameManager.Instance?.CargarEscena("MinijuegoCazaRatones");
    }

    // Vuelve a la escena del café.
    private void Volver()
    {
        GameManager.Instance?.CargarEscena("CafeScene");
    }
}