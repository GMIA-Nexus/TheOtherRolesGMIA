using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TheOtherRoles.Modules;
using TheOtherRoles;
using static TheOtherRoles.TheOtherRoles;
using static TheOtherRoles.TheOtherRolesGM;


namespace TheOtherRoles.Objects
{
    public sealed class FoxTask : Minigame
    {
        private GameObject obj;
        private TextMeshProUGUI RemainingTime;
        private TextMeshProUGUI TaskText;
        private Button closeButton;
        private bool completed = false;
        private float timer = 0;
        public static GameObject prefab;
        public static Sprite shrine;

        public void Awake()
        {
            if (obj != null)
            {
                GameObject.Destroy(obj);
            }
            obj = GameObject.Instantiate(prefab, this.transform);
            List<TextMeshProUGUI> texts = obj.GetComponentsInChildren<TextMeshProUGUI>().ToList();
            RemainingTime = texts.FirstOrDefault(x => x.name == "RemainingTime");
            TaskText = texts.FirstOrDefault(x => x.name == "TaskText");
            TaskText.text = ModTranslation.getString("foxTaskPray");
            closeButton = obj.GetComponentsInChildren<Button>().ToList().FirstOrDefault(x => x.name == "CloseButton");
            closeButton.onClick = new Button.ButtonClickedEvent();
            closeButton.onClick.AddListener((UnityEngine.Events.UnityAction)onClick);
            obj.SetActive(true);
            this.enabled = true;
        }

        private void FixedUpdate()
        {
            if (timer > 0)
            {
                RemainingTime.text = $"{timer:n}秒";
                timer -= Time.fixedDeltaTime;
            }
            else if (!completed)
            {
                completed = true;
                obj.SetActive(false);
                this.MyNormTask.NextStep();
                CustomNormalPlayerTask.completedConsoles.Add(this.ConsoleId);
                if (this.MyNormTask.taskStep < this.MyNormTask.MaxStep)
                {
                    var console = ShipStatus.Instance.AllConsoles.FirstOrDefault(x => x.ConsoleId == this.MyNormTask.Data[this.MyNormTask.taskStep]);
                    this.MyNormTask.StartAt = console.Room;
                }
                base.StartCoroutine(base.CoStartClose(0.5f));
            }
        }

        public void OnEnable()
        {
            this.enabled = true;
            completed = false;
            timer = Fox.stayTime;
        }

        public void OnDisable()
        {
            obj.SetActive(false);
        }

        public void OnDestroy()
        {
        }
        void onClick()
        {
            this.Close();
        }
    }
}
