using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour {
	// статическое поле, доступное любому другому коду
	static public bool goalMet = false;

	void OnTriggerEnter( Collider other ) {
		// когда в область действия триггера попадает что-то, проверить, является ли это "что-то" снарядом
		if ( other.gameObject.tag == "Projectile" ) {
			// если это снард, присвоить полю goalMet значение true
			Goal.goalMet = true;
			// также изменить альфа-канал цвета, чтобы увеличить непрозрачность
			Material mat = GetComponent<Renderer>().material;
			Color c = mat.color;
			c.a = 1;
			mat.color = c;
		}
	}
}
