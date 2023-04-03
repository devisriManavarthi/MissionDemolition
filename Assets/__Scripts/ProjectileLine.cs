using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLine : MonoBehaviour {
	static public ProjectileLine S; // одиночка

	[Header("Set in Inspector")]
	public float minDist = 0.1f;

	private LineRenderer line;
	private GameObject _poi;
	private List<Vector3> points;

	void Awake() {
		S = this; // установить ссылку на объект-одиночку
		// получить ссылку на LineRenderer
		line = GetComponent<LineRenderer>();
		// выключить LineRenderer, пока он не понадобится
		line.enabled = false;
		// инициализировать список точек
		points = new List<Vector3>();
	}

	// это свойство (то есть метод, маскирующийся под поле)
	public GameObject poi {
		get {
			return( _poi );
		}
		set {
			_poi = value;
			if (_poi != null) {
				//если поле _poi содержит действительную ссылку, сбросить все остальные параметры в исходное состояние
				line.enabled = false;
				points = new List<Vector3> ();
				AddPoint ();
			}
		}
	}
		
	// этот метод можно вызвать непосредственно, чтобы стереть линию
	public void Clear() {
		_poi = null;
		line.enabled = false;
		points = new List<Vector3> ();
	}

	public void AddPoint() {
		// вызывается для добавления точки в линии
		Vector3 pt = _poi.transform.position;
		if (points.Count > 0 && (pt - lastPoint).magnitude < minDist) {
			// если точка недостаточно далека от предыдущей, просто выйти
			return;
		}
		if (points.Count == 0) { // если это точка запуска...
			Vector3 launchPosDiff = pt - Slingshot.LAUNCH_POS; // для определения добавить дополнительный фрагмент линии, что помочь лучше прицелиться в будущем
			points.Add (pt + launchPosDiff);
			points.Add (pt);
			line.positionCount = 2;
			// установить первые две точки
			line.SetPosition (0, points [0]);
			line.SetPosition (1, points [1]);
			// включить LineRenderer
			line.enabled = true;
		} else {
			// обычная последовательность добавления точки
			points.Add( pt );
			line.positionCount = points.Count;
			line.SetPosition (points.Count - 1, lastPoint);
			line.enabled = true;
		}
	}

	// возвращает местоположение последней добавленной точки
	public Vector3 lastPoint {
		get {
			if (points == null) {
				// если точек нет, вернуть Vector3.zero
				return( Vector3.zero );
			}
			return (points [points.Count - 1]);
		}
	}

	void FixedUpdate () {
		if (poi == null) {
			// если свойство poi содержит пустое значение, найти интересующий объект
			if (FollowCam.POI != null) {
				if (FollowCam.POI.tag == "Projectile" || FollowCam.POI.tag == "Spear") {
					poi = FollowCam.POI;
				} else {
					return; // выйти, если интересующий объект не найден
				}
			} else {
				return; // выйти, если интересующий объект не найден
			}
		}
		// если интересующий объект найден, попытаться добавить точку с его координатами в каждом FixedUpdate
		AddPoint();
		if (FollowCam.POI == null) {
			// если FolloeCam.POI содержит null, записать null в poi
			poi = null;
		}
	}
}
