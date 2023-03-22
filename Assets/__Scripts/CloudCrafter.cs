using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudCrafter : MonoBehaviour {
	[Header("Set in Inspector")]
	public int numClouds = 40; // число облаков
	public GameObject cloudPrefab; //шаблон для облаков
	public Vector3 cloudPosMin = new Vector3(-50,-5,10);
	public Vector3 cloudPosMax = new Vector3(150,100,10);
	public float cloudScaleMin = 1; // минимальный масштаб каждого облака
	public float cloudScaleMax = 3; // максимальный масштаб каждого облака
	public float cloudSpeedMult = 0.5f; // коэффициент скорости облаков

	private GameObject[] cloudInstances;

	void Awake() {
		// создать массив для хранения всех экземпляров облаков
		cloudInstances = new GameObject[numClouds];
		// найти родительский игровой объект CloudAnchor
		GameObject anchor = GameObject.Find("CloudAnchor");
		// создать в цикде заданное количество облаков
		GameObject cloud;
		for (int i = 0; i < numClouds; i++) {
			// создать экземпляр cloudPrefab
			cloud = Instantiate<GameObject>( cloudPrefab );
			// выбрать местоположение для облака
			Vector3 cPos = Vector3.zero;
			cPos.x = Random.Range (cloudPosMin.x, cloudPosMax.x);
			cPos.y = Random.Range (cloudPosMin.y, cloudPosMax.y);
			// масштабировать облако
			float scaleU = Random.value;
			float scaleVal = Mathf.Lerp (cloudScaleMin, cloudScaleMax, scaleU);
			// меньшие облака (с меньшим значением scaleU) должны быть ближе к земле
			cPos.y = Mathf.Lerp( cloudPosMin.y, cPos.y, scaleU );
			// меньшие облака должны быть дальше
			cPos.z = 100 - 90*scaleU;
			// применить полученные значения  координат и масштаба к облаку
			cloud.transform.position = cPos;
			cloud.transform.localScale = Vector3.one * scaleVal;
			// сделать облако дочерним по отношению к anchor
			cloud.transform.SetParent( anchor.transform );
			// добавить облако в массив cloudInstances
			cloudInstances[i] = cloud;
		}
	}
	
	void Update () {
		// обойти в цикле все созданные облака
		foreach (GameObject cloud in cloudInstances) {
			// получить масштаб и координаты облака
			float scaleVal = cloud.transform.localScale.x;
			Vector3 cPos = cloud.transform.position;
			// увеличить скорость для ближник облаков
			cPos.x -= scaleVal * Time.deltaTime * cloudSpeedMult;
			// если облако сместилось слишком далеко влево...
			if (cPos.x <= cloudPosMin.x) {
				// переместить его далеко вправо
				cPos.x = cloudPosMax.x;
			}
			// применить новые координаты к облаку
			cloud.transform.position = cPos;
		}
	}
}
