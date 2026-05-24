// GestorMinijuegos.cs — versión compatible con GestorCazaRatones
using UnityEngine;
using UnityEngine.SceneManagement;

public class GestorMinijuegos : MonoBehaviour
{
    public static GestorMinijuegos Instance { get; private set; }

    private static readonly string[] ordenEscenas = {
        "MinijuegoCazaRatones",  // 0 → Gato 1
        "MinijuegoMemory",       // 1 → Monedas
        "MinijuegoDados",        // 2 → Gato 2
        "MinijuegoAtrapar",      // 3 → Monedas
        "MinijuegoBuscaBugs",    // 4 → Gato 3
        "MinijuegoCodigo"        // 5 → Créditos
    };

    private const string KEY_PROGRESO = "MinijuegoProgreso";

    public int ProgresoActual => PlayerPrefs.GetInt(KEY_PROGRESO, 0);
    public bool TodosCompletados => ProgresoActual >= ordenEscenas.Length;

    // GatoActual: el gato que se desbloqueará al ganar el minijuego actual
    public DatosGato GatoActual
    {
        get
        {
            int[] indicesGato = { 0, -1, 1, -1, 2, -1 }; // -1 = no hay gato en ese minijuego
            int prog = ProgresoActual;
            if (prog >= indicesGato.Length) return null;
            int idx = indicesGato[prog];
            if (idx < 0) return null;
            return GestorGatos.Instance?.ObtenerGatoPorIndex(idx);
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LanzarMinijuegoActual()
    {
        if (TodosCompletados)
        {
            GestorDialogos.Instance?.MostrarDialogo("¡Ya has completado todos los minijuegos!");
            return;
        }
        SceneManager.LoadScene(ordenEscenas[ProgresoActual]);
    }

    public void MinijuegoCompletado()
    {
        int actual = ProgresoActual;
        if (actual >= ordenEscenas.Length) return;
        PlayerPrefs.SetInt(KEY_PROGRESO, actual + 1);
        PlayerPrefs.Save();
        Debug.Log("Progreso guardado: " + ProgresoActual); 
    }

    public void MinijuegoGanado()
    {
        int actual = ProgresoActual - 1; // ya se incrementó en MinijuegoCompletado
        switch (actual)
        {
            case 0: // CazaRatones → Gato 1
                GestorGatos.Instance?.SalvarGatoPorIndex(0);
                break;
            case 1: // Memory → Monedas
                GameManager.Instance?.AnadirDinero(30);
                break;
            case 2: // Dados → Gato 2
                GestorGatos.Instance?.SalvarGatoPorIndex(1);
                break;
            case 3: // Atrapar → Monedas
                GameManager.Instance?.AnadirDinero(25);
                break;
            case 4: // BuscaBugs → Gato 3
                GestorGatos.Instance?.SalvarGatoPorIndex(2);
                break;
            case 5: // Quiz Java → Créditos
                SceneManager.LoadScene("Creditos");
                break;
        }
    }

    public void MinijuegoPerdido()
    {
        // No avanza el progreso, el jugador puede reintentar
        // Puedes añadir lógica de penalización aquí si quieres
    }

    public string NombreMinijuegoActual()
    {
        if (TodosCompletados) return "¡Completado!";
        return ordenEscenas[ProgresoActual].Replace("Minijuego", "");
    }

    public void ResetProgreso()
    {
        PlayerPrefs.SetInt(KEY_PROGRESO, 0);
        PlayerPrefs.Save();
    }
}