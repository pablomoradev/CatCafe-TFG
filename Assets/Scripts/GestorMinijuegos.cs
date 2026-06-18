using UnityEngine;
using UnityEngine.SceneManagement;

// Gestiona el flujo progresivo de los 6 minijuegos
// Controla que minijuego toca, guarda el progreso en PlayerPrefs
// y aplica las recompensas (gato o monedas) al ganar
public class GestorMinijuegos : MonoBehaviour
{
    public static GestorMinijuegos Instance { get; private set; }

    // Orden de las escenas de minijuegos en el flujo progresivo
    private static readonly string[] ordenEscenas = {
        "MinijuegoCazaRatones",  // 0 → Gato 1 (Milo)
        "MinijuegoMemory",       // 1 → 30 Monedas
        "MinijuegoDados",        // 2 → Gato 2 (Lira)
        "MinijuegoAtrapar",      // 3 → 25 Monedas
        "MinijuegoBuscaBugs",    // 4 → Gato 3 (Alex)
        "MinijuegoCodigo"        // 5 → Creditos (fin del juego)
    };

    // Clave para guardar el progreso en PlayerPrefs
    private const string KEY_PROGRESO = "MinijuegoProgreso";

    // Indice del minijuego actual (0 a 5)
    public int ProgresoActual => PlayerPrefs.GetInt(KEY_PROGRESO, 0);

    // True si ya se han completado los 6 minijuegos
    public bool TodosCompletados => ProgresoActual >= ordenEscenas.Length;

    // Devuelve el gato que se desbloquea con el minijuego actual, o null si da monedas
    public DatosGato GatoActual
    {
        get
        {
            // Indices de gato por minijuego: 0=Milo, -1=monedas, 1=Lira, -1=monedas, 2=Alex, -1=creditos
            int[] indicesGato = { 0, -1, 1, -1, 2, -1 };
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

    // Carga la escena del minijuego que toca en el flujo
    public void LanzarMinijuegoActual()
    {
        if (TodosCompletados)
        {
            GestorDialogos.Instance?.MostrarDialogo("¡Ya has completado todos los minijuegos!");
            return;
        }
        SceneManager.LoadScene(ordenEscenas[ProgresoActual]);
    }

    // Avanza el progreso al siguiente minijuego y lo guarda en PlayerPrefs
    public void MinijuegoCompletado()
    {
        int actual = ProgresoActual;
        if (actual >= ordenEscenas.Length) return;
        PlayerPrefs.SetInt(KEY_PROGRESO, actual + 1);
        PlayerPrefs.Save();
    }

    // Aplica la recompensa del minijuego que se acaba de completar
    public void MinijuegoGanado()
    {
        int actual = ProgresoActual - 1; // Ya se incremento en MinijuegoCompletado
        switch (actual)
        {
            case 0: // CazaRatones → salvar Milo
                GestorGatos.Instance?.SalvarGatoPorIndex(0);
                break;
            case 1: // Memory → 30 monedas
                GameManager.Instance?.AnadirDinero(30);
                PlayerPrefs.SetInt("Dinero", GameManager.Instance.Dinero);
                PlayerPrefs.Save();
                break;
            case 2: // Dados → salvar Lira
                GestorGatos.Instance?.SalvarGatoPorIndex(1);
                break;
            case 3: // Atrapar → 25 monedas
                GameManager.Instance?.AnadirDinero(25);
                break;
            case 4: // BuscaBugs → salvar Alex
                GestorGatos.Instance?.SalvarGatoPorIndex(2);
                break;
            case 5: // Quiz Java → creditos
                SceneManager.LoadScene("Creditos");
                break;
        }
    }

    // No avanza el progreso, el jugador puede reintentar
    public void MinijuegoPerdido() { }

    // Devuelve el nombre del minijuego actual sin el prefijo "Minijuego"
    public string NombreMinijuegoActual()
    {
        if (TodosCompletados) return "¡Completado!";
        return ordenEscenas[ProgresoActual].Replace("Minijuego", "");
    }

    // Resetea el progreso a 0 (para testing o nueva partida)
    public void ResetProgreso()
    {
        PlayerPrefs.SetInt(KEY_PROGRESO, 0);
        PlayerPrefs.Save();
    }
}