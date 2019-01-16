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
		const int height_map = 10;
		int[] zcell = new int[(width_map + 1) * (height_map + 1)];
		for (int i = 0; i < zcell.Length; i++) {
			//zcell[i] = Random.Range(-2, 2);
			zcell[i] = 0;
		}

		zcell[12] = 2;
		zcell[13] = 3;
		zcell[14] = 4;
		zcell[14] = -2;

		string[] textures_id = new string[width_map * height_map * 2];
		for (int i = 0; i < textures_id.Length; i++) {
			textures_id[i] = "grace";
		}

		textures_id[1] = "rock_texture";
		textures_id[2] = "rock_texture";
		textures_id[3] = "rock_texture";
		textures_id[4] = "rock_texture";
		textures_id[5] = "rock_texture";

		SpawnLandscape(zcell, width_map, height_map, textures_id);
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
				}
			}
		} catch {
			//Error loading map
		}
		read.Close();
		read.Dispose();
	}

	private void SpawnLandscape(int[] y, int width_map, int height_map, string[] textures_id = null) {
		Dictionary<string, MeshGenInfo> meshinfo = new Dictionary<string, MeshGenInfo>();
		if (textures_id == null) { return; }
		//Seperate landscape on objects
		for (int i = 0; i < textures_id.Length; i++) {
			if (!meshinfo.ContainsKey(textures_id[i])) {
				meshinfo.Add(textures_id[i], new MeshGenInfo(new List<Vector3>(), new List<int>()));
			}
		}
		
		string name_mesh_tmp;
		int findtmp;
		for (int z = 0; z < height_map; z++) {
			for (int x = 0; x < width_map; x++) {
				//Check texture id by x, z
				name_mesh_tmp = textures_id[(width_map * z) + (x * 2)];
				if ((findtmp = meshinfo[name_mesh_tmp].vertices.FindIndex(item => item.x == x 
					&& item.y == y[((z + 1) * (width_map + 1)) + x]
					&& item.z == z + 1)) == -1) {
					meshinfo[name_mesh_tmp].vertices.Add(new Vector3(x, y[((z + 1) * (width_map + 1)) + x], z + 1));
					meshinfo[name_mesh_tmp].triangles.Add(meshinfo[name_mesh_tmp].vertices.Count - 1);
				} else {
					meshinfo[name_mesh_tmp].triangles.Add(findtmp);
				}

				if ((findtmp = meshinfo[name_mesh_tmp].vertices.FindIndex(item => item.x == x + 1 
					&& item.y == y[x + (z * (width_map + 1)) + 1]
					&& item.z == z)) == -1) {
					meshinfo[name_mesh_tmp].vertices.Add(new Vector3(x + 1, y[x + (z * (width_map + 1)) + 1], z));
					meshinfo[name_mesh_tmp].triangles.Add(meshinfo[name_mesh_tmp].vertices.Count - 1);
				} else {
					meshinfo[name_mesh_tmp].triangles.Add(findtmp);
				}

				if ((findtmp = meshinfo[name_mesh_tmp].vertices.FindIndex(item => item.x == x
					&& item.y == y[x + (z * (width_map + 1))]
					&& item.z == z)) == -1) {
					meshinfo[name_mesh_tmp].vertices.Add(new Vector3(x, y[x + (z * (width_map + 1))], z));
					meshinfo[name_mesh_tmp].triangles.Add(meshinfo[name_mesh_tmp].vertices.Count - 1);
				} else {
					meshinfo[name_mesh_tmp].triangles.Add(findtmp);
				}

				name_mesh_tmp = textures_id[(width_map * z) + (x * 2) + 1];
				if ((findtmp = meshinfo[name_mesh_tmp].vertices.FindIndex(item => item.x == x
					&& item.y == y[((z + 1) * (width_map + 1)) + x]
					&& item.z == z + 1)) == -1) {
					meshinfo[name_mesh_tmp].vertices.Add(new Vector3(x, y[((z + 1) * (width_map + 1)) + x], z + 1));
					meshinfo[name_mesh_tmp].triangles.Add(meshinfo[name_mesh_tmp].vertices.Count - 1);
				} else {
					meshinfo[name_mesh_tmp].triangles.Add(findtmp);
				}

				if ((findtmp = meshinfo[name_mesh_tmp].vertices.FindIndex(item => item.x == x + 1
					&& item.y == y[((z + 1) * (width_map + 1)) + x + 1]
					&& item.z == z + 1)) == -1) {
					meshinfo[name_mesh_tmp].vertices.Add(new Vector3(x + 1, y[((z + 1) * (width_map + 1)) + x + 1], z + 1));
					meshinfo[name_mesh_tmp].triangles.Add(meshinfo[name_mesh_tmp].vertices.Count - 1);
				} else {
					meshinfo[name_mesh_tmp].triangles.Add(findtmp);
				}

				if ((findtmp = meshinfo[name_mesh_tmp].vertices.FindIndex(item => item.x == x + 1
					&& item.y == y[x + (z * (width_map + 1)) + 1]
					&& item.z == z)) == -1) {
					meshinfo[name_mesh_tmp].vertices.Add(new Vector3(x + 1, y[x + (z * (width_map + 1)) + 1], z));
					meshinfo[name_mesh_tmp].triangles.Add(meshinfo[name_mesh_tmp].vertices.Count - 1);
				} else {
					meshinfo[name_mesh_tmp].triangles.Add(findtmp);
				}
				//SpawnTile(i, j, y, width_map, height_map);
				//new Vector3(x, y[((z + 1) * (width_map + 1)) + x], z + 1),
        		//new Vector3(x + 1, y[x + (z * (width_map + 1)) + 1], z),
        		//new Vector3(x, y[x + (z * (width_map + 1))], z),
        		//new Vector3(x + 1, y[((z + 1) * (width_map + 1)) + x + 1], z + 1)
			}
		}

		foreach(KeyValuePair<string, MeshGenInfo> item in meshinfo) {
			//Debug.Log("OK" + item.Key.ToString());
			SpawnMesh(item.Value.vertices, item.Value.triangles, item.Key);
		}

		//SpawnTile(i, j, y, width_map, height_map);
	}

	private void SpawnMesh(List<Vector3> vertices, List<int> triangles, string texture_name = "grace") {
		GameObject gameObj = new GameObject(texture_name);
		gameObj.AddComponent<MeshRenderer>();
		MeshFilter meshFilter = gameObj.AddComponent<MeshFilter>();
        Mesh mesh = meshFilter.mesh;

        Vector2[] uvs = new Vector2[vertices.Count];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        gameObj.transform.Translate(0f, 0f, 0f);

        Material material = new Material(Shader.Find("Standard"));
        //Material.SetColor("_Color", new Color(0f, 0.7f, 0f)); //green main color
        material.SetTexture("_MainTex", Resources.Load<Texture>("Textures/" + texture_name));
        gameObj.GetComponent<Renderer>().material = material;
	}

	private void SpawnTile(int x, int z, int[] y, int width_map, int height_map) {
		GameObject tile = new GameObject("tile" + x + ":"  + z); 
        tile.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = tile.AddComponent<MeshFilter>();
        Mesh mesh = meshFilter.mesh;

        Vector3[] vertices = new Vector3[] {
        	new Vector3(x, y[((z + 1) * (width_map + 1)) + x], z + 1),
        	new Vector3(x + 1, y[x + (z * (width_map + 1)) + 1], z),
        	new Vector3(x, y[x + (z * (width_map + 1))], z),
        	new Vector3(x + 1, y[((z + 1) * (width_map + 1)) + x + 1], z + 1)
        };
        /*int x = 0;
        int z = 0;
        for (int i = 0; i < vertices.Length; i++) {
        	vertices[i] = new Vector3(x, y[i], z);
        	if (x == width_map) {
        		z++;
        		x = 0;
        	} else { x++; }
        }*/

        int[] triangles = new int[] {
        	0, 1, 2,
        	0, 3, 1
        };
        /*x = 0;
        z = 0;
        for (int i = 0; i < triangles.Length; i+= 6) {
        	triangles[i] = ((z + 1) * (width_map + 1)) + x;
        	triangles[i + 1] = x + (z * (width_map + 1)) + 1;
        	triangles[i + 2] = x + (z * (width_map + 1));

        	triangles[i + 3] = ((z + 1) * (width_map + 1)) + x;
        	triangles[i + 4] = ((z + 1) * (width_map + 1)) + x + 1;
        	triangles[i + 5] = x + (z * (width_map + 1)) + 1;

        	if (x == width_map - 1) {
        		z++;
        		x = 0;
        	}
        	else { x++; }
        }*/

        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        //mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        tile.transform.Translate(0f, 0f, 0f);

        Material material = new Material(Shader.Find("Standard"));
        //Material.SetColor("_Color", new Color(0f, 0.7f, 0f)); //green main color
        material.SetTexture("_MainTex", Resources.Load<Texture>("Textures/rock_texture"));
        tile.GetComponent<Renderer>().material = material;
        spawnObjects.Add("tile" + x + ":" + z, tile);
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

	private struct MeshGenInfo {
		public List<Vector3> vertices;
		public List<int> triangles;
		public MeshGenInfo(List<Vector3> verticesT, List<int> trianglesT) {
			vertices = verticesT;
			triangles = trianglesT;
		}
	}

	private struct Landscape {
		int width_map;
		int height_map;
		int[] y;
		string[] textures_id;
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