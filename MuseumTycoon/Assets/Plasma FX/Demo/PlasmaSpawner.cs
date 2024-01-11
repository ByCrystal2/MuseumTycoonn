using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;


public class PlasmaSpawner : MonoBehaviour {
	public GameObject[] gameObjects;			//gameObjects to spawn (used to only be particle systems aka var naming)
	public int maxButtons = 10;					//Maximum buttons per page	
	public bool spawnOnAwake = true;			//Instantiate the first model on start
	public string removeTextFromButton;			//Unwanted text 
	public float autoChangeDelay;

	int page = 0;								//Current page
	int pages;									//Number of pages
	GameObject currentGO;						//GameObject currently on stage
	Color currentColor;
	bool isPS;									//Toggle to check if this is a PS or a GO
	bool _active = true;
	int counter = -1;
	public GUIStyle bigStyle;


	public void Start() {
		gameObjects = gameObjects.OrderBy(go => go.name).ToArray();

		pages = (int)Mathf.Ceil((float)((gameObjects.Length - 1) / maxButtons));
		if (spawnOnAwake) {
			counter = 0;
			ReplaceGO(gameObjects[counter]);
		}
		if (autoChangeDelay > 0)
			InvokeRepeating("NextModel", autoChangeDelay, autoChangeDelay);
	}

	public void Update() {

		if (Input.GetKeyDown(KeyCode.Space)) {
			if (_active) {
				_active = false;
			} else {
				_active = true;
			}
		}
		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			NextModel();
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			counter--;
			if (counter < 0) counter = gameObjects.Length - 1;
			ReplaceGO(gameObjects[counter]);
		}
	}

	public void NextModel() {
		counter++;
		if (counter > gameObjects.Length - 1) counter = 0;
		ReplaceGO(gameObjects[counter]);
	}

	public void Duplicate() {
		Instantiate(currentGO, currentGO.transform.position, currentGO.transform.rotation);
	}

	public void DestroyAll() {
		ParticleSystem[] objects = (ParticleSystem[])GameObject.FindObjectsOfType(typeof(ParticleSystem));
		for (int i = 0; i < objects.Length; i++) {
			Destroy(objects[i].gameObject);
		}
	}

	public void OnGUI() {
		if (_active) {
			if (gameObjects.Length > maxButtons) {
				if (GUI.Button(new Rect(20.0f, (float)((maxButtons + 1) * 18), 75.0f, 18.0f), "Prev")) if (page > 0) page--; else page = pages;
				if (GUI.Button(new Rect(95.0f, (float)((maxButtons + 1) * 18), 75.0f, 18.0f), "Next")) if (page < pages) page++; else page = 0;
				GUI.Label(new Rect(60.0f, (float)((maxButtons + 2) * 18), 150.0f, 22.0f), "Page" + (page + 1) + " / " + (pages + 1));
			}
			int pageButtonCount = gameObjects.Length - (page * maxButtons);
			if (pageButtonCount > maxButtons) pageButtonCount = maxButtons;

			for (int i = 0; i < pageButtonCount; i++) {
				string buttonText = gameObjects[i + (page * maxButtons)].transform.name;
				if (removeTextFromButton != "")
					buttonText = buttonText.Replace(removeTextFromButton, "");
				if (GUI.Button(new Rect(20.0f, (float)(i * 18 + 18), 150.0f, 18.0f), buttonText)) {
					DestroyAll();
					GameObject go = (GameObject)Instantiate(gameObjects[i + page * maxButtons]);
					currentGO = go;
					counter = i + (page * maxButtons);
				}
			}
		}
	}

	public void ReplaceGO(GameObject _go) {
		if (currentGO != null) Destroy(currentGO);
		GameObject go = (GameObject)Instantiate(_go);
		currentGO = go;
	}

	public void PlayPS(ParticleSystem _ps, int _nr) {
		Time.timeScale = 1.0f;
		_ps.Play();
	}
}