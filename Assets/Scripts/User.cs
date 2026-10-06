using System; 
using UnityEngine;
using UnityEngine.UI;
using SQLite4Unity3d;

[Table("User")]
public class User
{
    [PrimaryKey, AutoIncrement]
    public int id { get; set; }
    public string username { get; set; } = "New User"; 
    public int score { get; set; } = 0;
    public string created_at { get; set; } = DateTime.UtcNow.ToString("o");
    public int click_level { get; set; }
    public int passive_level { get; set; }
    public int interval_level { get; set; }
    public int passive_income { get; set; }
    public float income_interval { get; set; }
    public string last_active { get; set; }
    public int total_play_time { get; set; }
    public string unlocked_backgrounds { get; set; }
    public int current_background { get; set; }
}