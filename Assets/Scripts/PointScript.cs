using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class PointScript : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler {

    public static GameObject ItemBeingDragged;
	public static List<LineEntity> LinesBeingDragged;
    public static List<byte> PositionOfLinesBeingDragged; 
	public static PointEntity ItemBeingLined;
    public Material m;
    public GameObject line;

	// Use this for initialization
	void Start () {
        transform.GetComponent<Button>().onClick.AddListener(Point_clicked);
	}
	
	// Update is called once per frame
	void Update () {

	}

    public void Point_clicked() {
        if (MainWindowScript.Instance.link_mod)
            Link();

        if (MainWindowScript.Instance.delete_mod)
            Delete();
    }

    public void Link()
    {
        if (ItemBeingLined == null)
        {
            ItemBeingLined = DatabaseScript.points.FirstOrDefault(x => x.go == gameObject);
            return;
        }

        if (ItemBeingLined.go == gameObject)
        {
            ItemBeingLined = null;
            return;
        }

        PointEntity this_point = DatabaseScript.points.FirstOrDefault(x => x.go == gameObject);
        if (DatabaseScript.lines.FirstOrDefault(x => (x.pos1 == this_point.id && x.pos2 == ItemBeingLined.id) || (x.pos1 == ItemBeingLined.id && x.pos2 == this_point.id)) == null)
            MainWindowScript.Instance.CreateLineEntity(ItemBeingLined, this_point);
        ItemBeingLined = null;
    }

    public void Delete()
    {
        PointEntity myPoint = DatabaseScript.points.FirstOrDefault(x => x.go == gameObject);
        
        for (int i = 0; i < DatabaseScript.points_frames.Count; i++)
        {
            PointEntity samePoint = DatabaseScript.points_frames[i].FirstOrDefault(x => x.id == myPoint.id);
            if (samePoint == null)
                continue;
            if (i < DatabaseScript.current_frame)
                samePoint.deleted_frame = DatabaseScript.current_frame;
            else if (i > DatabaseScript.current_frame)
            {
                samePoint.id = DatabaseScript.points_counter;
                samePoint.added_frame = i;
                foreach(LineEntity line in DatabaseScript.lines_frames[i])
                {
                    if (line.pos1 == myPoint.id)
                        line.pos1 = DatabaseScript.points_counter;
                    else if (line.pos2 == myPoint.id)
                        line.pos2 = DatabaseScript.points_counter;
                }
            }
        }
        List<LineEntity> lines_to_delete = new List<LineEntity>();
        foreach (LineEntity line in DatabaseScript.lines)
        {
            if (line.pos1 == myPoint.id || line.pos2 == myPoint.id)
            {
                lines_to_delete.Add(line);
				DeleteLine(line.go);
            }
        }
        foreach (LineEntity line in lines_to_delete)
        {
			DatabaseScript.lines_frames[DatabaseScript.current_frame].Remove(line);
            DatabaseScript.lines.Remove(line);
            Destroy(line.go);
        }
        if (DatabaseScript.current_frame < DatabaseScript.points_frames.Count)
            DatabaseScript.points_counter++;
        DatabaseScript.points_frames[DatabaseScript.current_frame].Remove(myPoint);
        DatabaseScript.points.Remove(myPoint);
        Destroy(gameObject);
    }

	public void DeleteLine(GameObject go){
		
		LineEntity this_line = DatabaseScript.lines.FirstOrDefault(x => x.go == go);
		
		for (int i = 0; i < DatabaseScript.lines_frames.Count; i++)
		{
			LineEntity sameLine = DatabaseScript.lines_frames[i].FirstOrDefault(x => x.id == this_line.id);
			if (sameLine == null)
				continue;
			if (i < DatabaseScript.current_frame)
				sameLine.deleted_frame = DatabaseScript.current_frame;
			else if (i > DatabaseScript.current_frame)
			{
				sameLine.id = DatabaseScript.lines_counter;
				sameLine.added_frame = i;
			}
		}
		
		if (DatabaseScript.current_frame < DatabaseScript.lines_frames.Count)
			DatabaseScript.lines_counter++;
	}

    public void OnDrag(PointerEventData eventData)
    {
        if (!MainWindowScript.Instance.move_mod)
            return;

		CheckLines();
        this.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
		if (!MainWindowScript.Instance.move_mod)
			return;

        ItemBeingDragged = gameObject;
		LinesBeingDragged = new List<LineEntity>();
        PositionOfLinesBeingDragged = new List<byte>();

        foreach (LineEntity line in DatabaseScript.lines)
        {
			if (line.pos1 == (DatabaseScript.points.FirstOrDefault(x => x.go == gameObject)).id)
            {
                LinesBeingDragged.Add(line);
                PositionOfLinesBeingDragged.Add(0);
                continue;
            }
			if (line.pos2 == (DatabaseScript.points.FirstOrDefault(x => x.go == gameObject)).id)
            {
                LinesBeingDragged.Add(line);
                PositionOfLinesBeingDragged.Add(1);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
		if (!MainWindowScript.Instance.move_mod)
			return;

        this.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        DatabaseScript.points.FirstOrDefault(x => x.go == gameObject).pos = this.transform.position;
		CheckLines ();
        ItemBeingDragged = null;
        LinesBeingDragged = null;
        PositionOfLinesBeingDragged = null;
    }

	void CheckLines()
	{
		foreach (LineEntity line in LinesBeingDragged)
		{
			Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector3 newPos = new Vector3(pos.x, pos.y, 1);
			if (PositionOfLinesBeingDragged[LinesBeingDragged.IndexOf(line)] == 0)
			{
				line.go.GetComponent<LineRenderer>().SetPosition(0, newPos);
			} else {
				line.go.GetComponent<LineRenderer>().SetPosition(1, newPos);
			}
		}
	}
}
