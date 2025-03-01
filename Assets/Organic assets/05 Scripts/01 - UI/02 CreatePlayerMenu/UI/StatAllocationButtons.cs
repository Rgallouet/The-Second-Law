﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class StatAllocationButtons : MonoBehaviour {

    public DataBaseManager dataBaseManager;
    private ArrayList refData = new();
    private ArrayList RefErrors = new();


    public MenuGUI menuGUI;

	public Canvas StatAllocationMenu;
    public GameObject DisplayStatArea;

    public Text[] StatName = new Text[22];

    public Text[] Stat = new Text[22];
	public Text[] PointsToAlloc = new Text[3];

	public Button[] StatPlus = new Button[22];
	public Button[] StatMinus = new Button[22];

	private Text DescriptionHead;
	private Text DescriptionBody;





    void Start(){

        refData = dataBaseManager.getArrayData("select * from REF_StatsDescription order by StatId asc");
        RefErrors = dataBaseManager.getArrayData("select * from REF_Dialogues where Context='Errors' order by DialogueId asc");




        //Find the stats name
        for (int i = 0; i < 22; i++) { StatName[i] = StatAllocationMenu.GetComponentsInChildren<Button>()[i].GetComponentInChildren<Text>(); }
        
        //Find the Minus buttons for each stat to update visible/invisible property
        for (int i = 0; i < 22; i++) { StatMinus[i] = StatAllocationMenu.GetComponentsInChildren<Button>()[i + 22]; }

        // Find the Plus buttons for each stat to update visibile/invisible property
        for (int i = 0; i < 22; i++) { StatPlus[i] = StatAllocationMenu.GetComponentsInChildren<Button>()[i + 44]; }


        // Find the stats display areas to update numbers when used push buttons
        for (int i=0; i<22; i++)	{ Stat[i] = StatAllocationMenu.GetComponentsInChildren<Text>()[i+54];}
		for (int i=0; i<3; i++) 	{ PointsToAlloc[i] = StatAllocationMenu.GetComponentsInChildren<Text>()[i + 76]; }




        // Fill what can be filled
        for (int i = 0; i < 22; i++) { StatName[i].text = (string)((ArrayList)refData[i+1])[1]; }

        DescriptionHead = DisplayStatArea.GetComponentsInChildren<Text> ()[0];
		DescriptionBody = DisplayStatArea.GetComponentsInChildren<Text> ()[1];

		StatAllocationMenu.enabled = false;

	}


    // Interaction with UI Stats Buttons
    public void DisplayStat(int WhichButton)
    {
    DescriptionHead.text = (string)((ArrayList)refData[WhichButton+1])[1] + " Description";
    DescriptionBody.text = "<i>" + (string)((ArrayList)refData[WhichButton+1])[5] + "</i> \n\n" + (string)((ArrayList)refData[WhichButton + 1])[4]; 
    }

    
    public void CallPlusStat(int Stat_ID){

        CallStatAllocationMoveStat (true, Stat_ID);
        DisplayStat(Stat_ID);
    }

	public void CallMinusStat(int Stat_ID){
		CallStatAllocationMoveStat (false, Stat_ID);
        DisplayStat(Stat_ID);
    }

    public void CallStatAllocationMoveStat(bool Move, int Stat_ID)
    {
        menuGUI.statAllocation.MoveStat(Move, Stat_ID);
        menuGUI.statAllocation.RefreshDisplayedStatsNumbers();
        menuGUI.statAllocation.CalculateDisplayPlusMinusButtons();
    }


    public void Next()
    {
        
        if (menuGUI.statAllocation.readyForNext==true)
        {
            menuGUI.MenuGoNext(0);
            StatAllocationMenu.enabled = false;
        }
        else
        {
            menuGUI.dialogue.UpdateDialogue(255, (string)((ArrayList)RefErrors[2])[2], (string)((ArrayList)RefErrors[2])[3]);
        }

    }

    public void Back ()
    {

        if (menuGUI.WasPredefinedPath == true)
        { menuGUI.MenuGoBack(1); }
        else { menuGUI.MenuGoBack(2); }
        StatAllocationMenu.enabled = false;

    }

    public void ActivateMenu() {

        StatAllocationMenu.enabled = true;
        menuGUI.currentState = MenuGUI.CreateAPlayerStates.STATALLOCATION;
        menuGUI.statAllocation.DisplayStatAllocationModule(menuGUI.lastActionWasNext, menuGUI.startingCharacter);

        // If first game (i.e. no data) or if guided mode, then directly select 5 strengths and 5 luck and pass to the next screen
        if (menuGUI.account.NumberOfDeaths < 2)
        {
            CallPlusStat(0);
            CallPlusStat(0);
            CallPlusStat(0);
            CallPlusStat(0);
            CallPlusStat(0);
            CallPlusStat(19);
            CallPlusStat(19);
            CallPlusStat(19);
            CallPlusStat(19);
            CallPlusStat(19);
            Next();
        }
        else
        {
            menuGUI.dialogue.UpdateDialogue(255, (string)((ArrayList)menuGUI.RefQuestions[3])[2], (string)((ArrayList)menuGUI.RefQuestions[3])[3]);

        }


    }




}
