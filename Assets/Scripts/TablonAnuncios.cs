using UnityEngine;

// Controla la apertura y cierre del tablón de anuncios.
public class TablonAnuncios : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject panelTablon; // Panel del tablón.

    private bool _panelAbierto = false; // Estado del panel.

    private void Update()
    {
        // Cierra el panel con Escape si está abierto.
        if (_panelAbierto && Input.GetKeyDown(KeyCode.Escape))
            CerrarPanel();
    }

    // Alterna el panel al hacer clic.
    public void AlHacerClic()
    {
        if (_panelAbierto)
            CerrarPanel();
        else
            AbrirPanel();
    }

    // Abre el panel y pausa el juego.
    public void AbrirPanel()
    {
        panelTablon.SetActive(true);
        _panelAbierto = true;
        Time.timeScale = 0f;
    }

    // Cierra el panel y reanuda el juego.
    public void CerrarPanel()
    {
        panelTablon.SetActive(false);
        _panelAbierto = false;
        Time.timeScale = 1f;
    }
}