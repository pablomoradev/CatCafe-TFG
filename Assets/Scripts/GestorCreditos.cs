using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GestorCreditos : MonoBehaviour
{
    [Header("Creditos")]
    [SerializeField] private RectTransform contenedorCreditos;
    [SerializeField] private float velocidadScroll = 30f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip musicaCreditos;

    [Header("UI")]
    [SerializeField] private Button botonSaltar;

    private float posicionFinal;
    private bool terminado = false;

    void Start()
    {
        if (audioSource != null && musicaCreditos != null)
        {
            audioSource.clip = musicaCreditos;
            audioSource.Play();
        }

        Canvas canvas = FindObjectOfType<Canvas>();
        float alturaCanvas = canvas.GetComponent<RectTransform>().rect.height;

        contenedorCreditos.anchoredPosition = new Vector2(0, -alturaCanvas);
        posicionFinal = contenedorCreditos.rect.height + alturaCanvas;

        botonSaltar?.onClick.AddListener(Saltar);

        StartCoroutine(EsperarFinMusica());
    }

    void Update()
    {
        if (terminado) return;

        contenedorCreditos.anchoredPosition += Vector2.up * velocidadScroll * Time.deltaTime;

        // Solo termina cuando acabe la música, no cuando llegue arriba
        if (contenedorCreditos.anchoredPosition.y >= posicionFinal && !audioSource.isPlaying)
            VolverAlMenu();
    }

    private IEnumerator EsperarFinMusica()
    {
        yield return new WaitForSeconds(220f);
        VolverAlMenu();
    }

    private void VolverAlMenu()
    {
        if (terminado) return;
        terminado = true;
        SceneManager.LoadScene("MenuInicio");
    }

    public void Saltar()
    {
        VolverAlMenu();
    }
}