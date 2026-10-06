using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SQLite4Unity3d;

[Table("Background")]
public class Background
{
    [PrimaryKey, AutoIncrement]
    public int id { get; set; }
    public int price { get; set; }
    public string image_path { get; set; }
}