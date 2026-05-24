using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Dinero { get; private set; } = 0;
    public float MultiplicadorGanancias { get; private set; } = 1f;
    public bool HayPartidaGuardada => PlayerPrefs.HasKey("Dinero");

    private GatoSalvado gatoActualEnEscena = null;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnScenaCargada;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnScenaCargada;
    }

    private void OnScenaCargada(Scene escena, LoadSceneMode modo)
    {
        if (escena.name == "CafeScene")
            CargarPartida();
    }

    public void RegistrarGatoActivo(GatoSalvado gato)
    {
        gatoActualEnEscena = gato;
    }

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

    public bool TieneSuficiente(int cantidad) => Dinero >= cantidad;

    public void GastarDinero(int cantidad)
    {
        Dinero = Mathf.Max(0, Dinero - cantidad);
        GestorUI.Instance?.ActualizarDinero(Dinero);
    }

    public void ModificarMultiplicador(float multiplicador)
    {
        MultiplicadorGanancias *= multiplicador;
    }

    public void CargarEscena(string nombreEscena) => SceneManager.LoadScene(nombreEscena);

    public void GuardarPartida()
    {
        PlayerPrefs.SetInt("Dinero", Dinero);
        PlayerPrefs.SetFloat("Multiplicador", MultiplicadorGanancias);

        if (GestorGatos.Instance != null)
        {
            DatosGato[] gatos = GestorGatos.Instance.ObtenerTodosLosGatos();
            for (int i = 0; i < gatos.Length; i++)
                PlayerPrefs.SetInt("Gato_" + i + "_Salvado", gatos[i].estaSalvado ? 1 : 0);
            PlayerPrefs.SetInt("NumGatos", gatos.Length);
            PlayerPrefs.SetInt("GatoActivo", GestorGatos.Instance.ObtenerIndexGatoActivo());
        }

        PlayerPrefs.Save();
        Debug.Log("Partida guardada. Dinero: " + Dinero);
    }

    public void CargarPartida()
    {
        Dinero = PlayerPrefs.GetInt("Dinero", 0);
        MultiplicadorGanancias = PlayerPrefs.GetFloat("Multiplicador", 1f);

        if (GestorGatos.Instance != null)
        {
            DatosGato[] gatos = GestorGatos.Instance.ObtenerTodosLosGatos();
            int numGuardados = PlayerPrefs.GetInt("NumGatos", 0);
            for (int i = 0; i < gatos.Length && i < numGuardados; i++)
                gatos[i].estaSalvado = PlayerPrefs.GetInt("Gato_" + i + "_Salvado", 0) == 1;

            int indexActivo = PlayerPrefs.GetInt("GatoActivo", -1);
            if (indexActivo >= 0)
                GestorGatos.Instance.EstablecerGatoActivo(indexActivo);
        }

        GestorUI.Instance?.ActualizarDinero(Dinero);
        Debug.Log("Partida cargada. Dinero: " + Dinero);
    }

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