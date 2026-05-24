using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UITablonGatos : MonoBehaviour
{
    [Header("Prefab ficha de gato y contenedor")]
    [SerializeField] private GameObject prefabFichaGato;
    [SerializeField] private Transform contenedorFichas;

    [Header("Botones")]
    [SerializeField] private Button botonCerrar;
    [SerializeField] private Button botonSalvar;
    [SerializeField] private TablonGatos tablonGatos;

    private void Start()
    {
        botonCerrar?.onClick.AddListener(tablonGatos.CerrarPanel);
    }

    public void RefrescarPanel()
    {
        foreach (Transform hijo in contenedorFichas)
            Destroy(hijo.gameObject);

        botonSalvar?.onClick.RemoveAllListeners();

        DatosGato gatoActual = GestorMinijuegos.Instance?.GatoActual;
        bool esRecado = gatoActual == null && !GestorMinijuegos.Instance.TodosCompletados;

        if (esRecado)
        {
            botonSalvar?.GetComponentInChildren<TextMeshProUGUI>(true)?.SetText("Hacer recado");
            botonSalvar?.onClick.AddListener(() =>
            {
                tablonGatos.CerrarPanel();
                GestorMinijuegos.Instance?.LanzarMinijuegoActual();
            });
            CrearFichaRecado();
        }
        else
        {
            botonSalvar?.GetComponentInChildren<TextMeshProUGUI>(true)?.SetText("¡Salvar!");
            botonSalvar?.onClick.AddListener(() =>
            {
                tablonGatos.CerrarPanel();
                GestorMinijuegos.Instance?.LanzarMinijuegoActual();
            });

            List<DatosGato> gatos = GestorGatos.Instance.GatosEnTablon;
            if (gatos.Count == 0)
            {
                botonSalvar?.gameObject.SetActive(false);
                GameObject ficha = Instantiate(prefabFichaGato, contenedorFichas);
                ficha.transform.Find("TextoNombre")?.GetComponent<TextMeshProUGUI>()
                    ?.SetText("¡No hay gatos perdidos ahora mismo!");
            }
            else
            {
                botonSalvar?.gameObject.SetActive(true);
                foreach (DatosGato gato in gatos)
                    CrearFicha(gato);
            }
        }
    }

    private void CrearFichaRecado()
    {
        GameObject ficha = Instantiate(prefabFichaGato, contenedorFichas);

        ficha.transform.Find("ImagenGato")?.gameObject.SetActive(false);
        ficha.transform.Find("TextoNombre")?.gameObject.SetActive(false);
        ficha.transform.Find("TextoDescripcion")?.gameObject.SetActive(false);
        ficha.transform.Find("TextoDificultad")?.gameObject.SetActive(false);
        ficha.transform.Find("TextoBonus")?.gameObject.SetActive(false);
        ficha.transform.Find("TextoGasto")?.gameObject.SetActive(false);
        ficha.transform.Find("BotonSalvar")?.gameObject.SetActive(false);

        // Crear texto de recado directamente
        GameObject textoObj = new GameObject("TextoRecado");
        textoObj.transform.SetParent(ficha.transform, false);
        TMPro.TextMeshProUGUI tmp = textoObj.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = "¡Recado pendiente!";
        tmp.fontSize = 18;
        tmp.color = Color.black;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        RectTransform rt = textoObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300, 60);
    }

    private void CrearFicha(DatosGato gato)
    {
        GameObject ficha = Instantiate(prefabFichaGato, contenedorFichas);

        ficha.transform.Find("TextoNombre")?.GetComponent<TextMeshProUGUI>()
            ?.SetText(gato.nombreGato);
        if (gato.fotoGato != null)
            ficha.transform.Find("ImagenGato")?.GetComponent<Image>()
                ?.SetSprite(gato.fotoGato);
        ficha.transform.Find("TextoDescripcion")?.GetComponent<TextMeshProUGUI>()
            ?.SetText(gato.descripcion);

        string dificultadTexto = gato.dificultad switch
        {
            1 => "Dificultad: Facil",
            2 => "Dificultad: Intermedia",
            3 => "Dificultad: Dificil",
            _ => ""
        };
        ficha.transform.Find("TextoDificultad")?.GetComponent<TextMeshProUGUI>()
            ?.SetText(dificultadTexto);
        ficha.transform.Find("TextoBonus")?.GetComponent<TextMeshProUGUI>()
            ?.SetText($"Bonus: {gato.descripcionBonus}");
        ficha.transform.Find("TextoGasto")?.GetComponent<TextMeshProUGUI>()
            ?.SetText($"Coste: {gato.gastoPorDia} monedas/día");
        ficha.transform.Find("BotonSalvar")?.gameObject.SetActive(false);
    }

    private void IniciarRescate(DatosGato gato)
    {
        tablonGatos.CerrarPanel();
        GestorMinijuegos.Instance?.LanzarMinijuegoActual();
    }
}

public static class ExtImagen
{
    public static void SetSprite(this Image img, Sprite sprite)
    {
        if (sprite != null) img.sprite = sprite;
    }
}