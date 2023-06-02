using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

namespace Manager
{
    public class MenuManager
    {
        private Dictionary<string, GameObject> _menus;


        public MenuManager()
        {
            _menus = new Dictionary<string, GameObject>();
        }

        public void Initial()
        {
            if (!PhotonNetwork.IsConnected)
            {
                foreach (var t in _menus)
                {
                    t.Value.SetActive(t.Key == "StartMenu");
                }
            }
            else
            {
                foreach (var t in _menus)
                {
                    t.Value.SetActive(t.Key == "TitleMenu");
                }
            }
        }

        public void OpenMenu(string menuName)
        {
            if (_menus[menuName] == null) return;
            _menus[menuName].SetActive(true);
            foreach (var t in _menus.Where(t => t.Key != menuName))
            {
                t.Value.SetActive(false);
            }
        }

        public void OpenAllMenus()
        {
            foreach (var menu in _menus)
            {
                menu.Value.SetActive(true);
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