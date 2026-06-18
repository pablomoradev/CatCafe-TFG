using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// Gestiona el minijuego de Dados contra Davy Jones
// El jugador y la IA se enfrentan en rondas de dados con apuestas
// Gana quien llegue a 4 victorias o tenga mas puntos tras 7 rondas
public class GestorDados : MonoBehaviour
{
    // Imagenes de los dados del jugador y la IA
    [Header("Dados")]
    [SerializeField] private Image imagenJugador;
    [SerializeField] private Image imagenIA;
    [SerializeField] private Sprite[] spritesJugador; // Las 6 caras del dado del jugador
    [SerializeField] private Sprite[] spritesIA;       // Las 6 caras del dado de la IA

    // Elementos de la interfaz
    [Header("UI")]
    [SerializeField] private Button botonTirar;
    [SerializeField] private TextMeshProUGUI textoJugador;
    [SerializeField] private TextMeshProUGUI textoIA;
    [SerializeField] private TextMeshProUGUI textoMarcador;
    [SerializeField] private TextMeshProUGUI textoDineroJugador;
    [SerializeField] private TextMeshProUGUI textoDineroIA;
    [SerializeField] private TextMeshProUGUI textoApuestaActual;
    [SerializeField] private TextMeshProUGUI textoApuestaIA;
    [SerializeField] private TextMeshProUGUI textoMensaje;
    [SerializeField] private GameObject panelResultado;
    [SerializeField] private TextMeshProUGUI textoFinal;

    // Botones de apuesta
    [Header("Apuestas")]
    [SerializeField] private Button botonApuesta5;
    [SerializeField] private Button botonApuesta10;
    [SerializeField] private Button botonApuesta20;

    // Configuracion
    [Header("Config")]
    [SerializeField] private float duracionAnimacion = 2f;
    [SerializeField] private int dineroInicialJugador = 70;
    [SerializeField] private int dineroInicialIA = 70;

    // Estado interno
    private int puntosJugador = 0;   // Rondas ganadas por el jugador
    private int puntosIA = 0;        // Rondas ganadas por Davy Jones
    private int ronda = 0;
    private int maxRondas = 7;
    private bool juegoActivo = true;
    private int apuestaJugador = 0;
    private int apuestaIA = 0;
    private int dineroJugador = 70;
    private int dineroIA = 70;

    // Inicializa la UI y espera la cuenta atras
    void Start()
    {
        dineroJugador = dineroInicialJugador;
        dineroIA = dineroInicialIA;

        panelResultado?.SetActive(false);
        botonTirar.interactable = false;
        MostrarBotonesApuesta(true);
        textoJugador?.SetText("?");
        textoIA?.SetText("?");
        ActualizarMarcador();
        ActualizarDineros();

        botonTirar?.onClick.AddListener(TirarDados);
        botonApuesta5?.onClick.AddListener(() => SeleccionarApuesta(5));
        botonApuesta10?.onClick.AddListener(() => SeleccionarApuesta(10));
        botonApuesta20?.onClick.AddListener(() => SeleccionarApuesta(20));

        // Esperar cuenta atras 3-2-1
        CuentaAtras ca = FindObjectOfType<CuentaAtras>();
        if (ca != null)
        {
            MostrarBotonesApuesta(false);
            botonTirar.interactable = false;
            ca.OnFinCuentaAtras += () => MostrarBotonesApuesta(true);
        }
    }

    // Muestra un mensaje en la UI
    private void MostrarMensaje(string mensaje)
    {
        if (textoMensaje != null)
            textoMensaje.text = mensaje;
    }

    // Muestra u oculta botones de apuesta segun el dinero del jugador
    private void MostrarBotonesApuesta(bool mostrar)
    {
        botonApuesta5?.gameObject.SetActive(mostrar && dineroJugador >= 5);
        botonApuesta10?.gameObject.SetActive(mostrar && dineroJugador >= 10);
        botonApuesta20?.gameObject.SetActive(mostrar && dineroJugador >= 20);
    }

