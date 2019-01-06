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

	private void Start() {
		//Init models from folder
		objects = new Dictionary<string, Object>();
		foreach (var path in Resources.LoadAll("Objects", typeof(GameObject))) {
			objects.Add(path.name, path);
		}
		//Init spawned obj and map
		spawnObjects = new Dictionary<string, GameObject>();
		//SpawnObject("anime_girls", new MeshObj(objects["anime_girls"], new Vector3(0, 0, 0)));
		//SetScale(spawnObjects["anime_girls"]);
		//With that coomand we can create folder after build project so we can archive maps in it
		//Debug.Log(Application.dataPath);
		//Directory.CreateDirectory(Application.dataPath + "/Test");
		const int width_map = 10;
		const int height_map = 2;
		int[] zcell = new int[(width_map + 1) * (height_map + 1)];
		for (int i = 0; i < zcell.Length; i++) {
			//zcell[i] = Random.Range(-2, 2);
			zcell[i] = 0;
		}
		//Seperate to rectangles


		SpawnLandscape(zcell, width_map, height_map);
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
				tmp = line.Split('=');
				switch(tmp[0]) {
					case "Hero":
					tmp_hero = JsonConvert.DeserializeObject<Hero>(tmp[1]);
					map_heroes.Add(tmp_hero.name_id, tmp_hero);
					SpawnObject(tmp_hero.name_id, 
						new MeshObj(objects[tmp_hero.model_name], 
							new Vector3(tmp_hero.posX, tmp_hero.posY, tmp_hero.posZ)));
					break;
					case "Obj":
					tmp_obj = JsonConvert.DeserializeObject<Obj>(tmp[1]);
					map_objs.Add(tmp_obj.name_id, tmp_obj);
					SpawnObject(tmp_obj.name_id,
						new MeshObj(objects[tmp_obj.model_name],
							new Vector3(tmp_obj.posX, tmp_obj.posY, tmp_obj.posZ)));
					break;
					case "Landscape":
					break;
					case "Desc":
					break;
					case "Author":
					break;
				}
			}
		} catch {
			//Error loading map
		}
		read.Close();
		read.Dispose();
	}

	private void SpawnLandscape(int[] y, int width_map, int height_map) {
		GameObject landscape = new GameObject("Landscape"); 
        landscape.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = landscape.AddComponent<MeshFilter>();
        Mesh mesh = meshFilter.mesh;

        Vector3[] vertices = new Vector3[y.Length];
        int x = 0;
        int z = 0;
        for (int i = 0; i < vertices.Length; i++) {
        	vertices[i] = new Vector3(x, y[i], z);
        	if (x == width_map) {
        		z++;
        		x = 0;
        	} else { x++; }
        }

        int[] triangles = new int[width_map * height_map * 6];
        x = 0;
        z = 0;
        for (int i = 0; i < triangles.Length; i+= 6) {
        	triangles[i] = ((z + 1) * width_map) + (z + 1) + x;
        	triangles[i + 1] = x + (z * width_map) + 1 + z;
        	triangles[i + 2] = x + (z * width_map) + z;

        	triangles[i + 3] = ((z + 1) * width_map) + (z + 1) + x;
        	triangles[i + 4] = ((z + 1) * width_map) + (z + 1) + x + 1;
        	triangles[i + 5] = x + (z * width_map) + 1 + z;

        	if (x == width_map - 1) {
        		z++;
        		x = 0;
        	}
        	else { x++; }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        landscape.transform.Translate(0f, 0f, 0f);

        Material Material = new Material(Shader.Find("Standard"));
        Material.SetColor("_Color", new Color(0f, 0.7f, 0f)); //green main color
        landscape.GetComponent<Renderer>().material = Material;
        spawnObjects.Add("landscape", landscape);
	}

	private void SpawnObject(string name, MeshObj obj) {
		spawnObjects.Add(
			name, 
			Instantiate(obj.gameObject, 
				obj.location, 
				Quaternion.Euler(0, 0, 0)) as GameObject);
	}

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

	private struct Landscape {
		uint width_map;
		uint height_map;
		int[] z;
	}

	private struct MeshObj {
		public Object gameObject;
		public Vector3 location;
		public MeshObj(Object go, Vector3 trans) {
			gameObject = go;
			location = trans;
		}
	}
}