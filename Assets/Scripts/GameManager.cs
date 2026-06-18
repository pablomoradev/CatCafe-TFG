using UnityEngine;
using UnityEngine.SceneManagement;

// Gestor principal del juego. Singleton con DontDestroyOnLoad
// Gestiona el dinero, multiplicadores, guardado/carga y cambio de escenas
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Dinero { get; private set; } = 0;
    public float MultiplicadorGanancias { get; private set; } = 1f;
    public bool HayPartidaGuardada => PlayerPrefs.HasKey("Dinero");

    private GatoSalvado gatoActualEnEscena = null; // Referencia al gato activo en la cafeteria

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Fuerza el reset en la primera ejecucion de cada build nueva
        if (PlayerPrefs.GetInt("Version", 0) < 5)
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetInt("Version", 5);
            PlayerPrefs.Save();
        }

        Screen.SetResolution(1280, 768, false);
    }

    // Suscribirse al evento de carga de escena
    private void OnEnable() { SceneManager.sceneLoaded += OnScenaCargada; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnScenaCargada; }

    // Al cargar CafeScene, restaurar la partida guardada
    private void OnScenaCargada(Scene escena, LoadSceneMode modo)
    {
        if (escena.name == "CafeScene")
            CargarPartida();
    }

    // Registra el gato activo para calcular su bonus en los pedidos
    public void RegistrarGatoActivo(GatoSalvado gato) { gatoActualEnEscena = gato; }

    // Anade dinero con multiplicadores aplicados (bonus de gato + multiplicador global)
    public void AnadirDinero(int cantidad)
    {
        float bonusGato = 1f;
        if (gatoActualEnEscena != null)
            bonusGato = 1f + gatoActualEnEscena.ObtenerMultiplicadorBonus() * 0.5f;

        int cantidadFinal = Mathf.RoundToInt(cantidad * MultiplicadorGanancias * bonusGato);
        Dinero += cantidadFinal;
        GestorUI.Instance?.ActualizarDinero(Dinero);
        GestorBBDD.Instance?.ActualizarDineroJugador(Dinero);
    }

    // Comprueba si el jugador tiene suficiente dinero
    public bool TieneSuficiente(int cantidad) => Dinero >= cantidad;

    // Gasta dinero (penalizaciones, desbloqueo de productos)
    public void GastarDinero(int cantidad)
    {
        Dinero = Mathf.Max(0, Dinero - cantidad);
        GestorUI.Instance?.ActualizarDinero(Dinero);
    }

    // Modifica el multiplicador global de ganancias (bonus de gato MasGanancias)
    public void ModificarMultiplicador(float multiplicador) { MultiplicadorGanancias *= multiplicador; }

    // Carga una escena por nombre
    public void CargarEscena(string nombreEscena) => SceneManager.LoadScene(nombreEscena);

    // Guarda la partida completa en PlayerPrefs
    public void GuardarPartida()
    {
        PlayerPrefs.SetInt("Dinero", Dinero);
        PlayerPrefs.SetFloat("Multiplicador", MultiplicadorGanancias);

        // Guardar estado de cada gato (salvado o no)
        if (GestorGatos.Instance != null)
        {
            DatosGato[] gatos = GestorGatos.Instance.ObtenerTodosLosGatos();
            for (int i = 0; i < gatos.Length; i++)
                PlayerPrefs.SetInt("Gato_" + i + "_Salvado", gatos[i].estaSalvado ? 1 : 0);
            PlayerPrefs.SetInt("NumGatos", gatos.Length);
            PlayerPrefs.SetInt("GatoActivo", GestorGatos.Instance.ObtenerIndexGatoActivo());
        }

        PlayerPrefs.Save();
    }

    // Carga la partida desde PlayerPrefs
    public void CargarPartida()
    {
        Dinero = PlayerPrefs.GetInt("Dinero", 0);
        MultiplicadorGanancias = PlayerPrefs.GetFloat("Multiplicador", 1f);

        // Restaurar estado de gatos
        if (GestorGatos.Instance != null)
        {
            DatosGato[] gatos = GestorGatos.Instance.ObtenerTodosLosGatos();
            int numGuardados = PlayerPrefs.GetInt("NumGatos", 0);
            for (int i = 0; i < gatos.Length && i < numGuardados; i++)
                gatos[i].estaSalvado = PlayerPrefs.GetInt("Gato_" + i + "_Salvado", 0) == 1;

            int indexActivo = PlayerPrefs.GetInt("GatoActivo", -1);
            if (indexActivo >= 0) GestorGatos.Instance.EstablecerGatoActivo(indexActivo);
        }

        // Recoger monedas pendientes de minijuegos
        int pendientes = PlayerPrefs.GetInt("Monedas_Pendientes", 0);
        if (pendientes > 0)
        {
            Dinero += pendientes;
            PlayerPrefs.DeleteKey("Monedas_Pendientes");
        }

        GestorUI.Instance?.ActualizarDinero(Dinero);
    }

    // Reinicia toda la partida y vuelve al menu principal
    public void ReiniciarPartida()
    {
        Dinero = 0;
        MultiplicadorGanancias = 1f;
        gatoActualEnEscena = null;
        PlayerPrefs.DeleteAll();
        GestorGatos.Instance?.ReiniciarGatos();
        CargarEscena("MenuInicio");
    }
}