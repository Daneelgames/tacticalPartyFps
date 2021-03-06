using System.Collections;
using System.Collections.Generic;
using System.Net.Configuration;
using MrPink.PlayerSystem;
using MrPink.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace MrPink
{
    public class Shop : MonoBehaviour
    {
        public static Shop Instance;
    
        public List<Tool> toolsList;
        public List<ShopItem> shopItemsIcons;
        public Animator canvasAnim;
        public Text selectedInfoNameText;
        public Text selectedInfoDescriptionText;
        public Text buyForText;
        public Image buyButtonImage;

        private bool isActive = false;
        private int selectedItemIndex = 0;

        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }
        private void Awake()
        {
            Instance = this;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        void Start()
        {
            toolsList = new List<Tool>(ProgressionManager.Instance.CurrentLevel.toolsInShop);
            CloseShop();
        }

        public void SetToolsList(List<Tool> tools)
        {
            toolsList = new List<Tool>(tools);
        }

        public void OpenShop(int newSelectedItem)
        {
            ScoringSystem.Instance.UpdateScore();
            canvasAnim.gameObject.SetActive(true);
            IsActive = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        
            for (int i = 0; i < shopItemsIcons.Count; i++)
            {
                if (i >= toolsList.Count)
                {
                    shopItemsIcons[i].HideItem();
                    continue;
                }
            
                shopItemsIcons[i].ShowItem(toolsList[i].toolName);
                if (toolsList[i].baseCost > ScoringSystem.Instance.CurrentScore)
                    shopItemsIcons[i].raycastedSprite.color = Color.red;
                else
                    shopItemsIcons[i].raycastedSprite.color = Color.white;
            }
            SelectItem(newSelectedItem);
        }

        public void CloseShop()
        {
            Debug.Log("CloseShop");
            canvasAnim.gameObject.SetActive(false);
            IsActive = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Game.Player.ToolControls.Init();    
        }
    
        public void SelectItem(int index)
        {
            // select tool
            selectedItemIndex = index;
            selectedInfoNameText.text = toolsList[selectedItemIndex].toolName;
            selectedInfoDescriptionText.text = toolsList[selectedItemIndex].toolDescription;
            int amount = Game.Player.Inventory.GetAmount(toolsList[selectedItemIndex].tool);
            selectedInfoDescriptionText.text += ". " + amount + " / " + toolsList[selectedItemIndex].maxAmount;
        
            // TODO ?????????????????? ?????????????????? ?????????? ?? ?????????????? ??????????????????????
        
            if (!Game.Player.Inventory.CanFitTool(toolsList[selectedItemIndex]))
            {
                buyForText.text = "Max Amount";
                buyButtonImage.color = Color.red;
            }
            else
            {
                if (toolsList[selectedItemIndex].baseCost > ScoringSystem.Instance.CurrentScore ||
                    Game.Player.Inventory.CanFitTool(toolsList[selectedItemIndex]) == false)
                {
                    buyForText.text = "Not enough DOLAS";   
                    buyButtonImage.color = Color.red;
                }
                else
                {
                    buyForText.text = "F: Buy for " + toolsList[selectedItemIndex].baseCost + " DOLAS";
                    buyButtonImage.color = Color.green;
                }
            }

            buyForText.text = buyForText.text.ToUpper();
            selectedInfoNameText.text = selectedInfoNameText.text.ToUpper();
            selectedInfoDescriptionText.text = selectedInfoDescriptionText.text.ToUpper(); 

        }

        public void BuyItem()
        {
            // buy selectedItemIndex item
            if (toolsList[selectedItemIndex].baseCost > ScoringSystem.Instance.CurrentScore)
                return;
            if (!Game.Player.Inventory.CanFitTool(toolsList[selectedItemIndex]))
                return;
            Game.Player.Inventory.AddTool(toolsList[selectedItemIndex]);
        
            ScoringSystem.Instance.RemoveScore(toolsList[selectedItemIndex].baseCost);
            OpenShop(selectedItemIndex);
        }
    }
}