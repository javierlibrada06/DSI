using UnityEngine.UIElements;

namespace DSI.Lab5
{
    public class Tarjeta
    {
        VisualElement raiz;

        public Tarjeta(VisualElement elemento)
        {
            raiz = elemento;
        }

        public void Actualizar(Individuo datos)
        {
            raiz.Q<Label>("name").text = datos.nombre;
            raiz.Q<Label>("surname").text = datos.apellido;
            if (datos.foto != null)
                raiz.Q("profilePhoto").style.backgroundImage = new StyleBackground(datos.foto);
        }
    }
}