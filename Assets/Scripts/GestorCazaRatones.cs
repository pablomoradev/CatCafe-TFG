using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestorCazaRatones : MonoBehaviour
{
    public static GestorCazaRatones Instance { get; private set; }

    [Header("Config por dificultad")]
    [SerializeField] private float[] tiempoLimite = { 30f, 20f, 15f };
    [SerializeField] private int[] ratonesNecesarios = { 8, 12, 15 };
    [SerializeField] private float[] tiempoEntreSpawn = { 1.5f, 1f, 0.7f };

    [Header("Posiciones de agujeros")]
    [SerializeField] private Transform[] agujeros;

    [Header("Prefab ratón")]
    [SerializeField] private GameObject prefabRaton;

    [Header("UI")]
    [SerializeField] private UIMinijuego uiMinijuego;

    private int dificultad = 0;
    private float timerActual;
    private int ratonesAtrapados;
    private bool juegoActivo = false;
    private List<GameObject> ratonesActivos = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        juegoActivo = false;

        CuentaAtras ca = FindObjectOfType<CuentaAtras>();
        if (ca != null)
            ca.OnFinCuentaAtras += IniciarJuego;
        else
            IniciarJuego();
    }

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

    private IEnumerator SpawnRatones()
    {
        while (juegoActivo)
        {
            yield return new WaitForSeconds(tiempoEntreSpawn[dificultad]);
            if (juegoActivo) SpawnRaton();
        }
    }

    private void SpawnRaton()
    {
        if (agujeros.Length == 0) return;

        Transform agujero = agujeros[Random.Range(0, agujeros.Length)];
        GameObject raton = Instantiate(prefabRaton, agujero.position, Quaternion.identity);
        ratonesActivos.Add(raton);
    }

    public void RatonAtrapado()
    {
        if (!juegoActivo) return;

        ratonesAtrapados++;
        uiMinijuego?.ActualizarContador(ratonesAtrapados, ratonesNecesarios[dificultad]);

        if (ratonesAtrapados >= ratonesNecesarios[dificultad])
            FinJuego(true);
    }

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
            GestorMinijuegos.Instance?.MinijuegoGanado();
        }
        else
        {
            uiMinijuego?.MostrarMensajePerdido("¡Se acabó el tiempo! ¿Lo intentamos de nuevo?");
            GestorMinijuegos.Instance?.MinijuegoPerdido();
        }
        GestorBBDD.Instance?.RegistrarPartidaMinijuego("Caza Ratones", ratonesAtrapados);
    }
}