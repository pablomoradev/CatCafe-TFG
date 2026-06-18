using UnityEngine;
using UnityEngine.UI;

// Controla el comportamiento de un producto que cae en el minijuego.
public class ProductoCayendo : MonoBehaviour
{
    private float velocidad;                 // Velocidad de caída.
    private RectTransform bandeja;           // Bandeja del jugador.
    private float limiteInferior;            // Límite para destruir objeto.
    private GestorAtrapar gestor;           // Gestor del minijuego.
    private RectTransform rt;               // RectTransform del producto.

    private bool esMalo;                    // Si el producto es negativo.
    private int penalizacion;               // Penalización si se atrapa.

    // Inicializa el producto.
    public void Inicializar(float velocidad, RectTransform bandeja, float alturaCanvas, GestorAtrapar gestor, bool esMalo, int penalizacion)
    {
        this.velocidad = velocidad;
        this.bandeja = bandeja;
        this.limiteInferior = -alturaCanvas / 2 - 100;
        this.gestor = gestor;
        this.esMalo = esMalo;
        this.penalizacion = penalizacion;

        rt = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (rt == null) return;

        // Movimiento hacia abajo.
        rt.anchoredPosition += Vector2.down * velocidad * Time.deltaTime;

        if (bandeja != null)
        {
            float hitboxProducto = 60f;

            Rect rectProducto = new Rect(
                rt.anchoredPosition.x - hitboxProducto / 2,
                rt.anchoredPosition.y - hitboxProducto / 2,
                hitboxProducto,
                hitboxProducto
            );

            float hitboxAncho = 100f;
            float hitboxAlto = 50f;

            Rect rectBandeja = new Rect(
                bandeja.anchoredPosition.x - hitboxAncho / 2,
                bandeja.anchoredPosition.y - hitboxAlto / 2,
                hitboxAncho,
                hitboxAlto
            );

            // Detecta colisión con la bandeja.
            if (rectProducto.Overlaps(rectBandeja))
            {
                if (esMalo)
                    gestor?.ProductoMalo(penalizacion);
                else
                    gestor?.ProductoAtrapado();

                Destroy(gameObject);
                return;
            }
        }

        // Destruye el objeto si cae fuera de pantalla.
        if (rt.anchoredPosition.y < limiteInferior)
            Destroy(gameObject);
    }
}