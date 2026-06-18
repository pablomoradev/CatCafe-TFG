using UnityEngine;

// Controla la apertura del tablón de gatos y su UI asociada.
public class TablonGatos : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject panelTablon;   // Panel del tablón.
    [SerializeField] private UITablonGatos uiTablon;   // UI del tablón.

    private bool _panelAbierto = false; // Estado del panel.

    private void Update()
    {
        // Cierra el panel con Escape si está abierto.
        if (_panelAbierto && Input.GetKeyDown(KeyCode.Escape))
            CerrarPanel();
    }

    // Alterna apertura/cierre del tablón.
    public void AlHacerClic()
    {
        if (_panelAbierto) CerrarPanel();
        else AbrirPanel();
    }

    // Abre el tablón, genera gatos y refresca UI.
    public void AbrirPanel()
    {
        GestorGatos.Instance.GenerarGatosTablon();
        uiTablon.RefrescarPanel();

        panelTablon.SetActive(true);
        _panelAbierto = true;

        Time.timeScale = 0f;
    }

    // Cierra el tablón y reanuda el juego.
    public void CerrarPanel()
    {
        panelTablon.SetActive(false);
        _panelAbierto = false;
        Time.timeScale = 1f;
    }
}