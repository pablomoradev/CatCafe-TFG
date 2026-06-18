using UnityEngine;

// Controla el comportamiento del gato rescatado en la cafeteria
// Se mueve por la escena, reacciona a caricias, tiene barra de felicidad
// y aporta un bonus pasivo basado en su nivel de felicidad
public class GatoSalvado : MonoBehaviour
{
    // Movimiento del gato por la cafeteria
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 0.3f;

    [Header("Sprite")]
    [SerializeField] private bool invertirDireccion = false;

    // Limites de movimiento horizontal
    [Header("Límites de la cafetería")]
    [SerializeField] private float limiteXMin = -7f;
    [SerializeField] private float limiteXMax = 7f;
    [SerializeField] private float posicionY = -1f;

    // ScriptableObject con los datos del gato
    [Header("Datos del gato")]
    public DatosGato datosGato;

    // Sistema de caricias y easter egg de enfado
    [Header("Caricias")]
    [SerializeField] private int cariciasParaEnfadar = 5;     // Caricias seguidas para enfadarse
    [SerializeField] private float tiempoResetCaricias = 3f;  // Segundos para resetear el contador
    [SerializeField] private float duracionReaccion = 1.5f;

    // Barra de felicidad que aparece al acariciar
    [Header("Barra felicidad")]
    [SerializeField] private GameObject prefabBarraFelicidad;
    [SerializeField] private float felicidadMaxima = 1f;
    [SerializeField] private float velocidadBajarFelicidad = 0.05f;

    // Corazon que aparece al acariciar
    [Header("Corazon flotante")]
    [SerializeField] private GameObject prefabCorazon;

    // Estado interno
    private Animator anim;
    private SpriteRenderer sr;
    private bool estaAndando;
    private float direccion = 1f;
    private int contadorCaricias = 0;
    private float timerResetCaricias = 0f;
    private bool estaReaccionando = false;
    private float timerReaccion = 0f;
    private float felicidadActual = 0f;
    private float timerOcultarBarra = 0f;
    private float tiempoOcultarBarra = 3f;
    private UnityEngine.UI.Slider barraFelicidad;

    private void Start()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        // Instanciar la barra de felicidad (oculta por defecto)
        if (prefabBarraFelicidad != null)
        {
            GameObject barra = Instantiate(prefabBarraFelicidad);
            barraFelicidad = barra.GetComponent<UnityEngine.UI.Slider>();
            barraFelicidad.gameObject.SetActive(false);
        }

        // Empezar con movimiento aleatorio
        estaAndando = Random.value > 0.5f;
        anim.SetBool("Andando", estaAndando);

        // Posicion inicial aleatoria dentro de los limites
        float x = Random.Range(limiteXMin, limiteXMax);
        transform.position = new Vector3(x, posicionY, 11f);
        direccion = Random.value > 0.5f ? 1f : -1f;

        // Registrarse como gato activo para calcular bonus
        GameManager.Instance?.RegistrarGatoActivo(this);
    }

    private void Update()
    {
        // Resetear contador de caricias tras un tiempo sin acariciar
        if (contadorCaricias > 0)
        {
            timerResetCaricias -= Time.deltaTime;
            if (timerResetCaricias <= 0)
                contadorCaricias = 0;
        }

        // Controlar duracion de la reaccion (contento/enfadado)
        if (estaReaccionando)
        {
            timerReaccion -= Time.deltaTime;
            if (timerReaccion <= 0)
                estaReaccionando = false;
        }

        // La felicidad baja poco a poco con el tiempo
        if (felicidadActual > 0)
        {
            felicidadActual -= velocidadBajarFelicidad * Time.deltaTime;
            felicidadActual = Mathf.Clamp(felicidadActual, 0f, felicidadMaxima);
            if (barraFelicidad != null)
                barraFelicidad.value = felicidadActual / felicidadMaxima;
        }

        // Ocultar la barra de felicidad despues de un tiempo
        if (timerOcultarBarra > 0)
        {
            timerOcultarBarra -= Time.deltaTime;
            if (timerOcultarBarra <= 0 && barraFelicidad != null)
                barraFelicidad.gameObject.SetActive(false);
        }

        // Posicionar la barra encima del gato
        if (barraFelicidad != null && barraFelicidad.gameObject.activeSelf)
            barraFelicidad.transform.position = transform.position + new Vector3(0, 0.5f, 0);

        if (!estaAndando || estaReaccionando) return;

        // Mover el gato horizontalmente
        transform.Translate(Vector2.right * direccion * velocidad * Time.deltaTime);

        // Voltear el sprite segun la direccion
        if (sr != null)
            sr.flipX = invertirDireccion ? direccion < 0 : direccion > 0;

        // Cambiar de direccion si llega al limite
        Vector3 pos = transform.position;
        if (pos.x >= limiteXMax || pos.x <= limiteXMin)
        {
            direccion = -direccion;
            pos.x = Mathf.Clamp(pos.x, limiteXMin, limiteXMax);
            transform.position = pos;
        }
    }

    // Al hacer clic en el gato: sube felicidad, muestra corazon, cuenta caricias
    // Si se superan las caricias maximas, se enfada (easter egg)
    private void OnMouseDown()
    {
        felicidadActual = Mathf.Clamp(felicidadActual + 0.2f, 0f, felicidadMaxima);
        timerOcultarBarra = tiempoOcultarBarra;

        if (barraFelicidad != null)
            barraFelicidad.gameObject.SetActive(true);

        // Instanciar corazon flotante
        if (prefabCorazon != null)
        {
            Vector3 posCorazon = new Vector3(
                transform.position.x + Random.Range(-0.3f, 0.3f),
                transform.position.y + 1.5f, 0f);
            GameObject corazon = Instantiate(prefabCorazon);
            corazon.transform.position = posCorazon;
        }

        contadorCaricias++;
        timerResetCaricias = tiempoResetCaricias;
        estaReaccionando = true;
        timerReaccion = duracionReaccion;

        // Easter egg: si acaricias demasiadas veces seguidas, se enfada
        if (contadorCaricias >= cariciasParaEnfadar)
        {
            contadorCaricias = 0;
            anim.SetTrigger("Enfadado");
        }
        else
        {
            anim.SetTrigger("Contento");
        }
    }

    // Devuelve un multiplicador basado en la felicidad actual (0 a 1)
    // Usado por GameManager para calcular el bonus en los pedidos
    public float ObtenerMultiplicadorBonus()
    {
        return felicidadActual / felicidadMaxima;
    }
}