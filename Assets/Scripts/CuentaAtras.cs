using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// Gestiona una cuenta atrás antes de iniciar el juego.
public class CuentaAtras : MonoBehaviour
{
    [SerializeField] private GameObject panelCuentaAtras; // Panel que muestra la cuenta atrás.
    [SerializeField] private TextMeshProUGUI textoCuenta; // Texto de los números.

    public System.Action OnFinCuentaAtras; // Evento que se ejecuta al finalizar.

    void Start()
    {
        // Inicia la cuenta atrás al comenzar.
        StartCoroutine(IniciarCuenta());
    }

    // Muestra 3, 2, 1 y ¡YA! con un segundo de intervalo.
    private IEnumerator IniciarCuenta()
    {
        panelCuentaAtras?.SetActive(true);

        string[] numeros = { "3", "2", "1", "¡YA!" };

        foreach (string n in numeros)
        {
            if (textoCuenta != null)
                textoCuenta.text = n;

            yield return new WaitForSeconds(1f);
        }

        // Oculta el panel y avisa de que la cuenta atrás terminó.
        panelCuentaAtras?.SetActive(false);
        OnFinCuentaAtras?.Invoke();
    }
}