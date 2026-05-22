using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CuentaAtras : MonoBehaviour
{
    [SerializeField] private GameObject panelCuentaAtras;
    [SerializeField] private TextMeshProUGUI textoCuenta;

    public System.Action OnFinCuentaAtras; // callback cuando termina

    void Start()
    {
        StartCoroutine(IniciarCuenta());
    }

    private IEnumerator IniciarCuenta()
    {
        panelCuentaAtras?.SetActive(true);

        string[] numeros = { "3", "2", "1", "¡YA!" };
        foreach (string n in numeros)
        {
            if (textoCuenta != null) textoCuenta.text = n;
            yield return new WaitForSeconds(1f);
        }

        panelCuentaAtras?.SetActive(false);
        OnFinCuentaAtras?.Invoke();
    }
}