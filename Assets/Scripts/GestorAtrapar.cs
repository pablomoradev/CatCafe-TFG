using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

// Gestiona el minijuego Atrapar Productos
// Caen productos desde arriba y el jugador los atrapa con una bandeja
// Easter egg: si atrapa un cigarro, le restan puntos
public class GestorAtrapar : MonoBehaviour
{
    // Productos que caen
    [Header("Productos")]
    [SerializeField] private GameObject prefabProducto;
    [SerializeField] private Sprite[] spritesProductos;   // Sprites de productos buenos
    [SerializeField] private Transform contenedorProductos;

    // Bandeja controlada por el jugador
    [Header("Bandeja")]
    [SerializeField] private RectTransform bandeja;
    [SerializeField] private float velocidadBandeja = 500f;

    // Easter egg del cigarro
    [Header("Easter Egg")]
    [SerializeField] private Sprite spriteEasterEgg;       // Sprite del cigarro
    [SerializeField] private int penalizacionEasterEgg = 15;
    [SerializeField] private float probabilidadEasterEgg = 0.15f;  // 15% de probabilidad

    // Configuracion del minijuego
    [Header("Config")]
    [SerializeField] private float tiempoLimite = 45f;
    [SerializeField] private float velocidadCaida = 200f;
    [SerializeField] private float intervaloSpawn = 1.5f;
    [SerializeField] private int recompensaPorProducto = 5;

    // Elementos de la UI
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI textoTiempo;
    [SerializeField] private TextMeshProUGUI textoPuntuacion;
    [SerializeField] private GameObject panelResultado;
    [SerializeField] private TextMeshProUGUI textoResultado;

    // Estado interno
    private float tiempoActual;
    private int puntuacion = 0;
    private bool juegoActivo = false;
    private float limiteIzquierdo;
    private float limiteDerecho;
    private bool haGanado = false;

    // Espera la cuenta atras antes de empezar
    void Start()
    {
        tiempoActual = tiempoLimite;
        panelResultado?.SetActive(false);
        juegoActivo = false;

        CuentaAtras ca = FindObjectOfType<CuentaAtras>();
        if (ca != null)
            ca.OnFinCuentaAtras += IniciarJuego;
        else
            IniciarJuego();
    }

    // Calcula los limites de la bandeja y empieza a spawnear productos
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

    // Mueve la bandeja con flechas o A/D y resta tiempo
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

    // Genera productos cada intervaloSpawn segundos
    private IEnumerator SpawnProductos()
    {
        yield return new WaitForSeconds(1f);
        while (juegoActivo)
        {
            SpawnProducto();
            yield return new WaitForSeconds(intervaloSpawn);
        }
    }

    // Instancia un producto en posicion aleatoria (puede ser el easter egg)
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

        // Decidir si es un producto bueno o el easter egg
        bool esMalo = Random.value < probabilidadEasterEgg && spriteEasterEgg != null;
        Image img = prod.GetComponent<Image>();
        if (img != null)
            img.sprite = esMalo ? spriteEasterEgg : spritesProductos[Random.Range(0, spritesProductos.Length)];

        ProductoCayendo pc = prod.GetComponent<ProductoCayendo>();
        if (pc == null) pc = prod.AddComponent<ProductoCayendo>();
        pc.Inicializar(velocidadCaida, bandeja, alturaCanvas, this, esMalo, penalizacionEasterEgg);
    }

    // Llamado por ProductoCayendo al atrapar un producto bueno
    public void ProductoAtrapado()
    {
        puntuacion += recompensaPorProducto;
        if (textoPuntuacion != null)
            textoPuntuacion.text = "Puntos: " + puntuacion;
    }

    // Llamado por ProductoCayendo al atrapar el easter egg (cigarro)
    public void ProductoMalo(int penalizacion)
    {
        puntuacion = Mathf.Max(0, puntuacion - penalizacion);
        if (textoPuntuacion != null)
            textoPuntuacion.text = "Puntos: " + puntuacion;
    }

    // Termina el juego al acabarse el tiempo
    private void TerminarJuego()
    {
        juegoActivo = false;
        panelResultado?.SetActive(true);
        haGanado = true;
        GestorMinijuegos.Instance?.MinijuegoCompletado();
        GestorMinijuegos.Instance?.MinijuegoGanado();
        if (textoResultado != null)
            textoResultado.text = "¡Tiempo!\n+" + puntuacion + " puntos";
        GestorBBDD.Instance?.RegistrarPartidaMinijuego("Atrapar Productos", puntuacion, true);
    }

    // Guarda monedas pendientes y vuelve a la cafeteria
    public void Volver()
    {
        if (haGanado)
        {
            int pendientes = PlayerPrefs.GetInt("Monedas_Pendientes", 0);
            PlayerPrefs.SetInt("Monedas_Pendientes", pendientes + puntuacion);
            PlayerPrefs.Save();
        }
        SceneManager.LoadScene("CafeScene");
    }
}