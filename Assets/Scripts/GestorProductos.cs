using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GestorProductos : MonoBehaviour
{
    public static GestorProductos Instance { get; private set; }

    [Header("Productos")]
    [SerializeField] private DatosProducto[] todosLosProductos;

    [Header("Paneles")]
    [SerializeField] private GameObject panelCafetera;
    [SerializeField] private GameObject panelCocina;
    [SerializeField] private Transform contenedorCafetera;
    [SerializeField] private Transform contenedorCocina;

    [Header("Prefab")]
    [SerializeField] private GameObject prefabBotonProducto;

    private Dictionary<DatosProducto, Coroutine> cooldownsActivos = new();
    private List<DatosProducto> productosListos = new List<DatosProducto>();

    private const string KEY_DESBLOQUEADO = "Producto_";

    void Awake() { Instance = this; }

    void Start()
    {
        GenerarBotones(contenedorCafetera, OrigenProducto.Cafetera);
        GenerarBotones(contenedorCocina, OrigenProducto.Cocina);
        panelCafetera?.SetActive(false);
        panelCocina?.SetActive(false);
    }

    public bool EstaDesbloqueado(DatosProducto p)
    {
        if (p.costeDesbloqueo == 0) return true;
        return PlayerPrefs.GetInt(KEY_DESBLOQUEADO + p.nombreProducto, 0) == 1;
    }

    private void GenerarBotones(Transform contenedor, OrigenProducto origen)
    {
        if (contenedor == null || prefabBotonProducto == null) return;

        foreach (Transform hijo in contenedor)
            Destroy(hijo.gameObject);

        foreach (DatosProducto p in todosLosProductos)
        {
            if (p.origen != origen) continue;

            GameObject btn = Instantiate(prefabBotonProducto, contenedor);
            bool desbloqueado = EstaDesbloqueado(p);

            btn.transform.Find("nombre")?.GetComponent<TextMeshProUGUI>()
                ?.SetText(p.nombreProducto);

            btn.transform.Find("precio")?.GetComponent<TextMeshProUGUI>()
                ?.SetText(desbloqueado ? p.precio + " monedas" : "🔒 " + p.costeDesbloqueo + " monedas");

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
        GenerarBotones(contenedorCafetera, OrigenProducto.Cafetera);
        GenerarBotones(contenedorCocina, OrigenProducto.Cocina);
    }

    public void AbrirCafetera()
    {
        panelCafetera?.SetActive(true);
        panelCocina?.SetActive(false);
    }

    public void AbrirCocina()
    {
        panelCocina?.SetActive(true);
        panelCafetera?.SetActive(false);
    }

    public void CerrarPaneles()
    {
        panelCafetera?.SetActive(false);
        panelCocina?.SetActive(false);
    }

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

    public DatosProducto ObtenerProductoPreparado()
    {
        return productosListos.Count > 0 ? productosListos[0] : null;
    }

    public void LimpiarProductoPreparado()
    {
        if (productosListos.Count > 0)
            productosListos.RemoveAt(0);
    }
}