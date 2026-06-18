using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// Gestiona la aparicion de clientes, asignacion de pedidos,
// barra de paciencia, servicio de productos y penalizacion
public class GestorClientes : MonoBehaviour
{
    public static GestorClientes Instance { get; private set; }

    // Configuracion de clientes
    [Header("Config clientes")]
    [SerializeField] private float tiempoEntreClientes = 8f;  // Segundos entre cada cliente nuevo
    [SerializeField] private float pacienciaMaxima = 15f;      // Segundos que espera antes de irse

    // Productos disponibles para pedir
    [Header("Productos")]
    [SerializeField] private DatosProducto[] productos;

    // Panel de pedido en la UI
    [Header("UI pedido")]
    [SerializeField] private GameObject panelPedido;
    [SerializeField] private TextMeshProUGUI textoPedido;
    [SerializeField] private Slider barraPaciencia;

    // Sprites de las mesas para resaltar la ocupada
    [Header("Mesas")]
    [SerializeField] private SpriteRenderer mesa1Sprite;
    [SerializeField] private SpriteRenderer mesa2Sprite;
    [SerializeField] private Color colorMesaOcupada = new Color(1f, 0.8f, 0.5f, 1f);

    // Vinetas flotantes con el icono del producto pedido
    [Header("Viñetas pedido")]
    [SerializeField] private GameObject vineta1;
    [SerializeField] private GameObject vineta2;
    [SerializeField] private Image iconoPedido1;
    [SerializeField] private Image iconoPedido2;
    [SerializeField] private TextMeshProUGUI nombrePedido1;
    [SerializeField] private TextMeshProUGUI nombrePedido2;

    // Estado interno
    private bool clienteEsperando = false;
    private DatosProducto pedidoActual;    // Producto que ha pedido el cliente actual
    private float pacienciaActual;
    private int mesaActual;                // Mesa donde esta sentado el cliente (1 o 2)
    private Color colorOriginalMesa;

    void Awake() { Instance = this; }

    void Start()
    {
        panelPedido?.SetActive(false);
        OcultarVinetas();
        if (mesa1Sprite != null) colorOriginalMesa = mesa1Sprite.color;
        StartCoroutine(SpawnClientes());
    }

    // Resta paciencia cada frame y comprueba si el cliente se fue
    void Update()
    {
        if (!clienteEsperando) return;
        pacienciaActual -= Time.deltaTime;
        if (barraPaciencia != null)
            barraPaciencia.value = pacienciaActual / pacienciaMaxima;
        if (pacienciaActual <= 0) ClienteSeFue();
    }

    // Genera clientes nuevos periodicamente
    private IEnumerator SpawnClientes()
    {
        yield return new WaitForSeconds(2f); // Espera inicial
        NuevoCliente();
        while (true)
        {
            yield return new WaitForSeconds(tiempoEntreClientes);
            if (!clienteEsperando) NuevoCliente();
        }
    }

    // Crea un nuevo cliente con un pedido aleatorio de los productos desbloqueados
    private void NuevoCliente()
    {
        if (productos == null || productos.Length == 0) return;

        // Filtrar solo productos desbloqueados
        System.Collections.Generic.List<DatosProducto> disponibles = new System.Collections.Generic.List<DatosProducto>();
        foreach (DatosProducto p in productos)
            if (p.estaDesbloqueado) disponibles.Add(p);

        if (disponibles.Count == 0) return;

        pedidoActual = disponibles[Random.Range(0, disponibles.Count)];
        pacienciaActual = pacienciaMaxima;
        clienteEsperando = true;
        mesaActual = Random.Range(1, 3); // Mesa 1 o 2

        ResaltarMesa(mesaActual, true);
        MostrarVineta(mesaActual);

        panelPedido?.SetActive(true);
        if (textoPedido != null)
            textoPedido.text = "Mesa " + mesaActual + ": " + pedidoActual.nombreProducto;

        GestorDialogos.Instance?.MostrarDialogo(
            "¡Nuevo cliente en la mesa " + mesaActual + "! Quiere un " + pedidoActual.nombreProducto);
    }

