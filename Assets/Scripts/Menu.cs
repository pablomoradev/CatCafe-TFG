using UnityEngine;
using UnityEngine.SceneManagement;

// Menu principal. Gestiona los botones Jugar y Salir
// Si no hay partida guardada, limpia PlayerPrefs para evitar datos residuales
public class Menu : MonoBehaviour
{
    // Carga la cafeteria. Si es partida nueva, resetea todo
    public void Jugar()
    {
        if (GameManager.Instance != null && !GameManager.Instance.HayPartidaGuardada)
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            GestorGatos.Instance?.ReiniciarGatos();
            GestorMinijuegos.Instance?.ResetProgreso();
        }
        SceneManager.LoadScene("CafeScene");
    }

    // Cierra la aplicacion
    public void Salir()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}