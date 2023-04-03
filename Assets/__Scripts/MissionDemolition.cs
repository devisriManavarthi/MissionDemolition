using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public enum GameMode {
	idle,
	playing,
	levelEnd
}

public class Player
{
    //public string Name { get; set; }
    public int Level { get; set; }
    public int Score { get; set; }

    public Player(int level, int score)
    {
        this.Level = level;
        this.Score = score;
    }
}

public class MissionDemolition : MonoBehaviour {
	static private MissionDemolition S; // скрытый объект-одиночка

	static public Slingshot slingshot;

	[Header ("Set in Inspector")]
	public Text uitLevel; //  ссылка на объект UIText_Level
	public Text uitShots; // ссылка на объект UIText_Shots
	public Text uitButton; // ссылка на дочерний объект Text в UIButton_View
	public Vector3 castlePos; // местоположение замка
	public GameObject[] castles; // массив замков

	[Header("Set Dynamically")]
	public int level; // текущий уровень
	public int levelMax; // количество уровней
	public int shotsTaken;
	public GameObject castle; // текущий замок
	public GameMode mode = GameMode.idle;
	public string showing = "Show Slingshot"; // режим FollowCam

	// Countdown timer
	public float timeLimitInSeconds = 60f;
	private float remainingTimeInSeconds;
	public Text countdownTextSeconds;

	// create a score variable and update it for each second
	[Header("Score")]
	private float score = 0f;
	private bool isLevelsCompleted = false;
	public Text scoreboard;

	private int totalScore = 0;
	private string input;
	public Canvas LeaderboardCanvas;

	// top 5 best scores
	List<Player> playerLeaderboard = new List<Player>();

	public GameObject[] projectiles; // array of projectiles
	public int currentProjectileIndex; // current index of the projectile array

	void Start () {
		// countdownTextSeconds.text = CurrentTimeInSeconds.ToString("0");
		LeaderboardCanvas.gameObject.SetActive(false);

		S = this; // определить объект-одиночку

		level = 0;
		levelMax = castles.Length;
		StartLevel ();
	}

	void StartLevel() {
		remainingTimeInSeconds = timeLimitInSeconds; // initialize the remaining time
		// уничтожить прежний замок, если он существует
		if (castle != null) {
			Destroy (castle);
		}

		// уничтожить прежние снаряды, если они существуют
		GameObject[] gos = GameObject.FindGameObjectsWithTag("Projectile");
		foreach (GameObject pTemp in gos) {
			Destroy (pTemp);
		}

		// создать новый замок
		castle = Instantiate<GameObject>( castles[level] );
		castle.transform.position = castlePos;
		shotsTaken = 0;

		// переустановить камеру в начальную позицию
		SwitchView("Show Both");
		ProjectileLine.S.Clear();

		// сбросить цель
		Goal.goalMet = false;
		score = 0;

		UpdateGUI();

		mode = GameMode.playing;

        // instantiate the current projectile
        //GameObject currentProjectile = Instantiate<GameObject>(projectiles[currentProjectileIndex]);
        //currentProjectile.transform.position = Vector3.zero;
        //currentProjectile.GetComponent<Rigidbody>().isKinematic = true;

    }

	void UpdateGUI() {
		// показать данные в элементах пользовательского интерфейса
		uitLevel.text = "Level: "+(level+1)+" of "+levelMax;
		uitShots.text = "Shots Taken: " + shotsTaken;
	}

	void Update () {
		// decrement the remaining time
		if (mode == GameMode.playing && remainingTimeInSeconds > 0f)
		{
			remainingTimeInSeconds -= Time.deltaTime;
			countdownTextSeconds.text = Mathf.CeilToInt(remainingTimeInSeconds).ToString();
		}

		// check if the time limit has been exceeded
		if (mode == GameMode.playing && remainingTimeInSeconds <= 0f)
		{
			// restart the level
			StartLevel();
		}

		UpdateGUI();

		// check level completion
		if ( (mode == GameMode.playing) && Goal.goalMet) {
			Debug.Log("level" + level);
			// change mode to stop level completion check
			mode = GameMode.levelEnd;
			// уменьшить масштаб
			SwitchView("Show Both");
			
			// add the player score 
			int scoreInt = (int)score;
			var playerScore = new Player(
				level: level + 1,
				score: scoreInt
			);
			Debug.Log("level:" + level + ",  " + "score: " + scoreInt + "\n");
			playerLeaderboard.Add(playerScore);
			totalScore = totalScore + scoreInt;

			// начать новый уровень через 2 секунды
			Invoke("NextLevel", 2f);
		}

		// Update score once per second
        if (!isLevelsCompleted) {
            score += Time.deltaTime;
        }

	}

	void NextLevel() {
		// add score to list
		level++;

		if (level == levelMax) {
			level = 0;
			isLevelsCompleted = true;
			LeaderboardCanvas.gameObject.SetActive(true);
		}
		else{
			StartLevel ();
		}
	}

	public void displayLeaderboard()
    {
		Debug.Log("Game finished! Your score is " + score.ToString("0") + " seconds.");

		var topBestScoresByLevel = playerLeaderboard
			.OrderBy(x => x.Score)
			.Take(5);

		string lvl_strings = "\n" + input + " Leaderboard \n\n";
		foreach (var player in topBestScoresByLevel)
		{
			Debug.Log("level:" + player.Level + ",  " + "score: " + player.Score + "\n");
			lvl_strings = lvl_strings + "Level" + player.Level + "  >  " + player.Score + "\n";
		}

		scoreboard.text = "Total Score: " + totalScore.ToString("0") + " sec" + lvl_strings;
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Projectile")
		{
			if (remainingTimeInSeconds > 0f)
			{ // check if the time limit has not been exceeded
				Goal.goalMet = true;
				mode = GameMode.levelEnd;
			}
		}
	}


	public void SwitchView( string eView = "" ) {
		if (eView == "") {
			eView = uitButton.text;
		}
		showing = eView;
		switch (showing) {
		case "Show Slingshot": // ДВОЕТОЧИЕ
			FollowCam.POI = null;
			uitButton.text = "Show Castle";
			break;

		case "Show Castle":
			FollowCam.POI = S.castle;
			uitButton.text = "Show Both";
			break;

		case "Show Both":
			FollowCam.POI = GameObject.Find ("ViewBoth");
			uitButton.text = "Show Slingshot";
			break;
		}
	}

	// статистический метод, позволяющий из любого кода увеличить shotsTaken
	public static void ShotFired() {
		S.shotsTaken++;
	}


	public void ReadStringInput(string s)
	{
		// get the player name from the input field
		input = s;
		Debug.Log(input);

	}

	public void SwitchPrefabProjectile()
	{
		//// increment the current projectile index
		//currentProjectileIndex++;
		//if (currentProjectileIndex >= projectiles.Length)
		//{
		//    currentProjectileIndex = 0;
		//}

		// destroy the current projectile and instantiate the new projectile
		Destroy(GameObject.FindGameObjectWithTag("Projectile"));
		if (gameObject.tag != "Spear") {
			slingshot.projectile = Instantiate(slingshot.spearPrefab) as GameObject;
			slingshot.projectile.transform.position = slingshot.launchPos;
			slingshot.projectile.GetComponent<Rigidbody>().isKinematic = true;
		}
        else
        {
			slingshot.projectile = Instantiate(slingshot.prefabProjectile) as GameObject;
			slingshot.projectile.transform.position = slingshot.launchPos;
			slingshot.projectile.GetComponent<Rigidbody>().isKinematic = true;
		}
			

    }


}
