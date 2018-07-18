using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class countSpaces : MonoBehaviour {

	//UI hack to keep data presentable in a uniformly spaced table

	public GameObject dataPanel;
	public GameObject rulePanel;

	Canvas dataCanvas;
	Canvas menuCanvas;
	public int n_spaces = 0;

	// Use this for initialization
	void Start () {
		dataCanvas = GameObject.Find("dataTable").GetComponent<Canvas>();
		dataCanvas.enabled = false;
		menuCanvas = GameObject.Find("rules").GetComponent<Canvas>();
		menuCanvas.enabled = false;
	}

	IEnumerator fade(GameObject panel, int sense) {
		Color color = panel.GetComponent<Image>().color;
		color.a = 0;
		if (sense == 1) {
			while (true) {
				color.a += 0.01f;
				panel.GetComponent<Image> ().color = color;
				if (color.a > 20)
					break ;
				yield return new WaitForSeconds (0.03f);
			}
		}
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.Space)) {
			n_spaces += 1;
			if (n_spaces == 1) {
				dataCanvas.enabled = true;
				StartCoroutine (fade (dataPanel, 1));
			}
			if (n_spaces == 2) {
				dataCanvas.enabled = false;
				menuCanvas.enabled = true;
			}
			if (n_spaces == 3) {
				menuCanvas.enabled = false;
				n_spaces = 0;
			}
		}
	}
}
