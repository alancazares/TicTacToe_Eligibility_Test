using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public enum TicTacToeState{none, cross, circle}

[System.Serializable]
public class WinnerEvent : UnityEvent<int>
{
}
public class TicTacToeAI : MonoBehaviour
{

	int _aiLevel;

	TicTacToeState[,] boardState;

	[SerializeField]
	private bool _isPlayerTurn;

	[SerializeField]
	private int _gridSize = 3;
	
	[SerializeField]
	private TicTacToeState playerState = TicTacToeState.cross;
	[SerializeField]
	TicTacToeState aiState = TicTacToeState.circle;
	TicTacToeState currentState;

	[SerializeField]
	private GameObject _xPrefab;

	[SerializeField]
	private GameObject _oPrefab;

	public UnityEvent onGameStarted;

	//Call This event with the player number to denote the winner
	public WinnerEvent onPlayerWin;

	ClickTrigger[,] _triggers;

	//some helper variables
	bool declaredWinner;
	bool firstmove;
	int strategy;
	int moveCount;
	int movex;
	int movey;
	int aimovex;
	int aimovey;
	
	private void Awake()
	{
		if(onPlayerWin == null){
			onPlayerWin = new WinnerEvent();
		}
	}

	public void StartAI(int AILevel){
		_aiLevel = AILevel; //easy is 0 and hard is 1
		StartGame();
	}

	public void RegisterTransform(int myCoordX, int myCoordY, ClickTrigger clickTrigger)
	{
		_triggers[myCoordX, myCoordY] = clickTrigger;
	}

	private void StartGame()
	{
		declaredWinner = false;
		strategy = 0;
		moveCount = 0;

		_triggers = new ClickTrigger[3,3];
		boardState = new TicTacToeState[3,3];
		
		onGameStarted.Invoke();
	}

	public void PlayerSelects(int coordX, int coordY){
		movex = coordX;
		movey = coordY;
        if (_isPlayerTurn)
        {
			_isPlayerTurn = false;
			SetVisual(coordX, coordY, playerState);
			boardState[coordX, coordY] = playerState;
			_triggers[coordX, coordY].canClick = false;
			currentState = playerState;
			CheckWin(currentState);
			if(_aiLevel == 0)
            {
				Invoke(nameof(AIPickRandom), 1f);
            }
            else
            {
				Invoke(nameof(AIPickSmart), .1f);
			}
			
		}
	}

	public void AiSelects(int coordX, int coordY){
		aimovex = coordX;
		aimovex = coordY;
		SetVisual(coordX, coordY, aiState);
		boardState[coordX, coordY] = aiState;
		_triggers[coordX, coordY].canClick = false;
		currentState = aiState;
		CheckWin(currentState);
		_isPlayerTurn = true;
	}

	private void SetVisual(int coordX, int coordY, TicTacToeState targetState)
	{
		Instantiate(
			targetState == TicTacToeState.circle ? _oPrefab : _xPrefab,
			_triggers[coordX, coordY].transform.position,
			Quaternion.identity
		);
		
	}


	private void AIPickRandom()
    {
		List<ClickTrigger> _availableTriggers = CountAvailableTriggers();
		if (_availableTriggers.Count > 0)
        {
			int index = Random.Range(0, _availableTriggers.Count);
			//maybe I should create a method to get these coordinates instead of making them public
			int x = _availableTriggers[index]._myCoordX;
			int y = _availableTriggers[index]._myCoordY;
			AiSelects(x, y);
        }
	}

