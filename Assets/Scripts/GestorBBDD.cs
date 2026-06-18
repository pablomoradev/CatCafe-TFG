using UnityEngine;
using SQLite;
using System.IO;

// Gestiona la base de datos SQLite del juego
// Singleton con DontDestroyOnLoad que crea las tablas al iniciar
// y ofrece metodos para registrar pedidos, partidas y actualizar datos
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

    // Abre la conexion a la BBDD y crea las tablas si no existen
    private void IniciarBBDD()
    {
        string ruta = Path.Combine(Application.dataPath, "catcafe.db");
        db = new SQLiteConnection(ruta);
        CrearTablas();
    }

    // Crea las 8 tablas del esquema relacional si no existen
    private void CrearTablas()
    {
        db.Execute(@"CREATE TABLE IF NOT EXISTS JUGADOR (
            Id_Jugador INTEGER PRIMARY KEY AUTOINCREMENT,
            Dinero INTEGER DEFAULT 0,
            Fecha_Inicio TEXT DEFAULT (date('now'))
        )");

        db.Execute(@"CREATE TABLE IF NOT EXISTS GATO (
            Id_Gato INTEGER PRIMARY KEY AUTOINCREMENT,
            Nombre TEXT NOT NULL,
            Descripcion TEXT,
            Tipo_Bonus TEXT NOT NULL,
            Gasto INTEGER DEFAULT 0,
            Salvado INTEGER DEFAULT 0
        )");

        db.Execute(@"CREATE TABLE IF NOT EXISTS PRODUCTOS (
            Id_Prod INTEGER PRIMARY KEY AUTOINCREMENT,
            nombre TEXT NOT NULL,
            Tipo TEXT NOT NULL,
            Precio INTEGER NOT NULL,
            Origen TEXT NOT NULL,
            Cooldown REAL DEFAULT 3.0,
            Coste_Desbloqueo INTEGER DEFAULT 0,
            Desbloqueado INTEGER DEFAULT 0
        )");

        db.Execute(@"CREATE TABLE IF NOT EXISTS PEDIDOS (
            Id_Pedido INTEGER PRIMARY KEY AUTOINCREMENT,
            Id_Prod INTEGER,
            fecha_ped TEXT DEFAULT (datetime('now')),
            completado INTEGER DEFAULT 0,
            importe INTEGER DEFAULT 0,
            FOREIGN KEY (Id_Prod) REFERENCES PRODUCTOS(Id_Prod)
        )");

        db.Execute(@"CREATE TABLE IF NOT EXISTS MINIJUEGO (
            Id_Minijuego INTEGER PRIMARY KEY AUTOINCREMENT,
            nombre TEXT NOT NULL
        )");

        db.Execute(@"CREATE TABLE IF NOT EXISTS JUEGA (
            Id_Jugador INTEGER,
            Id_Minijuego INTEGER,
            puntuacion INTEGER DEFAULT 0,
            fecha TEXT DEFAULT (datetime('now')),
            ganado INTEGER DEFAULT 0,
            PRIMARY KEY (Id_Jugador, Id_Minijuego),
            FOREIGN KEY (Id_Jugador) REFERENCES JUGADOR(Id_Jugador),
            FOREIGN KEY (Id_Minijuego) REFERENCES MINIJUEGO(Id_Minijuego)
        )");

        db.Execute(@"CREATE TABLE IF NOT EXISTS MJ_SALVACION (
            Id_Minijuego INTEGER PRIMARY KEY,
            Id_Gato INTEGER UNIQUE,
            FOREIGN KEY (Id_Minijuego) REFERENCES MINIJUEGO(Id_Minijuego),
            FOREIGN KEY (Id_Gato) REFERENCES GATO(Id_Gato)
        )");

        db.Execute(@"CREATE TABLE IF NOT EXISTS MJ_RECADO (
            Id_Minijuego INTEGER PRIMARY KEY,
            Monedas INTEGER NOT NULL,
            FOREIGN KEY (Id_Minijuego) REFERENCES MINIJUEGO(Id_Minijuego)
        )");

        InsertarDatosIniciales();
    }

    // Inserta los datos iniciales si las tablas estan vacias
    // (jugador, 3 gatos, 8 productos, 6 minijuegos con sus tipos)
    private void InsertarDatosIniciales()
    {
        // Jugador por defecto
        int count = db.ExecuteScalar<int>("SELECT COUNT(*) FROM JUGADOR");
        if (count == 0)
            db.Execute("INSERT INTO JUGADOR (Dinero) VALUES (0)");

        // 3 gatos con sus bonus
        int gatosCount = db.ExecuteScalar<int>("SELECT COUNT(*) FROM GATO");
        if (gatosCount == 0)
        {
            db.Execute("INSERT INTO GATO (Nombre, Descripcion, Tipo_Bonus, Gasto, Salvado) VALUES ('Milo', 'Gato jugueton', 'MasGanancias', 5, 0)");
            db.Execute("INSERT INTO GATO (Nombre, Descripcion, Tipo_Bonus, Gasto, Salvado) VALUES ('Lira', 'Gato tranquilo', 'MasPacienciaClientes', 15, 0)");
            db.Execute("INSERT INTO GATO (Nombre, Descripcion, Tipo_Bonus, Gasto, Salvado) VALUES ('Alex', 'Gato listo', 'MasClientesPorHora', 10, 0)");
        }

        // 8 productos (Cafe desbloqueado por defecto)
        int productosCount = db.ExecuteScalar<int>("SELECT COUNT(*) FROM PRODUCTOS");
        if (productosCount == 0)
        {
            db.Execute("INSERT INTO PRODUCTOS (nombre, Tipo, Precio, Origen, Cooldown, Coste_Desbloqueo, Desbloqueado) VALUES ('Cafe', 'Bebida', 5, 'Cafetera', 3, 0, 1)");
            db.Execute("INSERT INTO PRODUCTOS (nombre, Tipo, Precio, Origen, Cooldown, Coste_Desbloqueo, Desbloqueado) VALUES ('Te', 'Bebida', 4, 'Cafetera', 2, 20, 0)");
            db.Execute("INSERT INTO PRODUCTOS (nombre, Tipo, Precio, Origen, Cooldown, Coste_Desbloqueo, Desbloqueado) VALUES ('Zumo', 'Bebida', 6, 'Cafetera', 3, 30, 0)");
            db.Execute("INSERT INTO PRODUCTOS (nombre, Tipo, Precio, Origen, Cooldown, Coste_Desbloqueo, Desbloqueado) VALUES ('Latte', 'Bebida', 8, 'Cafetera', 5, 50, 0)");
            db.Execute("INSERT INTO PRODUCTOS (nombre, Tipo, Precio, Origen, Cooldown, Coste_Desbloqueo, Desbloqueado) VALUES ('Capuccino', 'Bebida', 7, 'Cafetera', 4, 40, 0)");
            db.Execute("INSERT INTO PRODUCTOS (nombre, Tipo, Precio, Origen, Cooldown, Coste_Desbloqueo, Desbloqueado) VALUES ('Croissant', 'Comida', 6, 'Cocina', 4, 35, 0)");
            db.Execute("INSERT INTO PRODUCTOS (nombre, Tipo, Precio, Origen, Cooldown, Coste_Desbloqueo, Desbloqueado) VALUES ('Tortitas', 'Comida', 9, 'Cocina', 6, 60, 0)");
            db.Execute("INSERT INTO PRODUCTOS (nombre, Tipo, Precio, Origen, Cooldown, Coste_Desbloqueo, Desbloqueado) VALUES ('Donut', 'Comida', 5, 'Cocina', 3, 25, 0)");
        }

        // 6 minijuegos con sus tipos (salvacion o recado)
        int minijuegosCount = db.ExecuteScalar<int>("SELECT COUNT(*) FROM MINIJUEGO");
        if (minijuegosCount == 0)
        {
            db.Execute("INSERT INTO MINIJUEGO (nombre) VALUES ('Caza Ratones')");
            db.Execute("INSERT INTO MINIJUEGO (nombre) VALUES ('Adivina la tegnologia')");
            db.Execute("INSERT INTO MINIJUEGO (nombre) VALUES ('Dados de Davy Jones')");
            db.Execute("INSERT INTO MINIJUEGO (nombre) VALUES ('Atrapa los helados')");
            db.Execute("INSERT INTO MINIJUEGO (nombre) VALUES ('BuscaBugs')");
            db.Execute("INSERT INTO MINIJUEGO (nombre) VALUES ('¿Quien quiere ser programador?')");

            // Minijuegos de salvacion: cada uno desbloquea un gato
            db.Execute("INSERT INTO MJ_SALVACION (Id_Minijuego, Id_Gato) VALUES (1, 1)");
            db.Execute("INSERT INTO MJ_SALVACION (Id_Minijuego, Id_Gato) VALUES (3, 2)");
            db.Execute("INSERT INTO MJ_SALVACION (Id_Minijuego, Id_Gato) VALUES (5, 3)");

            // Minijuegos de recado: cada uno da monedas
            db.Execute("INSERT INTO MJ_RECADO (Id_Minijuego, Monedas) VALUES (2, 30)");
            db.Execute("INSERT INTO MJ_RECADO (Id_Minijuego, Monedas) VALUES (4, 25)");
        }
    }

    // Registra un pedido completado o fallido en la tabla PEDIDOS
    public void RegistrarPedido(string nombreProducto, int importe, bool completado)
    {
        int idProd = db.ExecuteScalar<int>("SELECT Id_Prod FROM PRODUCTOS WHERE nombre = ?", nombreProducto);
        if (idProd > 0)
            db.Execute("INSERT INTO PEDIDOS (Id_Prod, completado, importe) VALUES (?, ?, ?)",
                idProd, completado ? 1 : 0, importe);
    }

    // Registra una partida de minijuego en la tabla JUEGA
    public void RegistrarPartidaMinijuego(string nombreMinijuego, int puntuacion, bool ganado)
    {
        int idMinijuego = db.ExecuteScalar<int>("SELECT Id_Minijuego FROM MINIJUEGO WHERE nombre = ?", nombreMinijuego);
        if (idMinijuego > 0)
            db.Execute("INSERT OR REPLACE INTO JUEGA (Id_Jugador, Id_Minijuego, puntuacion, ganado) VALUES (1, ?, ?, ?)",
                idMinijuego, puntuacion, ganado ? 1 : 0);
    }

    // Actualiza el dinero del jugador en la tabla JUGADOR
    public void ActualizarDineroJugador(int dinero)
    {
        db.Execute("UPDATE JUGADOR SET Dinero = ? WHERE Id_Jugador = 1", dinero);
    }

    // Marca un gato como salvado en la tabla GATO
    public void GuardarGatoSalvado(int idGato)
    {
        db.Execute("UPDATE GATO SET Salvado = 1 WHERE Id_Gato = ?", idGato);
    }

    // Cierra la conexion a la BBDD al destruir el objeto
    void OnDestroy() { db?.Close(); }
}