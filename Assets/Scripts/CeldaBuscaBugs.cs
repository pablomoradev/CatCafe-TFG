using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// Representa una celda del juego BuscaBugs.
public class CeldaBuscaBugs : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI texto; // Texto mostrado en la celda.
    [SerializeField] private Image imagen;          // Imagen de fondo.

    // Colores para los distintos estados de la celda.
    [SerializeField] private Color colorOculta = new Color(0.3f, 0.6f, 0.3f);
    [SerializeField] private Color colorRevelada = new Color(0.8f, 0.8f, 0.8f);
    [SerializeField] private Color colorBug = new Color(1f, 0.3f, 0.3f);

    public bool EsBug { get; set; }             // Indica si contiene un bug.
    public bool Revelada { get; private set; }  // Indica si ya fue descubierta.
    public bool Marcada { get; set; }           // Indica si está marcada.
    public int BugsAlrededor { get; set; }      // Número de bugs cercanos.

    private int fila, col;                      // Posición en el tablero.
    private GestorBuscaBugs gestor;             // Referencia al gestor.

    // Colores para los números mostrados.
    private Color[] coloresNumeros = {
        Color.blue, Color.green, Color.red, Color.magenta,
        Color.cyan, Color.yellow, Color.white, Color.gray
    };

    // Inicializa la celda.
    public void Inicializar(int f, int c, GestorBuscaBugs g)
    {
        fila = f;
        col = c;
        gestor = g;

        if (imagen != null) imagen.color = colorOculta;
        if (texto != null) texto.text = "";
    }

    // Revela la celda y muestra el número de bugs cercanos.
    public void Revelar()
    {
        Revelada = true;
        if (imagen != null) imagen.color = colorRevelada;

        if (BugsAlrededor > 0)
        {
            if (texto != null)
            {
                texto.text = BugsAlrededor.ToString();
                texto.color = coloresNumeros[Mathf.Min(BugsAlrededor - 1, coloresNumeros.Length - 1)];
            }
        }
        else if (texto != null)
        {
            texto.text = "";
        }
    }

    // Revela un bug.
    public void RevelarBug()
    {
        Revelada = true;
        if (imagen != null) imagen.color = colorBug;
        if (texto != null) texto.text = "🐛";
    }

    // Gestiona los clics sobre la celda.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (Revelada) return;

        if (eventData.button == PointerEventData.InputButton.Right)
            gestor?.MarcarCelda(fila, col);
        else
            gestor?.RevelarCelda(fila, col);

        // Actualiza la apariencia si está marcada.
        if (Marcada && !Revelada)
        {
            if (imagen != null) imagen.color = new Color(1f, 0.8f, 0f);
            if (texto != null) texto.text = "🐛?";
        }
        else if (!Revelada)
        {
            if (imagen != null) imagen.color = colorOculta;
            if (texto != null) texto.text = "";
        }
    }
}