    // Notifica que se ha empezado a preparar un producto
    public void NotificarProductoPreparado(DatosProducto producto)
    {
        if (!clienteEsperando) return;

        // Validar que el producto se prepara en el mismo origen que el pedido
        if (producto.origen != pedidoActual.origen)
        {
            GestorDialogos.Instance?.MostrarDialogo("¡Ese producto no se prepara aquí!");
            return;
        }

        GestorDialogos.Instance?.MostrarDialogo(
            "Preparando " + producto.nombreProducto + "... ¡cuando este listo en la mesa " + mesaActual + "!");
    }

    // Metodos publicos para servir desde las zonas clicables de cada mesa
    public void ServirMesa1() { IntentarServir(1); }
    public void ServirMesa2() { IntentarServir(2); }

    // Intenta servir el pedido en la mesa indicada
    private void IntentarServir(int mesa)
    {
        if (!clienteEsperando) return;

        // Comprobar si hay un producto preparado que coincida con el pedido
        DatosProducto preparado = GestorProductos.Instance?.ObtenerProductoPreparado(pedidoActual);

        if (preparado == null)
        {
            GestorDialogos.Instance?.MostrarDialogo("¡Primero prepara el pedido en la cafetera o cocina!");
            return;
        }

        // Comprobar que es la mesa correcta
        if (mesa != mesaActual)
        {
            GestorDialogos.Instance?.MostrarDialogo(
                "Mesa equivocada. El cliente está en la mesa " + mesaActual);
            return;
        }

        // Servir con exito: cobrar, limpiar y registrar en BBDD
        clienteEsperando = false;
        panelPedido?.SetActive(false);
        ResaltarMesa(mesaActual, false);
        OcultarVinetas();
        GestorProductos.Instance?.LimpiarProductoPreparado(pedidoActual);
        GameManager.Instance?.AnadirDinero(pedidoActual.precio);
        GestorBBDD.Instance?.RegistrarPedido(pedidoActual.nombreProducto, pedidoActual.precio, true);

        GestorDialogos.Instance?.MostrarDialogo(new string[] {
            "¡Aquí tiene su " + pedidoActual.nombreProducto + "!",
            "+" + pedidoActual.precio + " monedas. ¡Buen trabajo!"
        });
    }

    // El cliente se fue porque se agoto su paciencia
    private void ClienteSeFue()
    {
        clienteEsperando = false;
        panelPedido?.SetActive(false);
        ResaltarMesa(mesaActual, false);
        OcultarVinetas();

        // Penalizacion: pierde la mitad del precio del producto
        int penalizacion = pedidoActual != null ? pedidoActual.precio / 2 : 5;
        GameManager.Instance?.GastarDinero(penalizacion);

        GestorDialogos.Instance?.MostrarDialogo(new string[] {
            "El cliente se fue enfadado...",
            "-" + penalizacion + " monedas de penalización."
        });
    }

    // Muestra la vineta con el icono del producto pedido encima de la mesa
    private void MostrarVineta(int mesa)
    {
        if (mesa == 1)
        {
            if (vineta1 != null) vineta1.SetActive(true);
            if (iconoPedido1 != null && pedidoActual.iconoProducto != null)
                iconoPedido1.sprite = pedidoActual.iconoProducto;
            if (nombrePedido1 != null)
                nombrePedido1.text = pedidoActual.nombreProducto;
        }
        else
        {
            if (vineta2 != null) vineta2.SetActive(true);
            if (iconoPedido2 != null && pedidoActual.iconoProducto != null)
                iconoPedido2.sprite = pedidoActual.iconoProducto;
            if (nombrePedido2 != null)
                nombrePedido2.text = pedidoActual.nombreProducto;
        }
    }

    // Oculta todas las vinetas de pedido
    private void OcultarVinetas()
    {
        if (vineta1 != null) vineta1.SetActive(false);
        if (vineta2 != null) vineta2.SetActive(false);
    }

    // Cambia el color de la mesa para indicar que esta ocupada
    private void ResaltarMesa(int mesa, bool resaltar)
    {
        SpriteRenderer sr = (mesa == 1) ? mesa1Sprite : mesa2Sprite;
        if (sr != null) sr.color = resaltar ? colorMesaOcupada : colorOriginalMesa;
    }

    // Llamado por GestorGatos para modificar la paciencia con bonus de gato
    public void ModificarPaciencia(float multiplicador) { pacienciaMaxima *= multiplicador; }

    // Llamado por GestorGatos para modificar el ritmo de aparicion con bonus de gato
    public void ModificarRitmoAparicion(float multiplicador) { tiempoEntreClientes /= multiplicador; }
}