using UnityEngine;
using System;

public class PointEntity : IEntity, ICloneable
{

	public GameObject go;
	public Vector3 pos;

    public object Clone()
    {
        PointEntity p = new PointEntity();
        p.id = this.id;
        p.pos = this.pos;
        p.go = null;
		p.added_frame = this.added_frame;
		p.deleted_frame = this.deleted_frame;
        return p;
    }
}
