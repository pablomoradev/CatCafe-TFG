using UnityEngine;
using TMPro;
using System.Collections;

// Gestiona los dialogos con efecto typewriter (letra a letra)
// Se puede avanzar con clic o espacio, y soporta multiples lineas
public class GestorDialogos : MonoBehaviour
{
    public static GestorDialogos Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panelDialogo;
    [SerializeField] private TextMeshProUGUI textoDialogo;
    [SerializeField] private float velocidadTexto = 0.03f; // Segundos entre cada letra

    private string[] lineasActuales;     // Todas las lineas del dialogo actual
    private int lineaActual;
    private bool escribiendo = false;    // True mientras se escribe letra a letra
    private bool esperandoInput = false; // True cuando ya termino de escribir y espera clic
    private Coroutine coroutinaTexto;

    void Awake() { Instance = this; }
    void Start() { panelDialogo?.SetActive(false); }

    // Muestra un dialogo con una sola linea
    public void MostrarDialogo(string texto)
    {
        MostrarDialogo(new string[] { texto });
    }

    // Muestra un dialogo con varias lineas que se avanzan con clic o espacio
    public void MostrarDialogo(string[] lineas)
    {
        if (coroutinaTexto != null)
            StopCoroutine(coroutinaTexto);

        lineasActuales = lineas;
        lineaActual = 0;
        panelDialogo?.SetActive(true);
        esperandoInput = false;
        MostrarLineaActual();
    }

    // Detecta clic o espacio para avanzar el dialogo
    void Update()
    {
        if (panelDialogo == null || !panelDialogo.activeSelf) return;
        if (!esperandoInput) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (escribiendo)
            {
                // Si esta escribiendo, mostrar la linea completa de golpe
                StopCoroutine(coroutinaTexto);
                textoDialogo.text = lineasActuales[lineaActual];
                escribiendo = false;
            }
            else
            {
                // Si ya termino, pasar a la siguiente linea o cerrar
                lineaActual++;
                if (lineaActual < lineasActuales.Length)
                    MostrarLineaActual();
                else
                    CerrarDialogo();
            }
        }
    }

    private void MostrarLineaActual()
    {
        coroutinaTexto = StartCoroutine(EscribirTexto(lineasActuales[lineaActual]));
    }

    // Escribe el texto letra a letra con la velocidad configurada
    private IEnumerator EscribirTexto(string texto)
    {
        escribiendo = true;
        esperandoInput = false;
        textoDialogo.text = "";

        // Esperar 2 frames para evitar que un clic anterior avance el dialogo
        yield return null;
        yield return null;
        esperandoInput = true;

        foreach (char c in texto)
        {
            textoDialogo.text += c;
            yield return new WaitForSeconds(velocidadTexto);
        }
        escribiendo = false;
    }

    private void CerrarDialogo()
    {
        panelDialogo?.SetActive(false);
        esperandoInput = false;
    }

    // Propiedad para saber si el dialogo esta activo (usado por GestorTutorial)
    public bool EstaActivo => panelDialogo != null && panelDialogo.activeSelf;
}