    // El jugador elige apuesta, la IA elige una aleatoria entre las que puede pagar
    private void SeleccionarApuesta(int cantidad)
    {
        if (dineroJugador < cantidad)
        {
            MostrarMensaje("No tienes suficiente reales.");
            return;
        }

        apuestaJugador = cantidad;

        // La IA elige aleatoriamente entre las apuestas que puede pagar
        int[] opciones = { 5, 10, 20 };
        List<int> posibles = new List<int>();
        foreach (int op in opciones)
            if (dineroIA >= op) posibles.Add(op);

        apuestaIA = posibles.Count > 0 ? posibles[Random.Range(0, posibles.Count)] : 0;

        textoApuestaActual?.SetText("Tu apuesta: " + apuestaJugador + " reales");
        textoApuestaIA?.SetText("Davy Jones apuesta: " + apuestaIA + " reales");

        botonTirar.interactable = true;
        MostrarBotonesApuesta(false);
    }

    // Lanza los dados si hay apuesta seleccionada
    private void TirarDados()
    {
        if (!juegoActivo || apuestaJugador == 0) return;
        botonTirar.interactable = false;
        StartCoroutine(AnimarDados());
    }

    // Anima los dados, genera resultado sin empates, reparte el bote
    // y comprueba si alguien se quedo sin dinero o se alcanzo el maximo de rondas
    private IEnumerator AnimarDados()
    {
        MostrarMensaje("");

        // Animacion: cambiar sprites rapidamente
        float elapsed = 0f;
        while (elapsed < duracionAnimacion)
        {
            if (imagenJugador != null && spritesJugador.Length > 0)
                imagenJugador.sprite = spritesJugador[Random.Range(0, spritesJugador.Length)];
            if (imagenIA != null && spritesIA.Length > 0)
                imagenIA.sprite = spritesIA[Random.Range(0, spritesIA.Length)];
            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        // Resultado sin empates
        int resultadoJugador, resultadoIA;
        do
        {
            resultadoJugador = Random.Range(1, 7);
            resultadoIA = Random.Range(1, 7);
        } while (resultadoJugador == resultadoIA);

        // Mostrar resultado final en los dados
        if (imagenJugador != null && spritesJugador.Length >= resultadoJugador)
            imagenJugador.sprite = spritesJugador[resultadoJugador - 1];
        if (imagenIA != null && spritesIA.Length >= resultadoIA)
            imagenIA.sprite = spritesIA[resultadoIA - 1];

        textoJugador?.SetText(resultadoJugador.ToString());
        textoIA?.SetText(resultadoIA.ToString());

        ronda++;
        int bote = apuestaJugador + apuestaIA;

        // Repartir bote al ganador de la ronda
        if (resultadoJugador > resultadoIA)
        {
            puntosJugador++;
            dineroJugador += bote;
            dineroIA -= apuestaIA;
            MostrarMensaje("Ganaste esta ronda. +" + bote + " reales");
        }
        else
        {
            puntosIA++;
            dineroIA += bote;
            dineroJugador -= apuestaJugador;
            MostrarMensaje("Davy Jones gana esta ronda. -" + apuestaJugador + " reales");
        }

        // Resetear apuestas
        apuestaJugador = 0;
        apuestaIA = 0;
        textoApuestaActual?.SetText("");
        textoApuestaIA?.SetText("");

        ActualizarMarcador();
        ActualizarDineros();

        yield return new WaitForSeconds(1f);

        // El jugador se quedo sin dinero → derrota
        if (dineroJugador <= 0)
        {
            OcultarUI();
            GameManager.Instance?.GastarDinero(dineroInicialJugador);
            textoFinal?.SetText("Te quedaste sin reales grumete de agua dulce.\nDavy Jones se queda con tu corazon.");
            panelResultado?.SetActive(true);
            juegoActivo = false;
            GestorBBDD.Instance?.RegistrarPartidaMinijuego("Dados Davy Jones", puntosJugador, false);
            yield break;
        }

        // Davy Jones se quedo sin dinero → victoria
        if (dineroIA <= 0)
        {
            OcultarUI();
            int ganado = dineroJugador - dineroInicialJugador;
            GameManager.Instance?.AnadirDinero(ganado);
            GestorMinijuegos.Instance?.MinijuegoCompletado();
            GestorMinijuegos.Instance?.MinijuegoGanado();
            textoFinal?.SetText("Davy Jones se quedo sin dinero.\nTe quedas con su corazon. +" + ganado + " reales");
            panelResultado?.SetActive(true);
            juegoActivo = false;
            GestorBBDD.Instance?.RegistrarPartidaMinijuego("Dados Davy Jones", puntosJugador, puntosJugador > puntosIA);
            yield break;
        }

        // Comprobar si se alcanzo el maximo de rondas o 4 victorias
        if (puntosJugador >= 4 || puntosIA >= 4 || ronda >= maxRondas)
            TerminarJuego();
        else
            MostrarBotonesApuesta(true);
    }

    // Actualiza el marcador de rondas
    private void ActualizarMarcador()
    {
        textoMarcador?.SetText("Davy Jones " + puntosIA + " - " + puntosJugador + " Tu  |  Ronda " + ronda + "/" + maxRondas);
    }

    // Actualiza los textos de dinero
    private void ActualizarDineros()
    {
        textoDineroJugador?.SetText("Tu dinero: " + dineroJugador + " reales");
        textoDineroIA?.SetText("Davy Jones: " + dineroIA + " reales");
    }

    // Oculta toda la UI del juego para mostrar solo el resultado
    private void OcultarUI()
    {
        textoDineroJugador?.gameObject.SetActive(false);
        textoDineroIA?.gameObject.SetActive(false);
        textoMarcador?.gameObject.SetActive(false);
        textoJugador?.gameObject.SetActive(false);
        textoIA?.gameObject.SetActive(false);
        textoApuestaActual?.gameObject.SetActive(false);
        textoApuestaIA?.gameObject.SetActive(false);
        imagenJugador?.gameObject.SetActive(false);
        imagenIA?.gameObject.SetActive(false);
        botonTirar?.gameObject.SetActive(false);
        textoMensaje?.gameObject.SetActive(false);
    }

    // Termina el juego por rondas o por 4 victorias
    // Calcula ganancias/perdidas y notifica al GestorMinijuegos si gano
    private void TerminarJuego()
    {
        juegoActivo = false;
        OcultarUI();
        panelResultado?.SetActive(true);

        int diferencia = dineroJugador - dineroInicialJugador;

        if (puntosJugador > puntosIA)
        {
            // Victoria: sumar ganancias y avanzar en el flujo
            if (diferencia > 0) GameManager.Instance?.AnadirDinero(diferencia);
            GestorMinijuegos.Instance?.MinijuegoCompletado();
            GestorMinijuegos.Instance?.MinijuegoGanado();
            textoFinal?.SetText("Venciste a Davy Jones.\nTe quedas con su corazon. +" + Mathf.Max(0, diferencia) + " monedas");
        }
        else
        {
            // Derrota: restar perdidas
            if (diferencia < 0) GameManager.Instance?.GastarDinero(Mathf.Abs(diferencia));
            textoFinal?.SetText("Davy Jones te ha vencido.\nSe queda con tu corazon. -" + Mathf.Abs(Mathf.Min(0, diferencia)) + " monedas");
        }
        GestorBBDD.Instance?.RegistrarPartidaMinijuego("Dados Davy Jones", puntosJugador, puntosJugador > puntosIA);
    }

    // Vuelve a la cafeteria
    public void Volver()
    {
        SceneManager.LoadScene("CafeScene");
    }
}