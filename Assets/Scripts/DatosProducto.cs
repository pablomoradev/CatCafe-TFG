using UnityEngine;

// Tipos de producto disponibles en la cafetería.
public enum TipoProducto
{
    Bebida,
    Comida
}

// Lugar de origen del producto.
public enum OrigenProducto
{
    Cafetera,
    Cocina
}

// Datos de un producto configurable mediante ScriptableObject.
[CreateAssetMenu(fileName = "NuevoProducto", menuName = "CafeteriaGatos/Producto")]
public class DatosProducto : ScriptableObject
{
    [Header("Identidad")]
    public string nombreProducto;   // Nombre del producto.
    public Sprite iconoProducto;    // Icono visual.

    [Header("Precio")]
    public int precio;              // Precio de venta.

    [Header("Desbloqueo")]
    public int costeDesbloqueo;    // Coste para desbloquearlo.
    public bool estaDesbloqueado;  // Estado de desbloqueo.

    [Header("Tipo")]
    public TipoProducto tipo;      // Bebida o comida.
    public OrigenProducto origen;  // De dónde se prepara.

    [Header("Cooldown")]
    public float cooldown;         // Tiempo entre usos.
}