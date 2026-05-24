using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GestorAtrapar : MonoBehaviour
{
    [Header("Productos")]
    [SerializeField] private GameObject prefabProducto;
    [SerializeField] private Sprite[] spritesProductos;
    [SerializeField] private Transform contenedorProductos;

    [Header("Bandeja")]
    [SerializeField] private RectTransform bandeja;
    [SerializeField] private float velocidadBandeja = 500f;

    [Header("Easter Egg")]
    [SerializeField] private Sprite spriteEasterEgg;
    [SerializeField] private int penalizacionEasterEgg = 15;
    [SerializeField] private float probabilidadEasterEgg = 0.15f;

    [Header("Config")]
    [SerializeField] private float tiempoLimite = 45f;
    [SerializeField] private float velocidadCaida = 200f;
    [SerializeField] private float intervaloSpawn = 1.5f;
    [SerializeField] private int recompensaPorProducto = 5;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI textoTiempo;
    [SerializeField] private TextMeshProUGUI textoPuntuacion;
    [SerializeField] private GameObject panelResultado;
    [SerializeField] private TextMeshProUGUI textoResultado;

    private float tiempoActual;
    private int puntuacion = 0;
    private bool juegoActivo = false;
    private float limiteIzquierdo;
    private float limiteDerecho;

    void Start()
    {
        tiempoActual = tiempoLimite;
        panelResultado?.SetActive(false);
        juegoActivo = false; // empieza inactivo

        CuentaAtras ca = FindObjectOfType<CuentaAtras>();
        if (ca != null)
            ca.OnFinCuentaAtras += IniciarJuego;
        else
            IniciarJuego();
    }

    private void IniciarJuego()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            RectTransform rt = canvas.GetComponent<RectTransform>();
            limiteIzquierdo = -rt.rect.width / 2 + bandeja.rect.width / 2;
            limiteDerecho = rt.rect.width / 2 - bandeja.rect.width / 2;
        }
        StartCoroutine(SpawnProductos());
        juegoActivo = true;
    }

    void Update()
    {
        if (!juegoActivo) return;

        float input = 0f;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) input = -1f;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) input = 1f;

        Vector2 pos = bandeja.anchoredPosition;
        pos.x += input * velocidadBandeja * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, limiteIzquierdo, limiteDerecho);
        bandeja.anchoredPosition = pos;

        tiempoActual -= Time.deltaTime;
        if (textoTiempo != null)
            textoTiempo.text = "Tiempo: " + Mathf.CeilToInt(tiempoActual);
        if (tiempoActual <= 0) TerminarJuego();
    }

    private IEnumerator SpawnProductos()
    {
        yield return new WaitForSeconds(1f);
        while (juegoActivo)
        {
            SpawnProducto();
            yield return new WaitForSeconds(intervaloSpawn);
        }
    }

    private void SpawnProducto()
    {
        if (prefabProducto == null || spritesProductos.Length == 0) return;

        GameObject prod = Instantiate(prefabProducto, contenedorProductos);
        RectTransform rt = prod.GetComponent<RectTransform>();

        Canvas canvas = FindObjectOfType<Canvas>();
        float anchoCanvas = canvas != null ? canvas.GetComponent<RectTransform>().rect.width : 800f;
        float alturaCanvas = canvas != null ? canvas.GetComponent<RectTransform>().rect.height : 600f;

        rt.anchoredPosition = new Vector2(
            Random.Range(-anchoCanvas / 2 + 50, anchoCanvas / 2 - 50),
            alturaCanvas / 2 + 50
        );

        bool esMalo = Random.value < probabilidadEasterEgg && spriteEasterEgg != null;
        Image img = prod.GetComponent<Image>();
        if (img != null)
            img.sprite = esMalo ? spriteEasterEgg : spritesProductos[Random.Range(0, spritesProductos.Length)];

        ProductoCayendo pc = prod.GetComponent<ProductoCayendo>();
        if (pc == null) pc = prod.AddComponent<ProductoCayendo>();
        pc.Inicializar(velocidadCaida, bandeja, alturaCanvas, this, esMalo, penalizacionEasterEgg);
    }

    public void ProductoAtrapado()
    {
        puntuacion += recompensaPorProducto;
        if (textoPuntuacion != null)
            textoPuntuacion.text = "Puntos: " + puntuacion;
    }

    public void ProductoMalo(int penalizacion)
    {
        puntuacion = Mathf.Max(0, puntuacion - penalizacion);
        if (textoPuntuacion != null)
            textoPuntuacion.text = "Puntos: " + puntuacion;
        GestorDialogos.Instance?.MostrarDialogo("¡Eso no se sirve en un café!");
    }

    private void TerminarJuego()
    {
        juegoActivo = false;
        panelResultado?.SetActive(true);
        GestorMinijuegos.Instance?.MinijuegoCompletado(); 
        GestorMinijuegos.Instance?.MinijuegoGanado();     
        GameManager.Instance?.AnadirDinero(puntuacion);
        if (textoResultado != null)
            textoResultado.text = "¡Tiempo!\n+" + puntuacion + " monedas";
        GestorBBDD.Instance?.RegistrarPartidaMinijuego("Atrapar Productos", puntuacion);
    }

    public void Volver()
    {
        SceneManager.LoadScene("CafeScene");
    }
}