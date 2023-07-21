using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace Assets.Scripts.Resources
{
    public class ContainerInformation : MonoBehaviour
    {
        public GameObject _myself { get; set; }
        public string _name { get; set; }
        public Constants.SettingsField _settingName { get; set; }

        public TMP_InputField GetInputField() => _myself.transform.Find("input_Keys").GetComponent<TMP_InputField>();
        public Button GetTooltipButton() => _myself.transform.Find("btn_Tooltip").GetComponent<Button>();
        public Button GetBindingButton() => _myself.transform.Find("btn_Bind").GetComponent<Button>();
        public Button GetClearButton() => _myself.transform.Find("btn_Clear").GetComponent<Button>();

        public Transform GetTooltipContainer() => _myself.transform.Find("txt_Tooltip_Container");
        public TMP_Text GetTooltipText() => GetTooltipContainer().GetComponentInChildren<TMP_Text>();
        public Image GetTooltipImage() => GetTooltipContainer().GetComponentInChildren<Image>();
    }
}
