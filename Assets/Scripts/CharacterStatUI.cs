using UnityEngine;
using UnityEngine.UIElements;

public class CharacterStatUI : MonoBehaviour
{
    //public VisualTreeAsset template;
    [SerializeField] string iconoDefensa;
    [SerializeField] string iconoAtaque;
    [SerializeField] int Ataque;
    [SerializeField] int Defensa;

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        var ataque = new CharacterStat();
        ataque.Valor = Ataque;
        ataque.Icono = iconoAtaque;
        ataque.Name = "Ataque";

        root.Add(ataque);

        var defensa = new CharacterStat();
        defensa.Valor = Defensa;
        defensa.Icono = iconoDefensa;
        defensa.Name = "Defensa";


        root.Add(defensa);
    }
}