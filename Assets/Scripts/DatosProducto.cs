using UnityEngine;

public enum TipoProducto
{
    Bebida,
    Comida
}

public enum OrigenProducto
{
    Cafetera,
    Cocina
}

[CreateAssetMenu(fileName = "NuevoProducto", menuName = "CafeteriaGatos/Producto")]
public class DatosProducto : ScriptableObject
{
    [Header("Identidad")]
    public string nombreProducto;
    public Sprite iconoProducto;

    [Header("Precio")]
    public int precio;

    [Header("Desbloqueo")]
    public int costeDesbloqueo; 

    [Header("Tipo")]
    public TipoProducto tipo;
    public OrigenProducto origen;

    [Header("Cooldown")]
    public float cooldown;
}