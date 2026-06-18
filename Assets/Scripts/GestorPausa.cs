using UnityEngine;
using UnityEngine.SceneManagement;

// Gestiona la pausa del juego con la tecla ESC
// Permite guardar la partida y salir al menu principal
public class GestorPausa : MonoBehaviour
{
    [Header("Panel pausa")]
    [SerializeField] private GameObject panelPausa;

    private bool pausado = false;

    // Detecta la tecla ESC para pausar/despausar
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePausa();
    }

    // Alterna entre pausado y no pausado
    public void TogglePausa()
    {
        pausado = !pausado;
        panelPausa?.SetActive(pausado);
        Time.timeScale = pausado ? 0f : 1f;
    }

    // Guarda la partida y vuelve al menu principal
    public void GuardarYSalir()
    {
        Time.timeScale = 1f;
        GameManager.Instance?.GuardarPartida();
        SceneManager.LoadScene("MenuInicio");
    }
}