using UnityEngine;

// Datos configurables de un gato usando ScriptableObject.
[CreateAssetMenu(fileName = "NuevoGato", menuName = "CafeteriaGatos/Gato")]
public class DatosGato : ScriptableObject
{
    [Header("Identidad")]
    public string nombreGato;          // Nombre del gato.
    public Sprite fotoGato;            // Imagen del gato.

    [TextArea(2, 3)]
    public string descripcion;         // Descripción del gato.

    [Header("Dificultad minijuego")]
    [Range(1, 3)]
    public int dificultad;             // Dificultad del minijuego.

    [Header("Bonus a la cafetería")]
    public BonusGato tipoBonus;        // Tipo de bonus que aporta.
    public float valorBonus;           // Valor del bonus.
    public string descripcionBonus;    // Texto explicativo del bonus.

    [Header("Gasto pasivo")]
    public int gastoPorDia;            // Coste diario del gato.

    [Header("Estado")]
    public bool estaSalvado;           // Si el gato ya fue rescatado.
}

// Tipos de bonus que puede aportar un gato.
public enum BonusGato
{
    Ninguno,
    MasPacienciaClientes,
    MasGanancias,
    MasClientesPorHora,
    DescuentoCompras
}