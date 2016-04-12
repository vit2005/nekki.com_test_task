using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using System.IO;

public class MainWindowScript : MonoBehaviour {

	private static MainWindowScript _instance;
	public static MainWindowScript Instance{
		get{return _instance;}
	}

    public bool create_mod;
    public bool move_mod;
    public bool link_mod;
    public bool delete_mod;

	public GameObject point_GameObject;
    public GameObject line_GameObject;

	Vector3 point;

	// Use this for initialization
	void Start () {
		_instance = this;
        transform.FindChild("create_btn").GetComponent<Button>().onClick.AddListener(SetCreateMod);
        transform.FindChild("move_btn").GetComponent<Button>().onClick.AddListener(SetMoveMod);
        transform.FindChild("link_btn").GetComponent<Button>().onClick.AddListener(SetLinkMod);
        transform.FindChild("delete_btn").GetComponent<Button>().onClick.AddListener(SetDeleteMod);
        transform.FindChild("DrawField").GetComponent<Button>().onClick.AddListener(CreatePointEntity);
		#if UNITY_EDITOR
        transform.FindChild("Save_btn").GetComponent<Button>().onClick.AddListener(SaveButtonClicked);
        transform.FindChild("Load_btn").GetComponent<Button>().onClick.AddListener(LoadButtonClicked);
		#endif
        SetCreateMod();
	}

	public void CreatePointEntity() {
		if (!create_mod)
			return;

		point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		//point.y = transform.position.y;
		point.z = 0;
        GameObject o = CreatePoint(point);
		PointEntity pe = new PointEntity ();
		pe.go = o;
        pe.pos = o.transform.position;
		pe.id = DatabaseScript.points_counter;
		pe.added_frame = DatabaseScript.current_frame;
		DatabaseScript.points_counter++;
        DatabaseScript.points.Add(pe);
	}

    public void CreateLineEntity(PointEntity pos1, PointEntity pos2) {
        LineEntity le = new LineEntity();
        le.pos1 = pos1.id;
        le.pos2 = pos2.id;
        le.go = (CreateLine(pos1, pos2));
        le.id = DatabaseScript.lines_counter;
        le.added_frame = DatabaseScript.current_frame;
		DatabaseScript.lines_counter++;
        DatabaseScript.lines.Add(le);
    }

    public GameObject CreatePoint(Vector3 pos)
    {
        GameObject obj = (Instantiate(point_GameObject, pos, transform.rotation) as GameObject);
        obj.transform.SetParent(this.transform.FindChild("DrawField").transform);
        obj.transform.localScale = new Vector3(point_GameObject.transform.localScale.x * 100, point_GameObject.transform.localScale.y * 100);
        obj.SetActive(true);
        transform.FindChild("Text").GetComponent<Text>().text = obj.transform.localPosition.x.ToString() + " ; " + obj.transform.localPosition.y.ToString();
        return obj;
    }

    public GameObject CreateLine(PointEntity pos1, PointEntity pos2)
    {
		Object obj = Instantiate(line_GameObject, transform.position, transform.rotation);
		(obj as GameObject).SetActive(true);
        (obj as GameObject).transform.SetParent(this.transform.FindChild("DrawField").transform);
		LineRenderer lineRenderer = (obj as GameObject).GetComponent<LineRenderer>();
		lineRenderer.SetWidth(3,3);	
		lineRenderer.SetVertexCount(2);
        lineRenderer.SetPosition(0, new Vector3(pos1.pos.x, pos1.pos.y, 1));
        lineRenderer.SetPosition(1, new Vector3(pos2.pos.x, pos2.pos.y, 1));

        return obj as GameObject;
	}
	#if UNITY_EDITOR
    public void SaveButtonClicked()
    {
        string path = UnityEditor.EditorUtility.SaveFilePanel("Save", "", "default.an", "an");
		if (!string.IsNullOrEmpty(path))
        	DatabaseScript.SaveDatabase(path);
    }

    public void LoadButtonClicked()
    {
        string path = UnityEditor.EditorUtility.OpenFilePanel("Open", "", "an");
		if (!string.IsNullOrEmpty(path))
        	DatabaseScript.LoadDatabase(path);
    }
	#endif
	// Update is called once per frame
	void Update () {
        transform.FindChild("Frame_text").GetComponent<Text>().text = string.Format("[{0}/{1}]", DatabaseScript.current_frame+1 ,DatabaseScript.points_frames.Count);

		if (Input.GetKeyDown (KeyCode.A)) {
			DatabaseScript.SaveCurrentFrame();
			DatabaseScript.MoveBackward();
            RedrawFrame();
		}
		if (Input.GetKeyDown (KeyCode.D)) {
			DatabaseScript.SaveCurrentFrame();
			DatabaseScript.MoveForward();
            RedrawFrame();
		}
	}

	public void RedrawFrame()
	{
		foreach(Transform child in this.transform.FindChild("DrawField")) {
			Destroy(child.gameObject);
		}
		foreach (PointEntity pos in DatabaseScript.points) {
			GameObject point = CreatePoint(pos.pos);
            pos.go = point;
		}
		foreach (LineEntity pos in DatabaseScript.lines) {
			PointEntity p1 = DatabaseScript.points.FirstOrDefault(x => x.id == pos.pos1);
			PointEntity p2 = DatabaseScript.points.FirstOrDefault(x => x.id == pos.pos2);
			GameObject line = CreateLine(p1, p2);
            pos.go = line;
        }
	}

    void SetCreateMod()
    {
        SetMod(true, false, false, false);
    }

    void SetMoveMod()
    {
        SetMod(false, true, false, false);
    }

    void SetLinkMod()
    {
        SetMod(false, false, true, false);
    }

    void SetDeleteMod()
    {
        SetMod(false, false, false, true);
    }

    void SetMod(bool create, bool move, bool link, bool delete)
    {
        create_mod = create;
        move_mod = move;
        link_mod = link;
        delete_mod = delete;
        transform.FindChild("create_btn_blocker").gameObject.SetActive(create);
        transform.FindChild("move_btn_blocker").gameObject.SetActive(move);
        transform.FindChild("link_btn_blocker").gameObject.SetActive(link);
        transform.FindChild("delete_btn_blocker").gameObject.SetActive(delete);
    }
}
