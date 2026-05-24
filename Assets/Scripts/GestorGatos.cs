using System.Collections.Generic;
using UnityEngine;

public class GestorGatos : MonoBehaviour
{
    public static GestorGatos Instance { get; private set; }

    [Header("Todos los gatos del juego")]
    [SerializeField] private List<DatosGato> todosLosGatos;

    [Header("Prefab gato salvado")]
    [SerializeField] private GameObject prefabGatoSalvado;

    public List<DatosGato> GatosEnTablon { get; private set; } = new List<DatosGato>();

    [Header("Cuántos gatos mostrar a la vez")]
    [SerializeField] private int minGatosTablon = 1;
    [SerializeField] private int maxGatosTablon = 3;

    private int indexGatoActivo = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnScenaCargada;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnScenaCargada;
    }

    private void OnScenaCargada(UnityEngine.SceneManagement.Scene escena, UnityEngine.SceneManagement.LoadSceneMode modo)
    {
        // El jugador elige el gato desde el panel, no se instancia automáticamente
    }

    public void GenerarGatosTablon()
    {
        GatosEnTablon.Clear();

        List<DatosGato> disponibles = todosLosGatos.FindAll(g => !g.estaSalvado);

        if (disponibles.Count == 0) return;

        int cantidad = Random.Range(minGatosTablon, Mathf.Min(maxGatosTablon, disponibles.Count) + 1);

        for (int i = disponibles.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            DatosGato temp = disponibles[i];
            disponibles[i] = disponibles[j];
            disponibles[j] = temp;
        }

        for (int i = 0; i < cantidad; i++)
            GatosEnTablon.Add(disponibles[i]);
    }

    public void SalvarGato(DatosGato gato)
    {
        gato.estaSalvado = true;
        AplicarBonus(gato);

        if (GestorDialogos.Instance != null)
        {
            GestorDialogos.Instance.MostrarDialogo(new string[]
            {
                $"¡Has salvado a {gato.nombreGato}!",
                $"Bonus: {gato.descripcionBonus}",
                $"Gasto pasivo: {gato.gastoPorDia} monedas/día"
            });
        }
    }

    private void AplicarBonus(DatosGato gato)
    {
        switch (gato.tipoBonus)
        {
            case BonusGato.MasPacienciaClientes:
                GestorClientes.Instance?.ModificarPaciencia(gato.valorBonus);
                break;
            case BonusGato.MasGanancias:
                GameManager.Instance?.ModificarMultiplicador(gato.valorBonus);
                break;
            case BonusGato.MasClientesPorHora:
                GestorClientes.Instance?.ModificarRitmoAparicion(gato.valorBonus);
                break;
            case BonusGato.Ninguno:
            case BonusGato.DescuentoCompras:
            default:
                break;
        }
    }

    public List<DatosGato> ObtenerGatosSalvados()
    {
        return todosLosGatos.FindAll(g => g.estaSalvado);
    }

    public void ReiniciarGatos()
    {
        foreach (DatosGato gato in todosLosGatos)
            gato.estaSalvado = false;
        indexGatoActivo = -1;
    }

    // --- Métodos nuevos para el guardado ---

    public DatosGato[] ObtenerTodosLosGatos()
    {
        return todosLosGatos.ToArray();
    }

    public int ObtenerIndexGatoActivo()
    {
        return indexGatoActivo;
    }

    public void EstablecerGatoActivo(int index)
    {
        if (index < 0 || index >= todosLosGatos.Count) return;
        indexGatoActivo = index;
    }
    public void SalvarGatoPorIndex(int index)
    {
        if (index < 0 || index >= todosLosGatos.Count) return;
        DatosGato gato = todosLosGatos[index];
        if (gato.estaSalvado) return;
        SalvarGato(gato);
        GestorBBDD.Instance?.GuardarGatoSalvado(index + 1, 100f);
    }
    public DatosGato ObtenerGatoPorIndex(int index)
    {
        if (index < 0 || index >= todosLosGatos.Count) return null;
        return todosLosGatos[index];
    }

}