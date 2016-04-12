using UnityEngine;
using System;

public class LineEntity : IEntity, ICloneable
{
    
    public GameObject go;
    public int pos1;
	public int pos2;

	public object Clone()
	{
		LineEntity l = new LineEntity();
        l.id = this.id;
		l.added_frame = this.added_frame;
		l.deleted_frame = this.deleted_frame;
		l.pos1 = this.pos1;
		l.pos2 = this.pos2;
		l.go = null;
		return l;
	}
}
