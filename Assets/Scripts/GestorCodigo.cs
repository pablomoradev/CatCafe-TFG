using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GestorCodigo : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI textoCodigoArriba;
    [SerializeField] private TextMeshProUGUI textoCodigoAbajo;
    [SerializeField] private TextMeshProUGUI textoPregunta;
    [SerializeField] private Button[] botonesOpciones; // 3 botones
    [SerializeField] private TextMeshProUGUI textoPuntuacion;
    [SerializeField] private TextMeshProUGUI textoTiempo;
    [SerializeField] private GameObject panelResultado;
    [SerializeField] private TextMeshProUGUI textoFinal;

    [Header("Config")]
    [SerializeField] private float tiempoLimite = 60f;
    [SerializeField] private int recompensaPorAcierto = 10;

    private float tiempoActual;
    private int puntuacion = 0;
    private int preguntaActual = 0;
    private bool juegoActivo = false;

    // Preguntas: codigo1, codigo2, respuesta correcta, opciones
    private List<(string arriba, string abajo, string correcta, string[] opciones)> preguntas = new List<(string, string, string, string[])>
    {
        (
            "public class Main {",
            "    }\n}",
            "public static void main(String[] args) {",
            new string[] { "public static void main(String[] args) {", "void start() {", "static int main() {" }
        ),
        (
            "int[] numeros = ",
            ";",
            "new int[]{1, 2, 3}",
            new string[] { "new int[]{1, 2, 3}", "int[1, 2, 3]", "{1, 2, 3}[]" }
        ),
        (
            "for (int i = 0; ",
            "; i++) {",
            "i < 10",
            new string[] { "i < 10", "i = 10", "i > 10" }
        ),
        (
            "String nombre = ",
            ";",
            "\"Hola Mundo\"",
            new string[] { "\"Hola Mundo\"", "'Hola Mundo'", "Hola Mundo" }
        ),
        (
            "if (x > 0) {",
            "} else {\n    x = 0;\n}",
            "    x = x * 2;",
            new string[] { "    x = x * 2;", "    x == x * 2;", "    x => x * 2;" }
        ),
        (
            "public int sumar(int a, int b) {",
            "}",
            "    return a + b;",
            new string[] { "    return a + b;", "    print(a + b);", "    a + b;" }
        ),
        (
            "List<String> lista = ",
            ";",
            "new ArrayList<>()",
            new string[] { "new ArrayList<>()", "new List<String>()", "ArrayList.new()" }
        ),
        (
            "try {",
            "} catch (Exception e) {\n    e.printStackTrace();\n}",
            "    int x = Integer.parseInt(\"abc\");",
            new string[] { "    int x = Integer.parseInt(\"abc\");", "    int x = 0;", "    String x = \"abc\";" }
        ),
        (
            "System.out.",
            "(\"Hola Mundo\");",
            "println",
            new string[] { "println", "print", "log" }
        ),
        (
            "int resultado = ",
            ";",
            "Math.pow(2, 8)",
            new string[] { "Math.pow(2, 8)", "2^8", "pow(2, 8)" }
        ),
    };

    void Start()
    {
        tiempoActual = tiempoLimite;
        panelResultado?.SetActive(false);

        CuentaAtras ca = FindObjectOfType<CuentaAtras>();
        if (ca != null)
            ca.OnFinCuentaAtras += IniciarJuego;
        else
            IniciarJuego();
    }

    void Update()
    {
        if (!juegoActivo) return;
        tiempoActual -= Time.deltaTime;
        if (textoTiempo != null)
            textoTiempo.text = "Tiempo: " + Mathf.CeilToInt(tiempoActual);
        if (tiempoActual <= 0) TerminarJuego();
    }

    private void IniciarJuego()
    {
        juegoActivo = true;
        MostrarPregunta();
    }

    private void MostrarPregunta()
    {
        if (preguntaActual >= preguntas.Count)
        {
            TerminarJuego();
            return;
        }

        var p = preguntas[preguntaActual];

        textoCodigoArriba?.SetText(p.arriba);
        textoCodigoAbajo?.SetText(p.abajo);
        textoPregunta?.SetText("¿Qué va en el hueco?");

        // Mezclar opciones
        List<string> opcMezcladas = new List<string>(p.opciones);
        for (int i = opcMezcladas.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            string temp = opcMezcladas[i];
            opcMezcladas[i] = opcMezcladas[j];
            opcMezcladas[j] = temp;
        }

        for (int i = 0; i < botonesOpciones.Length; i++)
        {
            if (i >= opcMezcladas.Count) break;
            string opcion = opcMezcladas[i];
            string correcta = p.correcta;
            botonesOpciones[i].GetComponentInChildren<TextMeshProUGUI>()?.SetText(opcion);
            botonesOpciones[i].onClick.RemoveAllListeners();
            botonesOpciones[i].onClick.AddListener(() => ResponderPregunta(opcion, correcta));
        }
    }

    private void ResponderPregunta(string opcion, string correcta)
    {
        if (opcion == correcta)
        {
            puntuacion += recompensaPorAcierto;
            if (textoPuntuacion != null)
                textoPuntuacion.text = "Puntos: " + puntuacion;
            GestorDialogos.Instance?.MostrarDialogo("Correcto! +" + recompensaPorAcierto + " puntos");
        }
        else
        {
            GestorDialogos.Instance?.MostrarDialogo("Incorrecto. Era: " + correcta);
        }

        preguntaActual++;
        StartCoroutine(SiguientePregunta());
    }

    private IEnumerator SiguientePregunta()
    {
        yield return new WaitForSeconds(1.5f);
        MostrarPregunta();
    }

    private void TerminarJuego()
    {
        juegoActivo = false;
        panelResultado?.SetActive(true);
        GameManager.Instance?.AnadirDinero(puntuacion);
        if (textoFinal != null)
            textoFinal.text = "Fin del juego!\n+" + puntuacion + " monedas";
        GestorBBDD.Instance?.RegistrarPartidaMinijuego("Completa el Codigo", puntuacion);
    }

    public void Volver()
    {
        SceneManager.LoadScene("CafeScene");
    }
}