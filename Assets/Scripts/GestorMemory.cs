using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GestorMemory : MonoBehaviour
{
    [Header("Cartas")]
    [SerializeField] private GameObject prefabCarta;
    [SerializeField] private Transform contenedorCartas;
    [SerializeField] private Sprite[] iconosProductos; // arrastra los sprites de cafe, latte, etc
    [SerializeField] private Sprite reversoCarta;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI textoTiempo;
    [SerializeField] private TextMeshProUGUI textoPareja;
    [SerializeField] private GameObject panelResultado;
    [SerializeField] private TextMeshProUGUI textoResultado;
    [SerializeField] private Button botonVolver;

    [Header("Config")]
    [SerializeField] private float tiempoLimite = 60f;
    [SerializeField] private int recompensaGanar = 30;

    private List<CartaMemory> cartasVolteadas = new List<CartaMemory>();
    private int parejasEncontradas = 0;
    private int totalParejas;
    private float tiempoActual;
    private bool juegoActivo = false;
    private bool comprobando = false;

    void Start()
    {
        tiempoActual = tiempoLimite;
        GenerarTablero();
        panelResultado?.SetActive(false);
        botonVolver?.onClick.AddListener(Volver);
    }

    void Update()
    {
        if (!juegoActivo) return;
        tiempoActual -= Time.deltaTime;
        if (textoTiempo != null)
            textoTiempo.text = "⏱ " + Mathf.CeilToInt(tiempoActual);
        if (tiempoActual <= 0) TerminarJuego(false);
    }

    private void GenerarTablero()
    {
        // Crear pares de iconos
        List<Sprite> sprites = new List<Sprite>();
        foreach (Sprite s in iconosProductos)
        {
            sprites.Add(s);
            sprites.Add(s);
        }

        // Mezclar
        for (int i = sprites.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Sprite temp = sprites[i];
            sprites[i] = sprites[j];
            sprites[j] = temp;
        }

        totalParejas = iconosProductos.Length;

        // Instanciar cartas
        for (int i = 0; i < sprites.Count; i++)
        {
            GameObject carta = Instantiate(prefabCarta, contenedorCartas);
            CartaMemory cm = carta.GetComponent<CartaMemory>();
            cm.Inicializar(sprites[i], reversoCarta, this);
        }

        juegoActivo = true;
    }

    public void CartaSeleccionada(CartaMemory carta)
    {
        if (comprobando) return;
        if (cartasVolteadas.Contains(carta)) return;
        if (cartasVolteadas.Count >= 2) return;

        carta.Voltear(true);
        cartasVolteadas.Add(carta);

        if (cartasVolteadas.Count == 2)
            StartCoroutine(ComprobarPareja());
    }

    private IEnumerator ComprobarPareja()
    {
        comprobando = true;
        yield return new WaitForSeconds(1f);

        if (cartasVolteadas[0].Icono == cartasVolteadas[1].Icono)
        {
            // Pareja encontrada
            cartasVolteadas[0].Emparejar();
            cartasVolteadas[1].Emparejar();
            parejasEncontradas++;

            if (textoPareja != null)
                textoPareja.text = "Parejas: " + parejasEncontradas + "/" + totalParejas;

            if (parejasEncontradas >= totalParejas)
                TerminarJuego(true);
        }
        else
        {
            cartasVolteadas[0].Voltear(false);
            cartasVolteadas[1].Voltear(false);
        }

        cartasVolteadas.Clear();
        comprobando = false;
    }

    private void TerminarJuego(bool gano)
    {
        juegoActivo = false;
        panelResultado?.SetActive(true);

        if (gano)
        {
            GameManager.Instance?.AnadirDinero(recompensaGanar);
            if (textoResultado != null)
                textoResultado.text = "¡Has ganado!\n+" + recompensaGanar + " monedas";
        }
        else
        {
            if (textoResultado != null)
                textoResultado.text = "¡Se acabó el tiempo!";
        }
    }

    private void Volver()
    {
        SceneManager.LoadScene("CafeScene");
    }
}