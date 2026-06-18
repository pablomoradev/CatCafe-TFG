using UnityEngine;

// Comportamiento de un ratón en el minijuego de caza.
public class RatonMinijuego : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float tiempoVisible = 1.5f;   // Tiempo que permanece activo.
    [SerializeField] private float velocidad = 3f;         // Velocidad de movimiento.
    [SerializeField] private float tiempoCambioDir = 0.8f; // Frecuencia de cambio de dirección.

    [Header("Límites de movimiento")]
    [SerializeField] private float limiteX = 7f;
    [SerializeField] private float limiteY = 3.5f;

    private Animator animador;        // Control de animaciones.
    private SpriteRenderer sr;        // Para voltear sprite.
    private bool atrapado = false;    // Estado de captura.

    private float timerVisible;       // Tiempo restante visible.
    private float timerCambioDir;     // Timer para cambiar dirección.
    private Vector2 direccion;        // Dirección de movimiento.

    void Awake()
    {
        animador = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        atrapado = false;
        timerVisible = tiempoVisible;
        timerCambioDir = 0f;

        CambiarDireccion();
        animador?.Play("RatonIdle");
    }

    void Update()
    {
        if (atrapado) return;

        timerVisible -= Time.deltaTime;

        // Si se acaba el tiempo, el ratón se esconde.
        if (timerVisible <= 0)
        {
            Esconderse();
            return;
        }

        timerCambioDir -= Time.deltaTime;

        if (timerCambioDir <= 0)
            CambiarDireccion();

        // Movimiento.
        transform.Translate(direccion * velocidad * Time.deltaTime);

        // Límites del escenario.
        Vector3 pos = transform.position;

        if (pos.x > limiteX || pos.x < -limiteX)
        {
            direccion.x = -direccion.x;
            pos.x = Mathf.Clamp(pos.x, -limiteX, limiteX);
        }

        if (pos.y > limiteY || pos.y < -limiteY)
        {
            direccion.y = -direccion.y;
            pos.y = Mathf.Clamp(pos.y, -limiteY, limiteY);
        }

        transform.position = pos;

        if (sr != null)
            sr.flipX = direccion.x < 0;
    }

    // Click del jugador.
    void OnMouseDown()
    {
        if (atrapado) return;
        Atrapar();
    }

    // Cambia dirección aleatoria.
    private void CambiarDireccion()
    {
        float angulo = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        direccion = new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo));
        timerCambioDir = tiempoCambioDir;
    }

    // El ratón es atrapado.
    private void Atrapar()
    {
        atrapado = true;
        animador?.Play("ratonMuere");

        GestorCazaRatones.Instance.RatonAtrapado();

        Invoke(nameof(Esconderse), 0.5f);
    }

    // Desactiva el objeto.
    private void Esconderse()
    {
        gameObject.SetActive(false);
    }
}