/************************************************
 * Created by:  Danjel Galic
 * 
 * Modified by: -
 ************************************************/
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class HamsterGameManager : MonoBehaviour
{
#if UNITY_EDITOR
    [Help("The lower this value, the faster the simulation.", UnityEditor.MessageType.Info)]
#endif
    [Header("Global Gamespeed"), Range(0f, 2f)]
    [Tooltip("The lower this value, the faster the simulation.")]public float gameSpeed;
    [Header("UI startup settings")]
    public bool displayQuestLog = true;
    public bool displayWorldInformation = true;
    [Header("Save/Load options")]
    public bool saveHamsterInfo = true;
    [ConditionalHide("saveHamsterInfo")] public bool savePlayer = false;
    [ConditionalHide("saveHamsterInfo")] public bool saveEachFrame = false;
    public bool loadStoredInfo = false;
    [Header("Hamster movement")]
    public bool snapHamstersToGrid = true;

    [Header("Inspector show references?")]
    public bool showReferences = true;


    [Header("Hamster Gamemanager references")]
    [Space(50)]
    [ConditionalHide("showReferences", true)] public GameObject hamsterInstance;
    [ConditionalHide("showReferences", true)] public List<Sprite> hamsterSprites = new List<Sprite>();
    [ConditionalHide("showReferences", true)] public List<TileBase> grainSpritesTileBase = new List<TileBase>();
    [ConditionalHide("showReferences", true)] public GameObject heartPrefab;
    [ConditionalHide("showReferences", true)] public GameObject enduranceHeartPrefab;
    [ConditionalHide("showReferences", true)] public GameObject miniHeartPrefab;
    [ConditionalHide("showReferences", true)] public GameObject miniEndurancePrefab;
    [ConditionalHide("showReferences", true)] public List<Sprite> healthSprite = new List<Sprite>();
    [ConditionalHide("showReferences", true)] public List<Sprite> enduranceSprite = new List<Sprite>();

    [ConditionalHide("showReferences", true)] public TextMeshProUGUI grainAmountUI;
    [ConditionalHide("showReferences", true)] public TextMeshProUGUI hamsterAmountUI;

    [ConditionalHide("showReferences", true)] public Transform questContent;
    [ConditionalHide("showReferences", true)] public GameObject questContainer;

    [Header("UI")]
    [ConditionalHide("showReferences", true)] public GameObject generalUI;
    [ConditionalHide("showReferences", true)] public TMP_Dropdown questSelector; // part of generalUI
    [ConditionalHide("showReferences", true)] public GameObject dialogueCanvas;
    [ConditionalHide("showReferences", true)] public GameObject tradeCanvas;
    [ConditionalHide("showReferences", true)] public GameObject inventoryCanvas;

    [Header("Inventory")]
    [ConditionalHide("showReferences", true)] public GameObject itemPrefab;
    [ConditionalHide("showReferences", true)] public Transform itemContent;

    [ConditionalHide("showReferences", true)] public Image hamsterImage;
    [ConditionalHide("showReferences", true)] public TextMeshProUGUI hamsterName;
    [ConditionalHide("showReferences", true)] public TextMeshProUGUI hamsterGrains;

    [ConditionalHide("showReferences", true)] public Transform hamsterHealthPoints;
    [ConditionalHide("showReferences", true)] public Transform hamsterEndurancePoints;

    [ConditionalHide("showReferences", true)] public Transform Equipment;

    [Header("Trading (Hamster 1)")]
    [ConditionalHide("showReferences", true)] public Transform tradeItemContentHamster1;
    [ConditionalHide("showReferences", true)] public Image tradeHamster1Image;
    [ConditionalHide("showReferences", true)] public TextMeshProUGUI tradeHamster1Name;
    [ConditionalHide("showReferences", true)] public TextMeshProUGUI tradeHamster1Grains;

    [Header("Trading (Hamster 2)")]
    [ConditionalHide("showReferences", true)] public Transform tradeItemContentHamster2;
    [ConditionalHide("showReferences", true)] public Image tradeHamster2Image;
    [ConditionalHide("showReferences", true)] public TextMeshProUGUI tradeHamster2Name;
    [ConditionalHide("showReferences", true)] public TextMeshProUGUI tradeHamster2Grains;

    [Header("NPC Hamsters")]
    [ConditionalHide("showReferences", true)] public Transform npcHamsterCollection;
    [ConditionalHide("showReferences", true)] public Transform hamsterCollection;

    [Header("Dialogue")]
    [ConditionalHide("showReferences", true)] public TextMeshProUGUI npcHamsterNameDialogue;
    [ConditionalHide("showReferences", true)] public TextMeshProUGUI npcHamsterSentenceDialogue;
    [ConditionalHide("showReferences", true)] public Image npcHamsterImageDialogue;

    public static Hamster hamster1;
    public static Hamster hamster2;

    public static bool isTrading = false;
    public static bool isTalking = false;

    public static float hamsterGameSpeed;

    private void Awake()
    {
        hamsterGameSpeed = gameSpeed;
    }

    private void Start()
    {
        List<Quest> quests = this.GetComponent<QuestCollection>().quests;

        foreach (Quest quest in quests)
        {
            quest.questStarted = false;
            quest.questDone = false;
            quest.questFailed = false;
            foreach (StageInfo _stageInfo in quest.stageInfos)
            {
                _stageInfo.isActive = false;
                _stageInfo.isDone = false;
                _stageInfo.failed = false;
            }

            if (quest.startQuestOnStartup)
            {
                quest.StartQuest();
                /*
                quest.questStarted = true;
                GameObject n_quest = Instantiate(questContainer, questContent);
                foreach (StageInfo stageInfo in quest.stageInfos)
                {
                    if (stageInfo.onStartup)
                    {
                        n_quest.GetComponent<TextMeshProUGUI>().text = stageInfo.stageDescription;
                        stageInfo.isActive = true;
                    }
                }
                */
            }

            this.questSelector.value = this.questSelector.options.Count - 1;
            this.questSelector.value = 0;
        }

        SetCanvasVisibility(generalUI, true);
        SetCanvasVisibility(dialogueCanvas, false);
        SetCanvasVisibility(tradeCanvas, false);
        SetCanvasVisibility(inventoryCanvas, false);

        base.StartCoroutine(EnableQuestConditions());


        SetCanvasVisibility(generalUI.transform.GetChild(2).gameObject, displayQuestLog);
        SetCanvasVisibility(generalUI.transform.GetChild(0).gameObject, displayWorldInformation);
        SetCanvasVisibility(generalUI.transform.GetChild(1).gameObject, displayWorldInformation);
    }

    private IEnumerator EnableQuestConditions()
    {
        yield return new WaitForSeconds(1f);
        List<Quest> quests = this.GetComponent<QuestCollection>().quests;

        foreach (Quest quest in quests)
        {
            if (quest.questStarted)
            {
                foreach (StageInfo info in quest.stageInfos)
                {
                    if (info.isActive && info.condition.displayEndurance)
                    {
                        foreach (Hamster hamster in Territory.activHamsters)
                        {
                            hamster.SetEndurancePoints(info.condition.maxEndurancePoints);
                            hamster.HealEndurance(info.condition.maxEndurancePoints);
                            hamster.SetEnduranceConsumption(true);
                            hamster.DisplayEndurance(true);
                        }
                    }
                }
                
            }
        }
    }

    public void RefreshTradeWindow()
    {
        /* 
         * Remove all items 
         */
        for (int i = 0; i < tradeItemContentHamster1.childCount; i++)
        {
            Destroy(tradeItemContentHamster1.GetChild(i).gameObject);
        }

        for (int i = 0; i < tradeItemContentHamster2.childCount; i++)
        {
            Destroy(tradeItemContentHamster2.GetChild(i).gameObject);
        }


        /* 
         * Add all items into the inventory of "Hamster1" 
         */
        for (int i = 0; i < hamster1.Inventory.Count; i++)
        {
            GameObject itemSlot = Instantiate(this.itemPrefab, this.tradeItemContentHamster1);
            itemSlot.GetComponent<ItemHolder>().item = hamster1.Inventory[i].item;
            itemSlot.GetComponent<ItemHolder>().quantity = hamster1.Inventory[i].quantity;
            itemSlot.GetComponent<ItemHolder>().itemName.SetText(hamster1.Inventory[i].item.Name);
            itemSlot.GetComponent<ItemHolder>().itemBuyPrice.SetText(hamster1.Inventory[i].item.BuyPrice.ToString());
            itemSlot.GetComponent<ItemHolder>().itemSellPrice.SetText(hamster1.Inventory[i].item.SellPrice.ToString());
            itemSlot.GetComponent<ItemHolder>().itemQuantity.SetText(itemSlot.GetComponent<ItemHolder>().quantity + "/" + hamster1.Inventory[i].item.StackAmount.ToString());
            itemSlot.GetComponent<ItemHolder>().itemImage.sprite = hamster1.Inventory[i].item.ItemImage;
            itemSlot.GetComponent<ItemHolder>().item.IsEquipped = hamster1.Inventory[i].item.IsEquipped;
        }

        /* 
         * Add all items into the inventory of "Hamster2"  
         */
        for (int i = 0; i < hamster2.Inventory.Count; i++)
        {
            GameObject itemSlot = Instantiate(this.itemPrefab, this.tradeItemContentHamster2);
            itemSlot.GetComponent<ItemHolder>().item = hamster2.Inventory[i].item;
            itemSlot.GetComponent<ItemHolder>().quantity = hamster2.Inventory[i].quantity;
            itemSlot.GetComponent<ItemHolder>().itemName.SetText(hamster2.Inventory[i].item.Name);
            itemSlot.GetComponent<ItemHolder>().itemBuyPrice.SetText(hamster2.Inventory[i].item.BuyPrice.ToString());
            itemSlot.GetComponent<ItemHolder>().itemSellPrice.SetText(hamster2.Inventory[i].item.SellPrice.ToString());
            itemSlot.GetComponent<ItemHolder>().itemQuantity.SetText(itemSlot.GetComponent<ItemHolder>().quantity + "/" + hamster2.Inventory[i].item.StackAmount.ToString());
            itemSlot.GetComponent<ItemHolder>().itemImage.sprite = hamster2.Inventory[i].item.ItemImage;
            itemSlot.GetComponent<ItemHolder>().item.IsEquipped = hamster2.Inventory[i].item.IsEquipped;
        }
    }

    public void RefreshInventoryWindow()
    {
        Hamster hamster = null;

        /* 
         * Remove all items  
         */
        for (int i = 0; i < tradeItemContentHamster1.childCount; i++)
        {
            Destroy(itemContent.GetChild(i).gameObject);
        }

        /* Finde den Hamster der im inventar ist */
        foreach(Hamster ham in Territory.activHamsters)
        {
            if (ham.IsInInventory)
            {
                hamster = ham;
                break;
            }
        }

        for (int i = 0; i < itemContent.childCount; i++)
        {
            Destroy (itemContent.GetChild(i).gameObject);
        }

        /* F�ge alle items dem Inventar hinzu */
        for (int i = 0; i < hamster.Inventory.Count; i++)
        {
            GameObject itemSlot = Instantiate(this.itemPrefab, this.itemContent);
            itemSlot.GetComponent<ItemHolder>().item = hamster.Inventory[i].item;
            itemSlot.GetComponent<ItemHolder>().quantity = hamster.Inventory[i].quantity;
            itemSlot.GetComponent<ItemHolder>().itemName.SetText(hamster.Inventory[i].item.Name);
            itemSlot.GetComponent<ItemHolder>().itemBuyPrice.SetText(hamster.Inventory[i].item.BuyPrice.ToString());
            itemSlot.GetComponent<ItemHolder>().itemSellPrice.SetText(hamster.Inventory[i].item.SellPrice.ToString());
            itemSlot.GetComponent<ItemHolder>().itemQuantity.SetText(itemSlot.GetComponent<ItemHolder>().quantity + "/" + hamster.Inventory[i].item.StackAmount.ToString());
            itemSlot.GetComponent<ItemHolder>().itemImage.sprite = hamster.Inventory[i].item.ItemImage;
            itemSlot.GetComponent<ItemHolder>().item.IsEquipped = hamster.Inventory[i].item.IsEquipped;

            if (hamster.Inventory[i].item.IsEquipped)
            {
                /*
                switch (itemSlot.GetComponent<ItemHolder>().item.ItemRarity)
                {
                    case Item.Rarity.Normal: itemSlot.GetComponent<ItemHolder>().gameObject.GetComponent<Image>().color = itemSlot.GetComponent<ItemHolder>().item.Normal; break;
                    case Item.Rarity.Rare: itemSlot.GetComponent<ItemHolder>().gameObject.GetComponent<Image>().color = itemSlot.GetComponent<ItemHolder>().item.Rare; break;
                    case Item.Rarity.Epic: itemSlot.GetComponent<ItemHolder>().gameObject.GetComponent<Image>().color = itemSlot.GetComponent<ItemHolder>().item.Epic; break;
                    case Item.Rarity.Legendary: itemSlot.GetComponent<ItemHolder>().gameObject.GetComponent<Image>().color = itemSlot.GetComponent<ItemHolder>().item.Legendary; break;
                    case Item.Rarity.Unique: itemSlot.GetComponent<ItemHolder>().gameObject.GetComponent<Image>().color = itemSlot.GetComponent<ItemHolder>().item.Unique; break;
                    default: break;
                }
                */
                itemSlot.GetComponent<ItemHolder>().gameObject.GetComponent<Image>().color = itemSlot.GetComponent<ItemHolder>().EquipColor;
            }
        }

        /* Aktualisiere die Herzen f�r den Hamster */
        for (int i = 0; i < hamsterHealthPoints.childCount; i++)
        {
            if (i < hamster.HealthPointsFull)
            {
                hamsterHealthPoints.GetChild(i).GetComponent<Image>().sprite = healthSprite[0];
            }
            else
            {
                hamsterHealthPoints.GetChild(i).GetComponent<Image>().sprite = healthSprite[1];
            }
        }

        /* Aktualisiere die Herzen der Ausdauer f�r den Hamster */
        for (int i = 0; i < hamsterEndurancePoints.childCount; i++)
        {
            if (i < hamster.EndurancePointsFull)
            {
                hamsterEndurancePoints.GetChild(i).GetComponent<Image>().sprite = enduranceSprite[0];
            }
            else
            {
                hamsterEndurancePoints.GetChild(i).GetComponent<Image>().sprite = enduranceSprite[1];
            }
        }
    }

    private void CheckQuestState()
    {
        List<Quest> quests = this.GetComponent<QuestCollection>().quests;

        for (int i = 0; i < quests.Count; i++)
        {
            for (int j = 0; j < questContent.childCount; j++)
            {
                foreach (StageInfo info in quests[i].stageInfos)
                {
                    if (info.isDone && 
                        string.Compare(questContent.GetChild(j).GetComponent<TextMeshProUGUI>().text, info.stageDescription) == 0)
                    {
                        questContent.GetChild(j).GetChild(0).GetChild(0).gameObject.SetActive(true);
                    }
                    else if (info.failed &&
                        string.Compare(questContent.GetChild(j).GetComponent<TextMeshProUGUI>().text, info.stageDescription) == 0)
                    {
                        questContent.GetChild(j).GetChild(0).GetChild(1).gameObject.SetActive(true);
                    }

                    if (info.isDone && (info.questDone || info.questFailed))
                    {
                        quests[i].questDone = true;
                    }
                }
            }
            /*
            if (quests[i].questDone)
            {
                for (int j = 0; j < questContent.childCount; j++)
                {
                    foreach (StageInfo info in quests[i].stageInfos)
                    {
                        if (string.Compare(questContent.GetChild(j).GetComponent<TextMeshProUGUI>().text, info.stageDescription) == 0)
                        {
                            questContent.GetChild(j).GetChild(0).GetChild(0).gameObject.SetActive(true);
                        }
                    }
                }
            }
            */
        }
    }

    public bool GetCanvasVisibility(Canvas canvas)
    {
        return canvas.GetComponent<CanvasGroup>().alpha == 1;
    }

    public bool GetCanvasVisibility(GameObject canvas)
    {
        return canvas.GetComponent<CanvasGroup>().alpha == 1;
    }

    /// <summary>
    /// Aktiviere/Deaktiviere einen <paramref name="canvas"/>
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="b"></param>
    public void SetCanvasVisibility(GameObject canvas, bool b, float alpha = -1)
    {
        if (alpha == -1)
            canvas.GetComponent<CanvasGroup>().alpha = b ? 1 : 0;
        else
            canvas.GetComponent<CanvasGroup>().alpha = alpha;

        canvas.GetComponent<CanvasGroup>().interactable = b;
        canvas.GetComponent<CanvasGroup>().blocksRaycasts = b;
    }

    /// <summary>
    /// Aktiviere/Deaktiviere einen <paramref name="canvas"/>
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="b"></param>
    public void SetCanvasVisibility(Canvas canvas, bool b, float alpha = -1)
    {
        if (alpha == -1)
            canvas.GetComponent<CanvasGroup>().alpha = b ? 1 : 0;
        else
            canvas.GetComponent<CanvasGroup>().alpha = alpha;

        canvas.GetComponent<CanvasGroup>().alpha = b ? 1 : 0;
        canvas.GetComponent<CanvasGroup>().interactable = b;
        canvas.GetComponent<CanvasGroup>().blocksRaycasts = b;
    }

    private void Update()
    {
        CheckQuestState();
        hamsterGameSpeed = gameSpeed;
    }
}