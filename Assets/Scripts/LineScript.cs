using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using System.Linq;

public class LineScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		transform.GetComponent<Button>().onClick.AddListener(Delete); //doesn't work
	}
	
	public void Delete(){
		if (!MainWindowScript.Instance.delete_mod)
			return;

		LineEntity this_line = DatabaseScript.lines.FirstOrDefault(x => x.go == gameObject);

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
}
