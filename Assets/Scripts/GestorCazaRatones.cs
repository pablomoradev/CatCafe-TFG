using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Gestor principal del minijuego "Caza de Ratones".
public class GestorCazaRatones : MonoBehaviour
{
    public static GestorCazaRatones Instance { get; private set; }

    [Header("Config por dificultad")]
    [SerializeField] private float[] tiempoLimite = { 30f, 20f, 15f };     // Tiempo máximo.
    [SerializeField] private int[] ratonesNecesarios = { 8, 12, 15 };     // Objetivo.
    [SerializeField] private float[] tiempoEntreSpawn = { 1.5f, 1f, 0.7f }; // Frecuencia de spawn.

    [Header("Posiciones de agujeros")]
    [SerializeField] private Transform[] agujeros; // Lugares donde aparecen ratones.

    [Header("Prefab ratón")]
    [SerializeField] private GameObject prefabRaton; // Prefab del ratón.

    [Header("UI")]
    [SerializeField] private UIMinijuego uiMinijuego; // UI del minijuego.

    private int dificultad = 0;        // Nivel de dificultad actual.
    private float timerActual;         // Tiempo restante.
    private int ratonesAtrapados;      // Progreso del jugador.
    private bool juegoActivo = false;  // Estado del juego.

    private List<GameObject> ratonesActivos = new List<GameObject>();

    private void Awake()
    {
        // Singleton.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        juegoActivo = false;

        // Inicia cuando termina la cuenta atrás.
        CuentaAtras ca = FindObjectOfType<CuentaAtras>();
        if (ca != null)
            ca.OnFinCuentaAtras += IniciarJuego;
        else
            IniciarJuego();
    }

    // Inicia el minijuego.
    private void IniciarJuego()
    {
        ratonesAtrapados = 0;
        timerActual = tiempoLimite[dificultad];
        juegoActivo = true;

        uiMinijuego?.ActualizarContador(ratonesAtrapados, ratonesNecesarios[dificultad]);
        uiMinijuego?.ActualizarTimer(timerActual);

        StartCoroutine(SpawnRatones());
    }

    private void Update()
    {
        if (!juegoActivo) return;

        timerActual -= Time.deltaTime;
        uiMinijuego?.ActualizarTimer(timerActual);

        if (timerActual <= 0)
            FinJuego(false);
    }

    // Spawnea ratones continuamente.
    private IEnumerator SpawnRatones()
    {
        while (juegoActivo)
        {
            yield return new WaitForSeconds(tiempoEntreSpawn[dificultad]);
            if (juegoActivo) SpawnRaton();
        }
    }

    // Crea un ratón en un agujero aleatorio.
    private void SpawnRaton()
    {
        if (agujeros.Length == 0) return;

        Transform agujero = agujeros[Random.Range(0, agujeros.Length)];
        GameObject raton = Instantiate(prefabRaton, agujero.position, Quaternion.identity);
        ratonesActivos.Add(raton);
    }

    // Llamado cuando el jugador atrapa un ratón.
    public void RatonAtrapado()
    {
        if (!juegoActivo) return;

        ratonesAtrapados++;
        uiMinijuego?.ActualizarContador(ratonesAtrapados, ratonesNecesarios[dificultad]);

        if (ratonesAtrapados >= ratonesNecesarios[dificultad])
            FinJuego(true);
    }

    // Finaliza el minijuego.
    private void FinJuego(bool ganado)
    {
        juegoActivo = false;
        StopAllCoroutines();

        foreach (GameObject r in ratonesActivos)
            if (r != null) r.SetActive(false);

        if (ganado)
        {
            string nombreGato = GestorMinijuegos.Instance?.GatoActual?.nombreGato ?? "el gato";

            uiMinijuego?.MostrarMensajeGanado($"¡Has ganado! {nombreGato} confía en ti !!");
            GestorMinijuegos.Instance?.MinijuegoCompletado();
            GestorMinijuegos.Instance?.MinijuegoGanado();

            Debug.Log("GestorMinijuegos: " + GestorMinijuegos.Instance);
            Debug.Log("Ganado llamado");
        }
        else
        {
            uiMinijuego?.MostrarMensajePerdido("¡Se acabó el tiempo! ¿Lo intentamos de nuevo?");
            GestorMinijuegos.Instance?.MinijuegoPerdido();
        }

        // Registro de la partida.
        GestorBBDD.Instance?.RegistrarPartidaMinijuego("Caza Ratones", ratonesAtrapados, true);
    }
}