using UnityEngine;
using UnityEngine.UI;

public class ProductoCayendo : MonoBehaviour
{
    private float velocidad;
    private RectTransform bandeja;
    private float limiteInferior;
    private GestorAtrapar gestor;
    private RectTransform rt;
    private bool esMalo;
    private int penalizacion;

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

        rt.anchoredPosition += Vector2.down * velocidad * Time.deltaTime;

        if (bandeja != null)
        {
            Rect rectProducto = new Rect(
                rt.anchoredPosition.x - rt.rect.width / 2,
                rt.anchoredPosition.y - rt.rect.height / 2,
                rt.rect.width,
                rt.rect.height
            );

            Rect rectBandeja = new Rect(
                bandeja.anchoredPosition.x - bandeja.rect.width / 2,
                bandeja.anchoredPosition.y - bandeja.rect.height / 2,
                bandeja.rect.width,
                bandeja.rect.height
            );

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

        if (rt.anchoredPosition.y < limiteInferior)
            Destroy(gameObject);
    }
}