using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CeldaBuscaBugs : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI texto;
    [SerializeField] private Image imagen;
    [SerializeField] private Color colorOculta = new Color(0.3f, 0.6f, 0.3f);
    [SerializeField] private Color colorRevelada = new Color(0.8f, 0.8f, 0.8f);
    [SerializeField] private Color colorBug = new Color(1f, 0.3f, 0.3f);

    public bool EsBug { get; set; }
    public bool Revelada { get; private set; }
    public bool Marcada { get; set; }
    public int BugsAlrededor { get; set; }

    private int fila, col;
    private GestorBuscaBugs gestor;

    private Color[] coloresNumeros = {
        Color.blue, Color.green, Color.red, Color.magenta,
        Color.cyan, Color.yellow, Color.white, Color.gray
    };

    public void Inicializar(int f, int c, GestorBuscaBugs g)
    {
        fila = f; col = c; gestor = g;
        if (imagen != null) imagen.color = colorOculta;
        if (texto != null) texto.text = "";
    }

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
        else
        {
            if (texto != null) texto.text = "";
        }
    }

    public void RevelarBug()
    {
        Revelada = true;
        if (imagen != null) imagen.color = colorBug;
        if (texto != null) texto.text = "🐛";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Revelada) return;

        if (eventData.button == PointerEventData.InputButton.Right)
            gestor?.MarcarCelda(fila, col);
        else
            gestor?.RevelarCelda(fila, col);

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