using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GestorBuscaBugs : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private Transform contenedorGrid;
    [SerializeField] private GameObject prefabCelda;
    [SerializeField] private int filas = 8;
    [SerializeField] private int columnas = 8;
    [SerializeField] private int numBugs = 10;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI textoBugsRestantes;
    [SerializeField] private TextMeshProUGUI textoTiempo;
    [SerializeField] private GameObject panelResultado;
    [SerializeField] private TextMeshProUGUI textoFinal;
    [SerializeField] private int recompensa = 50;

    private CeldaBuscaBugs[,] celdas;
    private bool juegoActivo = false;
    private float tiempoActual = 0f;
    private int celdasReveladas = 0;
    private int bugsMarcados = 0;

    void Start()
    {
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
        tiempoActual += Time.deltaTime;
        if (textoTiempo != null)
            textoTiempo.text = "Tiempo: " + Mathf.FloorToInt(tiempoActual) + "s";
    }

    private void IniciarJuego()
    {
        GenerarGrid();
        ColocarBugs();
        CalcularNumeros();
        juegoActivo = true;
        ActualizarBugsRestantes();
    }

    private void GenerarGrid()
    {
        celdas = new CeldaBuscaBugs[filas, columnas];

        for (int f = 0; f < filas; f++)
        {
            for (int c = 0; c < columnas; c++)
            {
                GameObject obj = Instantiate(prefabCelda, contenedorGrid);
                CeldaBuscaBugs celda = obj.GetComponent<CeldaBuscaBugs>();
                celda.Inicializar(f, c, this);
                celdas[f, c] = celda;
            }
        }
    }

    private void ColocarBugs()
    {
        int colocados = 0;
        while (colocados < numBugs)
        {
            int f = Random.Range(0, filas);
            int c = Random.Range(0, columnas);
            if (!celdas[f, c].EsBug)
            {
                celdas[f, c].EsBug = true;
                colocados++;
            }
        }
    }

    private void CalcularNumeros()
    {
        for (int f = 0; f < filas; f++)
        {
            for (int c = 0; c < columnas; c++)
            {
                if (celdas[f, c].EsBug) continue;
                int count = 0;
                for (int df = -1; df <= 1; df++)
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        int nf = f + df, nc = c + dc;
                        if (nf >= 0 && nf < filas && nc >= 0 && nc < columnas && celdas[nf, nc].EsBug)
                            count++;
                    }
                celdas[f, c].BugsAlrededor = count;
            }
        }
    }

    public void RevelarCelda(int fila, int col)
    {
        if (!juegoActivo) return;
        CeldaBuscaBugs celda = celdas[fila, col];

        if (celda.EsBug)
        {
            // Game over
            RevelarTodo();
            TerminarJuego(false);
            return;
        }

        if (celda.Revelada || celda.Marcada) return;
        celda.Revelar();
        celdasReveladas++;

        if (celda.BugsAlrededor == 0)
        {
            // Revelar celdas vecinas
            for (int df = -1; df <= 1; df++)
                for (int dc = -1; dc <= 1; dc++)
                {
                    int nf = fila + df, nc = col + dc;
                    if (nf >= 0 && nf < filas && nc >= 0 && nc < columnas)
                        RevelarCelda(nf, nc);
                }
        }

        // Comprobar victoria
        if (celdasReveladas == filas * columnas - numBugs)
            TerminarJuego(true);
    }

    public void MarcarCelda(int fila, int col)
    {
        if (!juegoActivo) return;
        CeldaBuscaBugs celda = celdas[fila, col];
        if (celda.Revelada) return;

        celda.Marcada = !celda.Marcada;
        bugsMarcados += celda.Marcada ? 1 : -1;
        ActualizarBugsRestantes();
    }

    private void RevelarTodo()
    {
        for (int f = 0; f < filas; f++)
            for (int c = 0; c < columnas; c++)
                if (celdas[f, c].EsBug)
                    celdas[f, c].RevelarBug();
    }

    private void ActualizarBugsRestantes()
    {
        if (textoBugsRestantes != null)
            textoBugsRestantes.text = "Bugs: " + (numBugs - bugsMarcados);
    }

    private void TerminarJuego(bool gano)
    {
        juegoActivo = false;
        panelResultado?.SetActive(true);

        if (gano)
        {
            GestorMinijuegos.Instance?.MinijuegoCompletado(); 
            GestorMinijuegos.Instance?.MinijuegoGanado();    
            GameManager.Instance?.AnadirDinero(recompensa);
            textoFinal?.SetText("Bugs eliminados!\n+" + recompensa + " monedas\nTiempo: " + Mathf.FloorToInt(tiempoActual) + "s");
        }
        else
        {
            textoFinal?.SetText("Encontraste un bug!\nEl codigo ha fallado...");
        }
        GestorBBDD.Instance?.RegistrarPartidaMinijuego("BuscaBugs", gano ? 1 : 0);
    }

    public void Volver()
    {
        SceneManager.LoadScene("CafeScene");
    }
}