	/*Naive implementation of some logical moves and blocks for Player depending on Player's first move
	 * the AI will Select for the first 2 moves based on 3 strategies, if Player picks ((1)middle, (2)side or (3)corner)
	 */
	private void AIPickSmart() 
	{
		List<ClickTrigger> _availableTriggers = CountAvailableTriggers();
		//Find out Player's first move
		if (strategy == 0)
		{
			//Check if User picked the middle
			if (boardState[1, 1] == playerState) { strategy = 1; }

			//Check if user picked a side
			if (boardState[0, 1] == playerState || boardState[1, 2] == playerState || boardState[2, 1] == playerState || boardState[1, 0] == playerState) { strategy = 2; }

			//Check if user picked a corner
			if (boardState[0, 0] == playerState || boardState[0, 2] == playerState || boardState[2, 2] == playerState || boardState[2, 0] == playerState) { strategy = 3; }
		}

		if (strategy == 1)
		{
			//select a random corner
			if (moveCount == 0)
			{
				SelectRandomCorner();
				moveCount++;
				return;

			}
			if (moveCount == 1)
			{
				//BlockCorner Move
				if (movex == 0 && movey == 0) { if (boardState[2, 2] == aiState) { AIPickRandom(); } else { AiSelects(2, 2); } }

				if (movex == 0 && movey == 2) { if (boardState[2, 0] == aiState) { AIPickRandom(); } else { AiSelects(2, 0); } }
				if (movex == 2 && movey == 2) { if (boardState[0, 0] == aiState) { AIPickRandom(); } else { AiSelects(0, 0); } }
				if (movex == 2 && movey == 0) { if (boardState[0, 2] == aiState) { AIPickRandom(); } else { AiSelects(0, 2); } }

				//BlockSide Move
				if (movex == 1 && movey == 0) AiSelects(1, 2);
				if (movex == 0 && movey == 1) AiSelects(2, 1);
				if (movex == 1 && movey == 2) AiSelects(1, 0);
				if (movex == 2 && movey == 1) AiSelects(0, 1);
				moveCount++;
				return;
			}
			if (moveCount > 1)
			{
				AIPickRandom();
			}
		}

		if (strategy == 2)
		{
			//select a random corner
			if (moveCount == 0)
			{
				SelectRandomCorner();
				moveCount++;
				return;

			}
			if (moveCount == 1)
			{ //if player selects side Block the middle
				if (movex == 1 && movey == 0 || movex == 0 && movey == 1 || movex == 1 && movey == 2 || movex == 2 && movey == 1) AiSelects(1, 1);
				Debug.Log(moveCount);

				//if player selects the midle
				if (movex == 1 && movey == 1)
				{
					if (boardState[0, 1] == playerState && boardState[1, 1] == playerState) AiSelects(2, 1);
					if (boardState[1, 2] == playerState && boardState[1, 1] == playerState) AiSelects(1, 0);
					if (boardState[2, 1] == playerState && boardState[1, 1] == playerState) AiSelects(0, 1);
					if (boardState[1, 0] == playerState && boardState[1, 1] == playerState) AiSelects(1, 2);
				}

				moveCount++;
				return;
			}

			if (moveCount > 1)
			{
				//don't mean to make an unbeatable so I randomize following moves
				AIPickRandom();
			}
		}

		if (strategy == 3)
		{
			//select a random Side
			if (moveCount == 0)
			{
				int index = Random.Range(0, 4);
				switch (index)
				{
					case 0:
						AiSelects(0, 1);
						break;
					case 1:
						AiSelects(1, 2);
						break;
					case 2:
						AiSelects(2, 1);
						break;
					case 3:
						AiSelects(1, 0);
						break;
				}
				moveCount++;
				return;
			}
			if (moveCount == 1)
			{
				if (movex == 1 && movey == 1)
				{
					if (boardState[0, 0] == playerState) AiSelects(2, 2);
					if (boardState[0, 2] == playerState) AiSelects(2, 0);
					if (boardState[2, 2] == playerState) AiSelects(0, 0);
					if (boardState[2, 0] == playerState) AiSelects(0, 2);
				}
                if(movex == 0 && movey == 1|| movex == 1 && movey == 2 || movex == 2 && movey == 1 || movex == 1 && movey == 0)
                {
					
                    //if selects a side
                    if (boardState[0, 0] == playerState && movex == 0 && movey == 1) AiSelects(0,2);
					if (boardState[0, 0] == playerState && movex == 1 && movey == 0) AiSelects(2, 0);

					if (boardState[0, 2] == playerState && movex == 1 && movey == 2) AiSelects(2, 2);
					if (boardState[0, 2] == playerState && movex == 0 && movey == 1) AiSelects(0, 0);

					if (boardState[2, 2] == playerState && movex == 1 && movey == 2) AiSelects(0, 2);
					if (boardState[2, 2] == playerState && movex == 2 && movey == 1) AiSelects(2, 0);

					if (boardState[2, 0] == playerState && movex == 1 && movey == 0) AiSelects(0, 0);
					if (boardState[2, 0] == playerState && movex == 2 && movey == 1) AiSelects(2, 2);
				}
                //If selects another corner
                if (movex == 0 && movey == 0 || movex == 0 && movey == 2 || movex == 2 && movey == 2 || movex == 2 && movey == 0)
                {
					AiSelects(1, 1);
				}
				moveCount++;
				return;
			}
			if (moveCount > 1)
			{
				AIPickRandom();
			}
		}
	}

