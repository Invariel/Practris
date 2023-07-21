using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.EventSystems;
using Button = UnityEngine.UI.Button;

namespace Assets.Scripts.Resources
{
    public class InputBinding : MonoBehaviour, IPointerDownHandler
    {
        public static SettingsScene _scene;
        public Button _myself;

        public void OnPointerDown(PointerEventData eventData)
        {
            _scene.StartInputBinding(_myself);
        }
    }
}
