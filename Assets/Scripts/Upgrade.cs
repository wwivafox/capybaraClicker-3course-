using System.IO;
using System;
using UnityEngine;
using SQLite4Unity3d;
using System.Linq;
using System.Collections.Generic;

[Table("Upgrade")]
public class Upgrade
{
    [PrimaryKey, AutoIncrement]
    public int id { get; set; }
    public string type { get; set; } 
    public int level { get; set; }  
    public int price { get; set; }
    public float value { get; set; }  
    public string name { get; set; } 
}