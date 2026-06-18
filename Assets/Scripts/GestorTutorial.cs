using System.Collections;
using UnityEngine;

// Gestiona el tutorial interactivo de la cafeteria
// Solo se muestra la primera vez que el jugador abre el juego
// Guia al jugador por las zonas principales: mostrador, mesas y tablon
public class GestorTutorial : MonoBehaviour
{
    public static GestorTutorial Instance { get; private set; }

    // Zonas que se resaltan durante el tutorial
    [Header("Zonas clicables del tutorial")]
    [SerializeField] private ZonaClicable zonaMostrador;
    [SerializeField] private ZonaClicable zonaMesa1;
    [SerializeField] private ZonaClicable zonaMesa2;
    [SerializeField] private ZonaClicable zonaTablon;

    [Header("Color de resaltado del tutorial")]
    [SerializeField] private Color colorResaltado = new Color(1f, 1f, 0f, 0.5f);

    private int pasoActual = 0;
    private ZonaClicable zonaResaltadaActual;

    // Dialogos de cada paso del tutorial
    private string[][] dialogosPorPaso = new string[][]
    {
        new string[] {
            "Bienvenido a tu propia cafeteria de gatos.",
            "Aqui podras atender clientes, ganar monedas y rescatar gatos perdidos.",
            "Vamos a ver como funciona todo."
        },
        new string[] {
            "Este es el mostrador.",
            "Cuando un cliente llegue y pida algo, haz clic aqui para preparar el pedido."
        },
        new string[] {
            "Esta es una de las mesas.",
            "Una vez preparado el pedido, haz clic en la mesa correcta para servirlo."
        },
        new string[] {
            "Este es el tablon de gatos perdidos.",
            "Haz clic para ver que gatos necesitan ayuda y lanzar el minijuego para rescatarlos.",
            "Cada gato rescatado te dara un bonus especial en la cafeteria."
        },
        new string[] {
            "Ya lo sabes todo.",
            "Buena suerte al frente de tu cafeteria."
        }
    };

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // Solo inicia el tutorial si no se ha visto antes
    private void Start()
    {
        if (!PlayerPrefs.HasKey("TutorialVisto"))
            IniciarTutorial();
    }

    // Desactiva los clientes durante el tutorial y empieza por el paso 0
    private void IniciarTutorial()
    {
        pasoActual = 0;
        GestorClientes.Instance?.gameObject.SetActive(false);
        MostrarPaso(pasoActual);
    }

    // Muestra el dialogo y resalta la zona correspondiente a cada paso
    private void MostrarPaso(int paso)
    {
        QuitarResaltado();

        switch (paso)
        {
            case 0: MostrarDialogoYResaltar(dialogosPorPaso[0], null); break;
            case 1: MostrarDialogoYResaltar(dialogosPorPaso[1], zonaMostrador); break;
            case 2: MostrarDialogoYResaltar(dialogosPorPaso[2], zonaMesa1); break;
            case 3: MostrarDialogoYResaltar(dialogosPorPaso[3], zonaTablon); break;
            case 4: MostrarDialogoYResaltar(dialogosPorPaso[4], null); break;
            default: FinalizarTutorial(); break;
        }
    }

    // Muestra el dialogo y resalta la zona indicada
    private void MostrarDialogoYResaltar(string[] lineas, ZonaClicable zona)
    {
        if (zona != null)
        {
            zonaResaltadaActual = zona;
            ResaltarZona(zona, true);
        }

        GestorDialogos.Instance?.MostrarDialogo(lineas);
        StartCoroutine(EsperarDialogo());
    }

    // Espera a que el jugador cierre el dialogo y avanza al siguiente paso
    private IEnumerator EsperarDialogo()
    {
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => !EstaDialogoActivo());
        pasoActual++;
        MostrarPaso(pasoActual);
    }

    private bool EstaDialogoActivo()
    {
        if (GestorDialogos.Instance == null) return false;
        return GestorDialogos.Instance.EstaActivo;
    }

    // Cambia el color de la zona para resaltarla
    private void ResaltarZona(ZonaClicable zona, bool activar)
    {
        SpriteRenderer sr = zona.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = activar ? colorResaltado : Color.white;
    }

    private void QuitarResaltado()
    {
        if (zonaResaltadaActual != null)
        {
            ResaltarZona(zonaResaltadaActual, false);
            zonaResaltadaActual = null;
        }
    }

    // Marca el tutorial como visto y reactiva los clientes
    private void FinalizarTutorial()
    {
        QuitarResaltado();
        GestorClientes.Instance?.gameObject.SetActive(true);
        PlayerPrefs.SetInt("TutorialVisto", 1);
        PlayerPrefs.Save();
    }
}