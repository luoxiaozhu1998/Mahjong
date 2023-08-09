using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

namespace Manager
{
    public class MenuManager
    {
        private readonly Dictionary<string, GameObject> _menus = new();


        public void OpenMenu(string menuName)
        {
            if (_menus[menuName] == null) return;
            _menus[menuName].SetActive(true);
            foreach (var t in _menus.Where(t => t.Key != menuName))
            {
                t.Value.SetActive(false);
            }
        }

        public void CloseMenu(string menuName)
        {
            _menus[menuName].SetActive(false);
        }

        public void AddMenu(string name, GameObject go)
        {
            _menus[name] = go;
        }
    }
}