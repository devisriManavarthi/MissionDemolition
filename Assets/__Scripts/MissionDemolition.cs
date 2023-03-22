using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameMode {
	idle,
	playing,
	levelEnd
}

public class MissionDemolition : MonoBehaviour {
	static private MissionDemolition S; // скрытый объект-одиночка

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

	void Start () {
		S = this; // определить объект-одиночку

		level = 0;
		levelMax = castles.Length;
		StartLevel ();
	}

	void StartLevel() {
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

		UpdateGUI();

		mode = GameMode.playing;
	}

	void UpdateGUI() {
		// показать данные в элементах пользовательского интерфейса
		uitLevel.text = "Level: "+(level+1)+" of "+levelMax;
		uitShots.text = "Shots Taken: " + shotsTaken;
	}

	void Update () {
		UpdateGUI ();

		// проверить завершение уровня
		if ( (mode == GameMode.playing) && Goal.goalMet) {
			// изменить режим, чтобы пркратить проверку завершения уровня
			mode = GameMode.levelEnd;
			// уменьшить масштаб
			SwitchView("Show Both");
			// начать новый уровень через 2 секунды
			Invoke("NextLevel", 2f);
		}
	}

	void NextLevel() {
		level++;
		if (level == levelMax) {
			level = 0;
		}
		StartLevel ();
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
}
