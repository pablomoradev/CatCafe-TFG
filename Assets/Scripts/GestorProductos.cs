using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

// Gestiona el sistema de productos de la cafeteria
// Genera los botones de productos, controla el cooldown de preparacion
// y permite desbloquear productos nuevos con monedas
public class GestorProductos : MonoBehaviour
{
    public static GestorProductos Instance { get; private set; }

    // Array con todos los productos del juego (ScriptableObjects)
    [Header("Productos")]
    [SerializeField] private DatosProducto[] todosLosProductos;

    // Paneles de la cafetera y la cocina
    [Header("Paneles")]
    [SerializeField] private GameObject panelCafetera;
    [SerializeField] private GameObject panelCocina;
    [SerializeField] private Transform contenedorCafetera;  // Donde se instancian los botones de bebidas
    [SerializeField] private Transform contenedorCocina;    // Donde se instancian los botones de comida

    // Prefab del boton de producto y fondo oscuro del panel
    [Header("Prefab")]
    [SerializeField] private GameObject prefabBotonProducto;
    [SerializeField] private GameObject fondoOscuro;

    // Cooldowns activos y productos listos para servir
    private Dictionary<DatosProducto, Coroutine> cooldownsActivos = new();
    private List<DatosProducto> productosListos = new List<DatosProducto>();

    // Clave base para guardar el desbloqueo en PlayerPrefs
    private const string KEY_DESBLOQUEADO = "Producto_";

    void Awake() { Instance = this; }

    void Start()
    {
        GenerarBotones(contenedorCafetera, OrigenProducto.Cafetera);
        GenerarBotones(contenedorCocina, OrigenProducto.Cocina);
        panelCafetera?.SetActive(false);
        panelCocina?.SetActive(false);
    }

    // Comprueba si un producto esta desbloqueado (coste 0 = desbloqueado por defecto)
    public bool EstaDesbloqueado(DatosProducto p)
    {
        if (p.costeDesbloqueo == 0) return true;
        return PlayerPrefs.GetInt(KEY_DESBLOQUEADO + p.nombreProducto, 0) == 1;
    }

    // Genera los botones de productos en el contenedor indicado
    // Los desbloqueados muestran precio normal, los bloqueados aparecen oscurecidos
    private void GenerarBotones(Transform contenedor, OrigenProducto origen)
    {
        if (contenedor == null || prefabBotonProducto == null) return;

        // Limpiar botones anteriores
        foreach (Transform hijo in contenedor)
            Destroy(hijo.gameObject);

        foreach (DatosProducto p in todosLosProductos)
        {
            if (p.origen != origen) continue;

            GameObject btn = Instantiate(prefabBotonProducto, contenedor);
            bool desbloqueado = EstaDesbloqueado(p);

            // Nombre del producto
            btn.transform.Find("nombre")?.GetComponent<TextMeshProUGUI>()
                ?.SetText(p.nombreProducto);

            // Precio o coste de desbloqueo
            btn.transform.Find("precio")?.GetComponent<TextMeshProUGUI>()
                ?.SetText(desbloqueado ? p.precio + " monedas" : "🔒 " + p.costeDesbloqueo + " monedas");

            // Icono del producto (oscurecido si esta bloqueado)
            Image icono = btn.transform.Find("icono")?.GetComponent<Image>();
            if (icono != null && p.iconoProducto != null)
            {
                icono.sprite = p.iconoProducto;
                icono.color = desbloqueado ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1f);
            }

            Slider barra = btn.transform.Find("barraCooldown")?.GetComponent<Slider>();
            Button boton = btn.GetComponent<Button>();

            if (boton != null)
            {
                DatosProducto captura = p;
                if (desbloqueado)
                    boton.onClick.AddListener(() => SeleccionarProducto(captura, boton, barra));
                else
                    boton.onClick.AddListener(() => IntentarDesbloquear(captura));
            }
        }
    }

    // Intenta desbloquear un producto gastando monedas
    private void IntentarDesbloquear(DatosProducto p)
    {
        if (!GameManager.Instance.TieneSuficiente(p.costeDesbloqueo))
        {
            GestorDialogos.Instance?.MostrarDialogo("Necesitas " + p.costeDesbloqueo + " monedas para desbloquear " + p.nombreProducto + ".");
            return;
        }
        GameManager.Instance.GastarDinero(p.costeDesbloqueo);
        PlayerPrefs.SetInt(KEY_DESBLOQUEADO + p.nombreProducto, 1);
        PlayerPrefs.Save();
        GestorDialogos.Instance?.MostrarDialogo("¡" + p.nombreProducto + " desbloqueado!");
        p.estaDesbloqueado = true;
        // Regenerar botones para actualizar la UI
        GenerarBotones(contenedorCafetera, OrigenProducto.Cafetera);
        GenerarBotones(contenedorCocina, OrigenProducto.Cocina);
    }

    // Abre el panel de la cafetera (bebidas)
    public void AbrirCafetera()
    {
        panelCafetera?.SetActive(true);
        panelCocina?.SetActive(false);
        fondoOscuro?.SetActive(true);
    }

    // Abre el panel de la cocina (comida)
    public void AbrirCocina()
    {
        panelCocina?.SetActive(true);
        panelCafetera?.SetActive(false);
        fondoOscuro?.SetActive(true);
    }

    // Cierra todos los paneles de productos
    public void CerrarPaneles()
    {
        panelCafetera?.SetActive(false);
        panelCocina?.SetActive(false);
        fondoOscuro?.SetActive(false);
    }

    // Selecciona un producto para preparar e inicia el cooldown
    private void SeleccionarProducto(DatosProducto producto, Button boton, Slider barra)
    {
        if (cooldownsActivos.ContainsKey(producto))
        {
            GestorDialogos.Instance?.MostrarDialogo("¡Ese producto ya se está preparando!");
            return;
        }

        CerrarPaneles();
        GestorClientes.Instance?.NotificarProductoPreparado(producto);
        Coroutine c = StartCoroutine(EjecutarCooldown(producto, boton, barra));
        cooldownsActivos[producto] = c;
    }

    // Ejecuta el cooldown de preparacion con barra animada
    // Al terminar, anade el producto a la lista de listos
    private IEnumerator EjecutarCooldown(DatosProducto producto, Button boton, Slider barra)
    {
        if (boton != null) boton.interactable = false;
        if (barra != null) barra.value = 0f;

        float elapsed = 0f;
        while (elapsed < producto.cooldown)
        {
            elapsed += Time.deltaTime;
            if (barra != null) barra.value = elapsed / producto.cooldown;
            yield return null;
        }

        productosListos.Add(producto);
        cooldownsActivos.Remove(producto);
        if (boton != null) boton.interactable = true;
        if (barra != null) barra.value = 0f;

        GestorDialogos.Instance?.MostrarDialogo("¡" + producto.nombreProducto + " listo! Llévalo a la mesa.");
    }

    // Devuelve un producto preparado que coincida con el pedido
    public DatosProducto ObtenerProductoPreparado(DatosProducto pedido = null)
    {
        if (pedido == null) return productosListos.Count > 0 ? productosListos[0] : null;
        return productosListos.Find(p => p == pedido);
    }
        
    // Elimina un producto de la lista de listos tras servirlo
    public void LimpiarProductoPreparado(DatosProducto producto)
    {
        productosListos.Remove(producto);
    }
}