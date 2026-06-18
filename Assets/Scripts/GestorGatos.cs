using System.Collections.Generic;
using UnityEngine;

// Gestiona todos los gatos del juego: lista, estado de salvado,
// tablón, bonus pasivos y activacion en escena
public class GestorGatos : MonoBehaviour
{
    public static GestorGatos Instance { get; private set; }

    // Lista con los 3 gatos del juego (ScriptableObjects)
    [Header("Todos los gatos del juego")]
    [SerializeField] private List<DatosGato> todosLosGatos;

    // Prefab del gato que aparece en la cafeteria al ser rescatado
    [Header("Prefab gato salvado")]
    [SerializeField] private GameObject prefabGatoSalvado;

    // Gatos que se muestran actualmente en el tablon
    public List<DatosGato> GatosEnTablon { get; private set; } = new List<DatosGato>();

    [Header("Cuántos gatos mostrar a la vez")]
    [SerializeField] private int minGatosTablon = 1;
    [SerializeField] private int maxGatosTablon = 3;

    // Indice del gato activo en la cafeteria (-1 si no hay ninguno)
    private int indexGatoActivo = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable() { UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnScenaCargada; }
    private void OnDisable() { UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnScenaCargada; }

    private void OnScenaCargada(UnityEngine.SceneManagement.Scene escena, UnityEngine.SceneManagement.LoadSceneMode modo) { }

    // Rellena GatosEnTablon con el primer gato no salvado del flujo
    public void GenerarGatosTablon()
    {
        GatosEnTablon.Clear();

        int[] indicesGato = { 0, 1, 2 };
        foreach (int idx in indicesGato)
        {
            if (idx < todosLosGatos.Count && !todosLosGatos[idx].estaSalvado)
            {
                GatosEnTablon.Add(todosLosGatos[idx]);
                return; // Solo 1 gato a la vez en el tablon
            }
        }
    }

    // Marca un gato como salvado, aplica su bonus y muestra dialogo
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

        // Activar el gato en la escena automaticamente
        PanelGestorGatos panel = FindObjectOfType<PanelGestorGatos>();
        panel?.ActivarUltimoGatoSalvado();
    }

    // Aplica el bonus pasivo del gato al sistema correspondiente
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
        }
    }

    // Devuelve la lista de gatos salvados
    public List<DatosGato> ObtenerGatosSalvados()
    {
        return todosLosGatos.FindAll(g => g != null && g.estaSalvado);
    }

    // Resetea todos los gatos a no salvados (para nueva partida)
    public void ReiniciarGatos()
    {
        foreach (DatosGato gato in todosLosGatos)
            gato.estaSalvado = false;
        indexGatoActivo = -1;
    }

    public DatosGato[] ObtenerTodosLosGatos() { return todosLosGatos.ToArray(); }
    public int ObtenerIndexGatoActivo() { return indexGatoActivo; }

    // Establece el gato activo por indice (para cargar partida)
    public void EstablecerGatoActivo(int index)
    {
        if (index < 0 || index >= todosLosGatos.Count) return;
        indexGatoActivo = index;
    }

    // Salva un gato por su indice y lo registra en la BBDD
    public void SalvarGatoPorIndex(int index)
    {
        if (index < 0 || index >= todosLosGatos.Count) return;
        DatosGato gato = todosLosGatos[index];
        if (gato.estaSalvado) return;
        SalvarGato(gato);
        GestorBBDD.Instance?.GuardarGatoSalvado(index + 1);
    }

    // Devuelve un gato por su indice
    public DatosGato ObtenerGatoPorIndex(int index)
    {
        if (index < 0 || index >= todosLosGatos.Count) return null;
        return todosLosGatos[index];
    }
}