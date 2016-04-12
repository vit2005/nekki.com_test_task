using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

public static class DatabaseScript
{
    static public List<PointEntity> points;
    static public List<LineEntity> lines;
    static public List<List<PointEntity>> points_frames;
    static public List<List<LineEntity>> lines_frames;
    static public int current_frame;

    static public int points_counter;
    static public int lines_counter;

    static DatabaseScript()
    {
        RefreshDatabase();
    }

    static private void RefreshDatabase()
    {
        points = new List<PointEntity>();
        lines = new List<LineEntity>();
        points_frames = new List<List<PointEntity>>();
        lines_frames = new List<List<LineEntity>>();
        points_frames.Add(points);
        lines_frames.Add(lines);
        current_frame = 0;
        points_counter = 0;
        lines_counter = 0;
    }

    static public void SaveCurrentFrame()
    {
        List<PointEntity> temp_points = new List<PointEntity>();
        List<LineEntity> temp_lines = new List<LineEntity>();
        foreach (PointEntity p in points)
            temp_points.Add(p.Clone() as PointEntity);

        foreach (LineEntity l in lines)
            temp_lines.Add(l.Clone() as LineEntity);

        points_frames[current_frame] = temp_points;
        lines_frames[current_frame] = temp_lines;
        points = temp_points;
        lines = temp_lines;
    }

    static public void NewFrame()
    {
        points_frames.Add(new List<PointEntity>());
        lines_frames.Add(new List<LineEntity>());
        current_frame++;
        SaveCurrentFrame();
    }

    static public void MoveForward()
    {
        if (points_frames.Count - 1 == current_frame)
            NewFrame();
        else
        {
            current_frame++;
            points = points_frames[current_frame];
            lines = lines_frames[current_frame];
        }

    }

    static public void MoveBackward()
    {
        if (current_frame == 0)
            current_frame = points_frames.Count - 1;
        else
        {
            current_frame--;
        }
        points = points_frames[current_frame];
        lines = lines_frames[current_frame];
    }

