using UnityEngine;
using SQLite;
using System.IO;

public class GestorBBDD : MonoBehaviour
{
    public static GestorBBDD Instance { get; private set; }
    private SQLiteConnection db;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        IniciarBBDD();
    }

    private void IniciarBBDD()
    {
        string ruta = Path.Combine(Application.dataPath, "catcafe.db");
        db = new SQLiteConnection(ruta);
        CrearTablas();
        Debug.Log("BBDD iniciada en: " + ruta);
    }

    private void CrearTablas()
    {
        db.Execute(@"CREATE TABLE IF NOT EXISTS JUGADOR (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            nombre TEXT NOT NULL,
            dinero INTEGER DEFAULT 0,
            fecha_inicio TEXT DEFAULT (date('now'))
        )");

        db.Execute(@"CREATE TABLE IF NOT EXISTS GATO (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            nombre TEXT NOT NULL,
            descripcion TEXT,
            dificultad TEXT,
            tipo_bonus TEXT,
            valor_bonus REAL,
            gasto_dia INTEGER
        )");

        db.Execute(@"CREATE TABLE IF NOT EXISTS GATO_JUGADOR (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            id_jugador INTEGER NOT NULL,
            id_gato INTEGER NOT NULL,
            esta_salvado INTEGER DEFAULT 0,
            felicidad REAL DEFAULT 0
        )");

        db.Execute(@"CREATE TABLE IF NOT EXISTS PRODUCTO (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            nombre TEXT NOT NULL,
            precio INTEGER,
            tipo TEXT,
            origen TEXT,
            cooldown REAL
        )");

        db.Execute(@"CREATE TABLE IF NOT EXISTS PEDIDO (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            id_jugador INTEGER NOT NULL,
            id_producto INTEGER NOT NULL,
            fecha TEXT DEFAULT (datetime('now')),
            completado INTEGER DEFAULT 0
        )");

        db.Execute(@"CREATE TABLE IF NOT EXISTS MINIJUEGO (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            nombre TEXT NOT NULL,
            dificultad TEXT,
            recompensa INTEGER
        )");

        db.Execute(@"CREATE TABLE IF NOT EXISTS PARTIDA_MINIJUEGO (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            id_jugador INTEGER NOT NULL,
            id_minijuego INTEGER NOT NULL,
            puntuacion INTEGER,
            fecha TEXT DEFAULT (datetime('now'))
        )");

        InsertarDatosIniciales();
    }

    private void InsertarDatosIniciales()
    {
        // Jugador
        int count = db.ExecuteScalar<int>("SELECT COUNT(*) FROM JUGADOR");
        if (count == 0)
            db.Execute("INSERT INTO JUGADOR (nombre, dinero) VALUES ('Jugador', 0)");

        // Gatos
        int gatosCount = db.ExecuteScalar<int>("SELECT COUNT(*) FROM GATO");
        if (gatosCount == 0)
        {
            db.Execute("INSERT INTO GATO (nombre, descripcion, dificultad, tipo_bonus, valor_bonus, gasto_dia) VALUES ('Milo', 'Gato jugueton', 'Facil', 'MasGanancias', 1.1, 5)");
            db.Execute("INSERT INTO GATO (nombre, descripcion, dificultad, tipo_bonus, valor_bonus, gasto_dia) VALUES ('Lira', 'Gato tranquilo', 'Dificil', 'MasPacienciaClientes', 1.2, 15)");
            db.Execute("INSERT INTO GATO (nombre, descripcion, dificultad, tipo_bonus, valor_bonus, gasto_dia) VALUES ('Alex', 'Gato listo', 'Intermedia', 'MasClientesPorHora', 1.15, 10)");
        }

        // Productos
        int productosCount = db.ExecuteScalar<int>("SELECT COUNT(*) FROM PRODUCTO");
        if (productosCount == 0)
        {
            db.Execute("INSERT INTO PRODUCTO (nombre, precio, tipo, origen, cooldown) VALUES ('Zumo', 6, 'Bebida', 'Cafetera', 3)");
            db.Execute("INSERT INTO PRODUCTO (nombre, precio, tipo, origen, cooldown) VALUES ('Te', 4, 'Bebida', 'Cafetera', 2)");
            db.Execute("INSERT INTO PRODUCTO (nombre, precio, tipo, origen, cooldown) VALUES ('Cafe', 5, 'Bebida', 'Cafetera', 3)");
            db.Execute("INSERT INTO PRODUCTO (nombre, precio, tipo, origen, cooldown) VALUES ('Latte', 8, 'Bebida', 'Cafetera', 5)");
            db.Execute("INSERT INTO PRODUCTO (nombre, precio, tipo, origen, cooldown) VALUES ('Capuccino', 7, 'Bebida', 'Cafetera', 4)");
            db.Execute("INSERT INTO PRODUCTO (nombre, precio, tipo, origen, cooldown) VALUES ('Croissant', 6, 'Comida', 'Cocina', 4)");
            db.Execute("INSERT INTO PRODUCTO (nombre, precio, tipo, origen, cooldown) VALUES ('Tortitas', 9, 'Comida', 'Cocina', 6)");
            db.Execute("INSERT INTO PRODUCTO (nombre, precio, tipo, origen, cooldown) VALUES ('Donut', 5, 'Comida', 'Cocina', 3)");
        }

        // Minijuegos
        int minijuegosCount = db.ExecuteScalar<int>("SELECT COUNT(*) FROM MINIJUEGO");
        if (minijuegosCount == 0)
        {
            db.Execute("INSERT INTO MINIJUEGO (nombre, dificultad, recompensa) VALUES ('Caza Ratones', 'Variable', 20)");
            db.Execute("INSERT INTO MINIJUEGO (nombre, dificultad, recompensa) VALUES ('Memory Tech', 'Media', 30)");
            db.Execute("INSERT INTO MINIJUEGO (nombre, dificultad, recompensa) VALUES ('Atrapar Productos', 'Media', 25)");
            db.Execute("INSERT INTO MINIJUEGO (nombre, dificultad, recompensa) VALUES ('Dados Davy Jones', 'Alta', 0)");
            db.Execute("INSERT INTO MINIJUEGO (nombre, dificultad, recompensa) VALUES ('Completa el Codigo', 'Media', 100)");
            db.Execute("INSERT INTO MINIJUEGO (nombre, dificultad, recompensa) VALUES ('BuscaBugs', 'Alta', 50)");
        }
    }

    // Métodos públicos
    public void RegistrarPedido(string nombreProducto)
    {
        int idProducto = db.ExecuteScalar<int>("SELECT id FROM PRODUCTO WHERE nombre = ?", nombreProducto);
        if (idProducto > 0)
            db.Execute("INSERT INTO PEDIDO (id_jugador, id_producto, completado) VALUES (1, ?, 1)", idProducto);
    }

    public void RegistrarPartidaMinijuego(string nombreMinijuego, int puntuacion)
    {
        int idMinijuego = db.ExecuteScalar<int>("SELECT id FROM MINIJUEGO WHERE nombre = ?", nombreMinijuego);
        if (idMinijuego > 0)
            db.Execute("INSERT INTO PARTIDA_MINIJUEGO (id_jugador, id_minijuego, puntuacion) VALUES (1, ?, ?)", idMinijuego, puntuacion);
    }

    public void ActualizarDineroJugador(int dinero)
    {
        db.Execute("UPDATE JUGADOR SET dinero = ? WHERE id = 1", dinero);
    }

    public void GuardarGatoSalvado(int idGato, float felicidad)
    {
        int existe = db.ExecuteScalar<int>("SELECT COUNT(*) FROM GATO_JUGADOR WHERE id_jugador = 1 AND id_gato = ?", idGato);
        if (existe == 0)
            db.Execute("INSERT INTO GATO_JUGADOR (id_jugador, id_gato, esta_salvado, felicidad) VALUES (1, ?, 1, ?)", idGato, felicidad);
        else
            db.Execute("UPDATE GATO_JUGADOR SET esta_salvado = 1, felicidad = ? WHERE id_jugador = 1 AND id_gato = ?", felicidad, idGato);
    }

    void OnDestroy()
    {
        db?.Close();
    }
}