    private void SelectRandomCorner()
    {
        int[] choices = new int[] { 0, 2 };
        int i = Random.Range(0, choices.Length);
        int j = Random.Range(0, choices.Length);
        int randomX = choices[i];
        int randomY = choices[j];
        AiSelects(randomX, randomY);
    }

	//Need to fix MinMax implementation, I'll keep trying
	private void BestMove()
	{
		float bestScore = Mathf.NegativeInfinity;
		int coordx =0;
		int coordy =0;

		for (int i = 0; i< 3; i++){
			for(int j =0; j < 3; j++)
            {
                if (_triggers[i, j].canClick)
                {
					boardState[i, j] = aiState;
					int score = MiniMax(boardState,0, false);
					boardState[i, j] = TicTacToeState.none;

					if (score > bestScore)
                    {
						bestScore = score;
						coordx = i;
						coordy = j;
						//move = new int[];
                    }
				}

            }

        }
		AiSelects(coordx, coordy);
	}

	//Need to fix MinMax implementation, I'll keep trying
	private int MiniMax(TicTacToeState[,] boardState, int depth, bool isMaximizing)
	{
		if (declaredWinner == false)
		{
			int score = 1;
			return score;
		}
		if (isMaximizing)
		{
			float bestScore = Mathf.NegativeInfinity;
			//int coordx = 0;
			//int coordy = 0;
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					if (_triggers[i, j].canClick)
					{
						boardState[i, j] = playerState;
						int score = MiniMax(boardState, depth+1, false);
						boardState[i, j] = TicTacToeState.none;
						if (score > bestScore)
						{
							bestScore = Math.Max(score, bestScore);
							//move = new int[];
						}
					}
				}
			}
			///AiSelects(coordx, coordy);
			return (int)bestScore;
		}
        else
        {
			float bestScore = Mathf.Infinity;
			//int coordx = 0;
			//int coordy = 0;
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					if (_triggers[i, j].canClick)
					{
						boardState[i, j] = playerState;
						int score = MiniMax(boardState, depth + 1, false);
						boardState[i, j] = TicTacToeState.none;
						if (score > bestScore)
						{
							bestScore = Mathf.Min(score, bestScore);
							//move = new int[];
						}
					}
				}
			}
			//AiSelects(coordx, coordy);
			return (int)bestScore;
		}
	}

    private List<ClickTrigger> CountAvailableTriggers()
    {
		List<ClickTrigger> availableTriggers = new List<ClickTrigger>();
		foreach (ClickTrigger t in _triggers)
		{
			if (t.canClick == true)
			{
				availableTriggers.Add(t);
			}
		}
		return availableTriggers;
	}

	private void CheckWin(TicTacToeState currentState)
    {
		List<ClickTrigger> _availableTriggers = CountAvailableTriggers();

		//Check Vertical Results
		for (int i =0; i <3; i++)
        {
			if(boardState[0,i] == currentState && boardState[1,i]== currentState && boardState[2,i] == currentState)
            {
				declaredWinner = true;
				DeclareWinner();
				
			}
        }

		//Check Horizontal Results
		for (int i = 0; i < 3; i++)
		{
			if (boardState[i, 0] == currentState && boardState[i, 1] == currentState && boardState[i, 2] == currentState)
			{
				DeclareWinner();
				declaredWinner = true;
				
			}
		}
		//Check for Cross Results
		if (boardState[0, 0] == currentState && boardState[1, 1] == currentState && boardState[2, 2] == currentState)
		{
			DeclareWinner();
			declaredWinner = true;
			
		}
		if (boardState[0, 2] == currentState && boardState[1, 1] == currentState && boardState[2, 0] == currentState)
		{
			DeclareWinner();
			declaredWinner = true;
			
		}
		//Check for Tie
		if (_availableTriggers.Count == 0 && declaredWinner == false)
		{
			Debug.Log(declaredWinner);
			Debug.Log(_availableTriggers.Count);
			onPlayerWin.Invoke(-1);
			return;
		}
	}

	private void DeclareWinner()
    {
		if(currentState == playerState) onPlayerWin.Invoke(0);
		if(currentState == aiState) onPlayerWin.Invoke(1);	
	}
}
