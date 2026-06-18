using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// Gestiona el minijuego Memory Tech
// El jugador debe emparejar cartas con logos de tecnologias antes de que se acabe el tiempo
public class GestorMemory : MonoBehaviour
{
    // Cartas del tablero
    [Header("Cartas")]
    [SerializeField] private GameObject prefabCarta;
    [SerializeField] private Transform contenedorCartas;
    [SerializeField] private Sprite[] iconosProductos;  // Sprites de las tecnologias (Java, C#, SQL, etc.)
    [SerializeField] private Sprite reversoCarta;        // Sprite del dorso de la carta

    // Elementos de la UI
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI textoTiempo;
    [SerializeField] private TextMeshProUGUI textoPareja;
    [SerializeField] private GameObject panelResultado;
    [SerializeField] private TextMeshProUGUI textoResultado;
    [SerializeField] private Button botonVolver;

    // Configuracion
    [Header("Config")]
    [SerializeField] private float tiempoLimite = 60f;
    [SerializeField] private int recompensaGanar = 30;

    // Estado interno
    private List<CartaMemory> cartasVolteadas = new List<CartaMemory>(); // Cartas actualmente boca arriba
    private int parejasEncontradas = 0;
    private int totalParejas;
    private float tiempoActual;
    private bool juegoActivo = false;
    private bool comprobando = false;  // True mientras se comprueba si dos cartas son pareja
    private bool haGanado = false;

    // Espera la cuenta atras antes de empezar
    void Start()
    {
        tiempoActual = tiempoLimite;
        panelResultado?.SetActive(false);
        botonVolver?.onClick.AddListener(Volver);
        juegoActivo = false;

        CuentaAtras ca = FindObjectOfType<CuentaAtras>();
        if (ca != null)
            ca.OnFinCuentaAtras += IniciarJuego;
        else
            IniciarJuego();
    }

    private void IniciarJuego()
    {
        GenerarTablero();
        juegoActivo = true;
    }

    // Resta tiempo cada frame
    void Update()
    {
        if (!juegoActivo) return;
        tiempoActual -= Time.deltaTime;
        if (textoTiempo != null)
            textoTiempo.text = "Tiempo : " + Mathf.CeilToInt(tiempoActual);
        if (tiempoActual <= 0) TerminarJuego(false);
    }

    // Crea el tablero duplicando los sprites y mezclando las cartas
    private void GenerarTablero()
    {
        // Duplicar cada icono para tener parejas
        List<Sprite> sprites = new List<Sprite>();
        foreach (Sprite s in iconosProductos)
        {
            sprites.Add(s);
            sprites.Add(s);
        }

        // Mezclar aleatoriamente (algoritmo Fisher-Yates)
        for (int i = sprites.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Sprite temp = sprites[i];
            sprites[i] = sprites[j];
            sprites[j] = temp;
        }

        totalParejas = iconosProductos.Length;

        // Instanciar cada carta con su icono
        for (int i = 0; i < sprites.Count; i++)
        {
            GameObject carta = Instantiate(prefabCarta, contenedorCartas);
            CartaMemory cm = carta.GetComponent<CartaMemory>();
            cm.Inicializar(sprites[i], reversoCarta, this);
        }

        juegoActivo = true;
    }

    // Llamado por CartaMemory al hacer clic en una carta
    public void CartaSeleccionada(CartaMemory carta)
    {
        if (comprobando) return;                    // No permitir clics mientras se comprueba pareja
        if (cartasVolteadas.Contains(carta)) return; // No permitir clicar la misma carta
        if (cartasVolteadas.Count >= 2) return;      // Maximo 2 cartas volteadas a la vez

        carta.Voltear(true);
        cartasVolteadas.Add(carta);

        // Si hay 2 cartas volteadas, comprobar si son pareja
        if (cartasVolteadas.Count == 2)
            StartCoroutine(ComprobarPareja());
    }

    // Espera 1 segundo y comprueba si las dos cartas volteadas son pareja
    private IEnumerator ComprobarPareja()
    {
        comprobando = true;
        yield return new WaitForSeconds(1f);

        if (cartasVolteadas[0].Icono == cartasVolteadas[1].Icono)
        {
            // Son pareja: dejarlas boca arriba
            cartasVolteadas[0].Emparejar();
            cartasVolteadas[1].Emparejar();
            parejasEncontradas++;

            if (textoPareja != null)
                textoPareja.text = "Parejas: " + parejasEncontradas + "/" + totalParejas;

            // Si encontro todas las parejas, gana
            if (parejasEncontradas >= totalParejas)
                TerminarJuego(true);
        }
        else
        {
            // No son pareja: voltearlas boca abajo
            cartasVolteadas[0].Voltear(false);
            cartasVolteadas[1].Voltear(false);
        }

        cartasVolteadas.Clear();
        comprobando = false;
    }

    // Termina el juego y notifica al GestorMinijuegos si gano
    private void TerminarJuego(bool gano)
    {
        juegoActivo = false;
        panelResultado?.SetActive(true);
        haGanado = gano;

        if (gano)
        {
            GestorMinijuegos.Instance?.MinijuegoCompletado();
            GestorMinijuegos.Instance?.MinijuegoGanado();
            if (textoResultado != null)
                textoResultado.text = "¡Has ganado!\n+" + recompensaGanar + " monedas";
        }
        else
        {
            if (textoResultado != null)
                textoResultado.text = "¡Se acabó el tiempo!";
        }
        GestorBBDD.Instance?.RegistrarPartidaMinijuego("Memory Tech", parejasEncontradas * 5, gano);
    }

    // Guarda monedas pendientes en PlayerPrefs y vuelve a la cafeteria
    public void Volver()
    {
        if (haGanado)
        {
            int pendientes = PlayerPrefs.GetInt("Monedas_Pendientes", 0);
            PlayerPrefs.SetInt("Monedas_Pendientes", pendientes + recompensaGanar);
            PlayerPrefs.Save();
        }
        SceneManager.LoadScene("CafeScene");
    }
}