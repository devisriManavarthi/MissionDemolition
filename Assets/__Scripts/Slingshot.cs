using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slingshot : MonoBehaviour {
	static private Slingshot S;
	// поля, устанавливаемые в инспекторе Unity
	[Header("Set in Inspector")]
	public GameObject prefabProjectile;
	public GameObject spearPrefab;
	public float velocityMult = 8f;

	//поля, устанавливаемые динамически
	[Header("Set Dynamically")]
	public GameObject launchPoint;
	public Vector3 launchPos;
	public GameObject projectile;
	public bool aimingMode;

	private Rigidbody projectileRigidbody;

	static public Vector3 LAUNCH_POS {
		get {
			if (S == null) return Vector3.zero;
				return S.launchPos;
		}
	}

	void Awake() {
		S = this;
		Transform launchPointTrans = transform.Find("LaunchPoint");
		launchPoint = launchPointTrans.gameObject;
		launchPoint.SetActive ( false );
		launchPos = launchPointTrans.position;
	}

		void OnMouseEnter() {
		// print ("Slingshot:OnMouseEnter()");
		launchPoint.SetActive( true );
	}
	
	void OnMouseExit() {
		// print ("Slingshot:OnMouseExit()");
		launchPoint.SetActive( false );
	}

	void OnMouseDown() {
	// игрок нажал кнопку мыши, когда указатель находился над рогаткой
		aimingMode = true;
		// создать снаряд
		projectile = Instantiate(prefabProjectile) as GameObject;
		// поместить в точку launchPoint
		projectile.transform.position = launchPos;
		// сделать его кинематическим
		projectileRigidbody = projectile.GetComponent<Rigidbody>();
		projectileRigidbody.isKinematic = true;
	}
	public void SwitchProjectile()
	{
		if (gameObject.tag != "Spear")
		{
			Destroy(gameObject);
			Debug.Log("Destroyed");

		}
		Debug.Log("ball >> " + projectile);
		// destroy the current projectile and instantiate the new projectile
		Destroy(GameObject.FindGameObjectWithTag("Projectile"));
		projectile = Instantiate(spearPrefab) as GameObject;
		projectile.transform.position = launchPos;
		projectileRigidbody = projectile.GetComponent<Rigidbody>();
		projectileRigidbody.isKinematic = true;

		Debug.Log("ball Destroyed"+ projectile);



	}

	void Update() {
	// если рогатка не в режиме прицеливания, не выполнять этот код
		if (!aimingMode) return;

		// получить текущие экранные координаты указателя мыши
		Vector3 mousePos2D = Input.mousePosition;
		mousePos2D.z = -Camera.main.transform.position.z;
		Vector3 mousePos3D = Camera.main.ScreenToWorldPoint (mousePos2D);

		// найти разность координат между launchPos и mousePos3D
		Vector3 mouseDelta = mousePos3D-launchPos;
		// ограничить mouseDelta радиусом коллайдера объекта Slingshot
		float MaxMagnitude = this.GetComponent<SphereCollider>().radius;
		if (mouseDelta.magnitude > MaxMagnitude) {
			mouseDelta.Normalize ();
			mouseDelta *= MaxMagnitude;
		}

		// передвинуть снаряд в новую позицию
		Vector3 projPos = launchPos + mouseDelta;
		projectile.transform.position = projPos;
		if (Input.GetMouseButtonUp (0)) {
			// кнопка мыши отпущена
			aimingMode = false;
			projectileRigidbody.isKinematic = false;
			projectileRigidbody.velocity = -mouseDelta * velocityMult;
			FollowCam.POI = projectile;
			projectile = null;
			MissionDemolition.ShotFired();
			ProjectileLine.S.poi = projectile;
		}
	}
}
