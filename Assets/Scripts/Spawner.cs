using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json; //Net 3.5
using Newtonsoft.Json.Converters; //Net 3.5

public class Spawner : MonoBehaviour {
	private Dictionary<string, Object> objects; //object_name, object_model
	private Dictionary<string, GameObject> spawnObjects; //object_id, clone_object_model
	private Dictionary<string, Hero> map_heroes; //list of heroes
	private Dictionary<string, Obj> map_objs;//list of objects

	private struct Mesh {
		public Object gameObject;
		public Vector3 location;
		public Mesh(Object go, Vector3 trans) {
			gameObject = go;
			location = trans;
		}
	}

	//Structs of objects
	private struct Hero {
		public string name_id; //name for interaction
		public string name; //name of object
		public string model_name; //name for model
		public int russ;//type of russ
		public int[] buffs;
		public int nowXp;
		public int maxXp;
		public int nowHp;
		public int maxHp;
		public int nowMana;
		public int maxMana;
		public int posX;
		public int posY;
		public int posZ;
	}

	private struct Obj {
		public string name_id;
		public string name;
		public string model_name;
		public string description;
		public int posX;
		public int posY;
		public int posZ;
	}

	private void Start() {
		//Init models from folder
		objects = new Dictionary<string, Object>();
		foreach (var path in Resources.LoadAll("Objects", typeof(GameObject))) {
			objects.Add(path.name, path);
		}
		//Init spawned obj
		spawnObjects = new Dictionary<string, GameObject>();
		SpawnObject("anime_girls", new Mesh(objects["anime_girls"], new Vector3(0, 0, 0)));
		SetScale(spawnObjects["anime_girls"]);
		//With that coomand we can create folder after build project so we can archive maps in it
		//Debug.Log(Application.dataPath);
		//Directory.CreateDirectory(Application.dataPath + "/Test");
	}

	private void SetScale(GameObject obj) {
		obj.transform.localScale = new Vector3(0.01F, 0.01F, 0.01F);
	}
//TO JSON string json = JsonConvert.SerializeObject(product);
	private void LoadMap(string path) {
		map_heroes = new Dictionary<string, Hero>();
		map_objs = new Dictionary<string, Obj>();
		StreamReader read = new StreamReader(path);
		string line;
		string[] tmp;
		Hero tmp_hero;
		Obj tmp_obj;
		try {
			while ((line = read.ReadLine()) != null) {
				tmp = line.Split(' ');
				switch(tmp[0]) {
					case "Hero":
					tmp_hero = JsonConvert.DeserializeObject<Hero>(tmp[1]);
					map_heroes.Add(tmp_hero.name_id, tmp_hero);
					SpawnObject(tmp_hero.name_id, 
						new Mesh(objects[tmp_hero.model_name], 
							new Vector3(tmp_hero.posX, tmp_hero.posY, tmp_hero.posZ)));
					break;
					case "Obj":
					tmp_obj = JsonConvert.DeserializeObject<Obj>(tmp[1]);
					map_objs.Add(tmp_obj.name_id, tmp_obj);
					SpawnObject(tmp_obj.name_id,
						new Mesh(objects[tmp_obj.model_name],
							new Vector3(tmp_obj.posX, tmp_obj.posY, tmp_obj.posZ)));
					break;
				}
			}
		} catch {
			//Error loading map
		}
		read.Close();
		read.Dispose();
	}



	private void SpawnObject(string name, Mesh obj) {
		spawnObjects.Add(
			name, 
			Instantiate(obj.gameObject, 
			obj.location, 
			Quaternion.Euler(0, 0, 0)) as GameObject);
	}
}