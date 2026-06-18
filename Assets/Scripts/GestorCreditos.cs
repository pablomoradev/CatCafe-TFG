using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

// Gestiona la escena de creditos finales
// Los creditos suben con scroll automatico y se puede saltar con un boton
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
        // Reproducir musica de creditos
        if (audioSource != null && musicaCreditos != null)
        {
            audioSource.clip = musicaCreditos;
            audioSource.Play();
        }

        // Posicionar los creditos debajo de la pantalla
        Canvas canvas = FindObjectOfType<Canvas>();
        float alturaCanvas = canvas.GetComponent<RectTransform>().rect.height;
        contenedorCreditos.anchoredPosition = new Vector2(0, -alturaCanvas);
        posicionFinal = contenedorCreditos.rect.height + alturaCanvas;

        botonSaltar?.onClick.AddListener(Saltar);
        StartCoroutine(EsperarFinMusica());
    }

    // Mueve los creditos hacia arriba cada frame
    void Update()
    {
        if (terminado) return;
        contenedorCreditos.anchoredPosition += Vector2.up * velocidadScroll * Time.deltaTime;

        // Termina cuando acabe el scroll y la musica
        if (contenedorCreditos.anchoredPosition.y >= posicionFinal && !audioSource.isPlaying)
            VolverAlMenu();
    }

    // Seguridad: volver al menu despues de 220 segundos aunque no termine el scroll
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

    // Boton para saltar los creditos
    public void Saltar() { VolverAlMenu(); }
}