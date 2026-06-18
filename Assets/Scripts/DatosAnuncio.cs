using UnityEngine;

// Datos de un anuncio configurable mediante ScriptableObject.
[CreateAssetMenu(fileName = "NuevoAnuncio", menuName = "CafeteriGatos/Anuncio")]
public class DatosAnuncio : ScriptableObject
{
    [Header("Contenido")]
    public string titulo;          // Título del anuncio.

    [TextArea(2, 4)]
    public string descripcion;     // Descripción mostrada al jugador.

    public Sprite icono;           // Icono representativo.

    [Header("Tipo")]
    public TipoAnuncio tipo;       // Categoría del anuncio.

    [Header("Coste")]
    public int coste;              // Precio para desbloquearlo.
    public bool estaDesbloqueado;  // Indica si ya está disponible.

    [Header("Efecto")]
    public EfectoAnuncio efecto;   // Efecto que aplica.
    public float valorEfecto;      // Intensidad del efecto.
}

// Tipos de anuncios disponibles.
public enum TipoAnuncio
{
    Mejora,    // Mejora permanente.
    Evento,    // Evento temporal.
    Info       // Información o consejo.
}

// Efectos que puede producir un anuncio.
public enum EfectoAnuncio
{
    Ninguno,
    BonusPacienciaClientes,
    MultiplicadorGanancias,
    BonusAparicionClientes,
    DesbloquearMinijuego
}