    static public void SaveDatabase(string filename)
    {
        Dictionary<int, List<PointEntity>> points_array = new Dictionary<int, List<PointEntity>>();
        foreach (List<PointEntity> frame in points_frames)
        {
            foreach (PointEntity p in frame)
            {
                if (!points_array.ContainsKey(p.id))
                    points_array.Add(p.id, new List<PointEntity>());
                points_array[p.id].Add(p);
            }
        }

        Dictionary<int, LineEntity> lines_array = new Dictionary<int,LineEntity>();
        foreach (List<LineEntity> frame in lines_frames)
        {
            foreach (LineEntity l in frame)
            {
                if (!lines_array.ContainsKey(l.id))
                    lines_array.Add(l.id, l);
            }
        }


        List<List<string>> saved_points = SavePoints(points_array);
        List<string> saved_lines = SaveLines(lines_array);
        

        using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename))
        {
            file.WriteLine(points_frames.Count);
            foreach (int point_index in points_array.Keys)
            {
                string line = string.Empty;
                foreach (string value in saved_points[point_index])
                    line = (line == string.Empty) ? value : string.Format("{0};{1}", line, value);
                file.WriteLine(line);
            }
            file.WriteLine("=");
            foreach(string line in saved_lines)
            {
                file.WriteLine(line);
            }
        }
    }

    static public void LoadDatabase(string filename)
    {
        RefreshDatabase();

        using (System.IO.StreamReader reader = new System.IO.StreamReader(filename))
        {
            int frames = Convert.ToInt32(reader.ReadLine());
            for (int i = 0; i < frames-1; i++)
            {
                points_frames.Add(new List<PointEntity>());
                lines_frames.Add(new List<LineEntity>());
            }

            string line;
            bool isPoint = true;
            while ((line = reader.ReadLine()) != null)
            {
                if (line == "=")
                {
                    isPoint = false;
                    continue;
                }

				if (line == string.Empty)
					continue;

                if (isPoint)
                {
                    int id = Convert.ToInt32(line.Split(new[] { ')' })[0]);
					if (id > points_counter)
						points_counter = id;

					Debug.Log("point "+id.ToString()+" loaded");
                    string[] frame_delta = line.Substring(line.IndexOf('['), line.IndexOf(']') - line.IndexOf('[')).Replace("[","").Replace("]","").Split(new[]{'|'}); 
                    // i was drunked, when i wrote this... should work
                    int frame_from = Convert.ToInt32(frame_delta[0]);
                    int frame_to = Convert.ToInt32(frame_delta[1]);

                    string[] each_frame = line.Split(new[] { ']' })[1].Split(new[] { ';' });
                    List<Vector3> each_frame_Vector = new List<Vector3>();
                    string[] first_frame_Vector = each_frame[0].Split(new[]{','});
                    each_frame_Vector.Add(new Vector3(Convert.ToInt32(first_frame_Vector[0]), Convert.ToInt32(first_frame_Vector[1])));
                    for (int i = 1; i < each_frame.Length; i++)
                    {
                        if (each_frame[i].Contains("s"))
                        {
                            for (int j = 0; j < Convert.ToInt32(each_frame[i].Replace("s", "")); j++)
                                each_frame_Vector.Add(each_frame_Vector[i - 1]);
                        }
                        else
                        {
                            string[] xy = each_frame[i].Split(new[] {','});
                            each_frame_Vector.Add(new Vector3(each_frame_Vector[i - 1].x + Convert.ToInt32(xy[0]), each_frame_Vector[i - 1].y + Convert.ToInt32(xy[1])));
                        }
                    }
                    int due = (frame_to == -1) ? frames : frame_to;
                    for (int i = frame_from, j = 0; i < due; i++, j++)
                    {
                        PointEntity p = new PointEntity();
                        p.id = id;
                        p.added_frame = frame_from;
                        p.deleted_frame = frame_to;
                        p.pos = each_frame_Vector[j];
                        points_frames[i].Add(p);
                    }
                }
                else
                {
                    int id = Convert.ToInt32(line.Split(new[] { ')' })[0]);
					if (id > lines_counter)
						lines_counter = id;

					Debug.Log("line "+id.ToString()+" loaded");
                    string[] frame_delta = line.Substring(line.IndexOf('['), line.IndexOf(']') - line.IndexOf('[')).Replace("[", "").Replace("]", "").Split(new[] { '|' });
                    // i was drunked, when i wrote this... should work
                    int frame_from = Convert.ToInt32(frame_delta[0]);
                    int frame_to = Convert.ToInt32(frame_delta[1]);
                    string[] pos = line.Split(new[] { ']' })[1].Split(new[] { ',' });
                    int due = (frame_to == -1) ? frames : frame_to;

                    for (int i = frame_from; i < due; i++)
                    {
                        LineEntity l = new LineEntity();
                        l.id = id;
                        l.added_frame = frame_from;
                        l.deleted_frame = frame_to;
                        l.pos1 = Convert.ToInt32(pos[0]);
                        l.pos2 = Convert.ToInt32(pos[1]);
                        lines_frames[i].Add(l);
                    }
                }
            }
			points = points_frames[0];
			lines = lines_frames[0];

            MainWindowScript.Instance.RedrawFrame();
        }
    }

    static private List<List<string>> SavePoints(Dictionary<int, List<PointEntity>> points_array)
    {
        List<List<string>> saved_points = new List<List<string>>();
        for (int i = 0; i < points_counter; i++)
            saved_points.Add(new List<string>());

        foreach (int point_index in points_array.Keys)
        {
            Vector3 prepious_pos = new Vector3();
            foreach (PointEntity p in points_array[point_index])
            {
                if (saved_points[point_index].Count == 0)
                {
                    saved_points[point_index].Add(string.Format("{0})[{1}|{2}]{3},{4}", p.id, p.added_frame, p.deleted_frame, (int)p.pos.x, (int)p.pos.y));
                    prepious_pos = p.pos;
                    continue;
                }
                if (p.pos == prepious_pos)
                {
                    string previous_value = saved_points[point_index][saved_points[point_index].Count - 1];
                    if (previous_value.Contains("s"))
                    {
                        int same_frames_value = Convert.ToInt32(previous_value.Replace("s", ""));
                        same_frames_value++;
                        saved_points[point_index][saved_points[point_index].Count - 1] = string.Format("s{0}", same_frames_value);
                        continue;
                    }
                    saved_points[point_index].Add("s1");
                    continue;
                }

                saved_points[point_index].Add(string.Format("{0},{1}", (int)(p.pos.x - prepious_pos.x), (int)(p.pos.y - prepious_pos.y)));
                prepious_pos = p.pos;
            }
        }

        return saved_points;
    }

    static private List<string> SaveLines(Dictionary<int, LineEntity> lines_array)
    {
        List<string> saved_lines = new List<string>();

        for (int i = 0; i <= lines_counter; i++)
            saved_lines.Add(string.Empty);

        foreach (int line_index in lines_array.Keys)
        {
            saved_lines[line_index] = string.Format("{0})[{1}|{2}]{3},{4}", lines_array[line_index].id, lines_array[line_index].added_frame, lines_array[line_index].deleted_frame, lines_array[line_index].pos1, lines_array[line_index].pos2);
        }

        return saved_lines;
    }
}
