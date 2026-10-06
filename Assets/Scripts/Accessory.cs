using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SQLite4Unity3d;

[Table("Accessory")]
public class Accessory
{
    [PrimaryKey, AutoIncrement]
    public int id { get; set; }
    public int price { get; set; }
}