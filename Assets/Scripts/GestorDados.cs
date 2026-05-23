using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GestorDados : MonoBehaviour
{
    [Header("Dados")]
    [SerializeField] private Image imagenJugador;
    [SerializeField] private Image imagenIA;
    [SerializeField] private Sprite[] spritesJugador;
    [SerializeField] private Sprite[] spritesIA;

    [Header("UI")]
    [SerializeField] private Button botonTirar;
    [SerializeField] private TextMeshProUGUI textoJugador;
    [SerializeField] private TextMeshProUGUI textoIA;
    [SerializeField] private TextMeshProUGUI textoMarcador;
    [SerializeField] private TextMeshProUGUI textoDineroJugador;
    [SerializeField] private TextMeshProUGUI textoDineroIA;
    [SerializeField] private TextMeshProUGUI textoApuestaActual;
    [SerializeField] private TextMeshProUGUI textoApuestaIA;
    [SerializeField] private GameObject panelResultado;
    [SerializeField] private TextMeshProUGUI textoFinal;

    [Header("Apuestas")]
    [SerializeField] private Button botonApuesta5;
    [SerializeField] private Button botonApuesta10;
    [SerializeField] private Button botonApuesta20;

    [Header("Config")]
    [SerializeField] private float duracionAnimacion = 2f;
    [SerializeField] private int dineroInicialJugador = 70;
    [SerializeField] private int dineroInicialIA = 70;

    private int puntosJugador = 0;
    private int puntosIA = 0;
    private int ronda = 0;
    private int maxRondas = 7;
    private bool juegoActivo = true;
    private int apuestaJugador = 0;
    private int apuestaIA = 0;
    private int dineroJugador = 70;
    private int dineroIA = 70;

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

        CuentaAtras ca = FindObjectOfType<CuentaAtras>();
        if (ca != null)
        {
            MostrarBotonesApuesta(false);
            botonTirar.interactable = false;
            ca.OnFinCuentaAtras += () => MostrarBotonesApuesta(true);
        }
    }

    private void MostrarBotonesApuesta(bool mostrar)
    {
        botonApuesta5?.gameObject.SetActive(mostrar && dineroJugador >= 5);
        botonApuesta10?.gameObject.SetActive(mostrar && dineroJugador >= 10);
        botonApuesta20?.gameObject.SetActive(mostrar && dineroJugador >= 20);
    }

    private void SeleccionarApuesta(int cantidad)
    {
        if (dineroJugador < cantidad)
        {
            GestorDialogos.Instance?.MostrarDialogo("No tienes suficiente reales.");
            return;
        }

        apuestaJugador = cantidad;

        int[] opciones = { 5, 10, 20 };
        System.Collections.Generic.List<int> posibles = new System.Collections.Generic.List<int>();
        foreach (int op in opciones)
            if (dineroIA >= op) posibles.Add(op);

        apuestaIA = posibles.Count > 0 ? posibles[Random.Range(0, posibles.Count)] : 0;

        textoApuestaActual?.SetText("Tu apuesta: " + apuestaJugador + " reales");
        textoApuestaIA?.SetText("Davy Jones apuesta: " + apuestaIA + " reales");

        botonTirar.interactable = true;
        MostrarBotonesApuesta(false);
    }

    private void TirarDados()
    {
        if (!juegoActivo || apuestaJugador == 0) return;
        botonTirar.interactable = false;
        StartCoroutine(AnimarDados());
    }

    private IEnumerator AnimarDados()
    {
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

        // Sin empates: repetir hasta que haya ganador
        int resultadoJugador;
        int resultadoIA;
        do
        {
            resultadoJugador = Random.Range(1, 7);
            resultadoIA = Random.Range(1, 7);
        } while (resultadoJugador == resultadoIA);

        if (imagenJugador != null && spritesJugador.Length >= resultadoJugador)
            imagenJugador.sprite = spritesJugador[resultadoJugador - 1];
        if (imagenIA != null && spritesIA.Length >= resultadoIA)
            imagenIA.sprite = spritesIA[resultadoIA - 1];

        textoJugador?.SetText(resultadoJugador.ToString());
        textoIA?.SetText(resultadoIA.ToString());

        ronda++;

        int bote = apuestaJugador + apuestaIA;

        if (resultadoJugador > resultadoIA)
        {
            puntosJugador++;
            dineroJugador += bote;
            dineroIA -= apuestaIA;
            GestorDialogos.Instance?.MostrarDialogo("Ganaste esta ronda. +" + bote + " reales");
        }
        else
        {
            puntosIA++;
            dineroIA += bote;
            dineroJugador -= apuestaJugador;
            GestorDialogos.Instance?.MostrarDialogo("Davy Jones gana esta ronda. -" + apuestaJugador + " reales");
        }

        apuestaJugador = 0;
        apuestaIA = 0;
        textoApuestaActual?.SetText("");
        textoApuestaIA?.SetText("");

        ActualizarMarcador();
        ActualizarDineros();

        yield return new WaitForSeconds(1f);

        if (dineroJugador <= 0)
        {
            OcultarUI();
            GameManager.Instance?.GastarDinero(dineroInicialJugador);
            textoFinal?.SetText("Te quedaste sin reales grumete de agua dulce.\nDavy Jones se queda con tu corazon.");
            panelResultado?.SetActive(true);
            juegoActivo = false;
            yield break;
        }

        if (dineroIA <= 0)
        {
            OcultarUI();
            int ganado = dineroJugador - dineroInicialJugador;
            GameManager.Instance?.AnadirDinero(ganado);
            textoFinal?.SetText("Davy Jones se quedo sin dinero.\nTe quedas con su corazon. +" + ganado + " reales");
            panelResultado?.SetActive(true);
            juegoActivo = false;
            yield break;
        }

        if (puntosJugador >= 4 || puntosIA >= 4 || ronda >= maxRondas)
            TerminarJuego();
        else
            MostrarBotonesApuesta(true);
    }

    private void ActualizarMarcador()
    {
        textoMarcador?.SetText("Davy Jones " + puntosIA + " - " + puntosJugador + " Tu  |  Ronda " + ronda + "/" + maxRondas);
    }

    private void ActualizarDineros()
    {
        textoDineroJugador?.SetText("Tu dinero: " + dineroJugador + " reales");
        textoDineroIA?.SetText("Davy Jones: " + dineroIA + " reales");
    }

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
    }

    private void TerminarJuego()
    {
        juegoActivo = false;
        OcultarUI();
        panelResultado?.SetActive(true);

        int diferencia = dineroJugador - dineroInicialJugador;

        if (puntosJugador > puntosIA)
        {
            if (diferencia > 0) GameManager.Instance?.AnadirDinero(diferencia);
            textoFinal?.SetText("Venciste a Davy Jones.\nTe quedas con tu corazon. +" + Mathf.Max(0, diferencia) + " monedas");
        }
        else
        {
            if (diferencia < 0) GameManager.Instance?.GastarDinero(Mathf.Abs(diferencia));
            textoFinal?.SetText("Davy Jones te ha vencido.\nSe queda con tu corazon. -" + Mathf.Abs(Mathf.Min(0, diferencia)) + " monedas");
        }
    }

    public void Volver()
    {
        SceneManager.LoadScene("CafeScene");
    }
}