using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Panel lateral que muestra los gatos salvados y permite activar uno
// El gato activo aparece caminando por la cafeteria y aporta bonus
public class PanelGestorGatos : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panelGatos;
    [SerializeField] private GameObject prefabFichaGato;
    [SerializeField] private Transform contenedor;
    [SerializeField] private TextMeshProUGUI textoBoton;

    // Prefabs de los gatos que se instancian en la escena al activarlos
    [Header("Prefabs de gatos")]
    [SerializeField] private List<GameObject> prefabsGatos;

    private bool panelAbierto = false;
    private DatosGato gatoActivo = null;       // Gato seleccionado actualmente
    private GameObject gatoEnEscena = null;     // Instancia del gato en la cafeteria

    private void Start()
    {
        panelGatos.SetActive(false);
        ActivarUltimoGatoSalvado();
    }

    // Activa el ultimo gato salvado automaticamente
    public void ActivarUltimoGatoSalvado()
    {
        List<DatosGato> salvados = GestorGatos.Instance?.ObtenerGatosSalvados();
        if (salvados == null || salvados.Count == 0) return;
        ActivarGato(salvados[salvados.Count - 1]);
    }

    // Abre o cierra el panel
    public void TogglePanel()
    {
        panelAbierto = !panelAbierto;
        panelGatos.SetActive(panelAbierto);
        textoBoton.text = panelAbierto ? "▲" : "▼";
        if (panelAbierto) RefrescarPanel();
    }

    // Regenera las fichas de los gatos salvados
    private void RefrescarPanel()
    {
        foreach (Transform hijo in contenedor)
            Destroy(hijo.gameObject);

        List<DatosGato> salvados = GestorGatos.Instance.ObtenerGatosSalvados();
        foreach (DatosGato gato in salvados)
            CrearFicha(gato);
    }

    // Crea una ficha con los datos del gato y un toggle para activarlo
    private void CrearFicha(DatosGato gato)
    {
        GameObject ficha = Instantiate(prefabFichaGato, contenedor);

        ficha.transform.Find("ImagenGato")?.GetComponent<Image>()?.SetSprite(gato.fotoGato);
        ficha.transform.Find("nombreGato")?.GetComponent<TextMeshProUGUI>()?.SetText(gato.nombreGato);
        ficha.transform.Find("descripcion")?.GetComponent<TextMeshProUGUI>()?.SetText(gato.descripcion);
        ficha.transform.Find("textoBonus")?.GetComponent<TextMeshProUGUI>()?.SetText(gato.descripcionBonus);

        Toggle toggle = ficha.transform.Find("activargato")?.GetComponent<Toggle>();
        if (toggle != null)
        {
            toggle.isOn = gatoActivo == gato;
            toggle.onValueChanged.AddListener((activo) => { if (activo) ActivarGato(gato); });
        }
    }

    // Instancia el gato seleccionado en la escena y lo registra como activo
    private void ActivarGato(DatosGato gato)
    {
        if (gatoActivo == gato) return;
        gatoActivo = gato;

        if (gatoEnEscena != null) Destroy(gatoEnEscena);

        // Buscar el prefab que corresponde a este gato
        GameObject prefabElegido = null;
        foreach (GameObject prefab in prefabsGatos)
        {
            GatoSalvado gs = prefab.GetComponent<GatoSalvado>();
            if (gs != null && gs.datosGato == gato)
            {
                prefabElegido = prefab;
                break;
            }
        }

        if (prefabElegido != null)
        {
            gatoEnEscena = Instantiate(prefabElegido);
            GatoSalvado gsInstanciado = gatoEnEscena.GetComponent<GatoSalvado>();
            if (gsInstanciado != null)
                GameManager.Instance?.RegistrarGatoActivo(gsInstanciado);
        }

        RefrescarPanel();
    }
}