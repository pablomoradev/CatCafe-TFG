using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

// Gestiona la reproducción de vídeo en UI.
public class VideoManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;     // Reproductor de vídeo.
    public RawImage rawImage;           // UI donde se muestra el vídeo.
    public RenderTexture renderTexture; // Textura donde se renderiza el vídeo.

    void Start()
    {
        // Asigna la textura de salida del vídeo a la RenderTexture.
        videoPlayer.targetTexture = renderTexture;

        // Muestra la RenderTexture en la UI.
        rawImage.texture = renderTexture;

        // Prepara el vídeo antes de reproducirlo.
        videoPlayer.Prepare();

        // Cuando el vídeo esté listo, se llama a OnVideoPrepared.
        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    // Se ejecuta cuando el vídeo termina de prepararse.
    void OnVideoPrepared(VideoPlayer source)
    {
        source.playbackSpeed = 0.75f; // Reproduce más lento.
        source.Play();                // Inicia reproducción.
    }
}