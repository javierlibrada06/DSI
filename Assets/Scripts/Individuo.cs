using System;
using UnityEngine;

namespace DSI.Lab5
{
    [Serializable]
    public class Individuo
    {
        public string nombre;
        public string apellido;
        public Sprite foto;

        public Individuo(string n, string a, Sprite f)
        {
            nombre = n;
            apellido = a;
            foto = f;
        }
    }
}