using UnityEngine;
using UnityEngine.UIElements;

public class Lab8 : MonoBehaviour {
    public UIDocument ui;

    Button tab1, tab2, tab3;
    VisualElement content1, content2, content3;
    Label title1;
    TextField s1;

    void Start() {
        var root = ui.rootVisualElement;

        tab1 = root.Q<Button>("tab1");
        tab2 = root.Q<Button>("tab2");
        tab3 = root.Q<Button>("tab3");
        s1 = root.Q<TextField>("edad");
        title1 = root.Q<Label>("Inventario");
        title1.text = @"<line-indent=15%>Acabas de entrar<smallcaps>a tu casa</smallcaps> </line-indent><br>
        llamada <rotate=""45"">casa juliajavi</rotate>,
        <b><gradient=""textoColorido"">hacia mucho que no venias</gradient></b>";
        content1 = root.Q<VisualElement>("content1");
        content2 = root.Q<VisualElement>("content2");
        content3 = root.Q<VisualElement>("content3");

        tab1.clicked += () => ShowTab(1);
        tab2.clicked += () => ShowTab(2);
        tab3.clicked += () => ShowTab(3);
        ShowTab(1);
    }

    void ShowTab(int i) {
        // ocultar contenidos
        content1.style.display = DisplayStyle.None;
        content2.style.display = DisplayStyle.None;
        content3.style.display = DisplayStyle.None;

        // quitar selección
        tab1.RemoveFromClassList("selected");
        tab2.RemoveFromClassList("selected");
        tab3.RemoveFromClassList("selected");

        // activar
        if (i == 1) {
            content1.style.display = DisplayStyle.Flex;
            tab1.AddToClassList("selected");
        }
        if (i == 2) {
            content2.style.display = DisplayStyle.Flex;
            tab2.AddToClassList("selected");
        }
        if (i == 3) {
            content3.style.display = DisplayStyle.Flex;
            tab3.AddToClassList("selected");
        }
    }
}
