using SQLite4Unity3d;


[Table("UserItem")]
public class UserItem
{
    [PrimaryKey, AutoIncrement]
    public int id { get; set; }
    public int user_id { get; set; }
    public string item_type { get; set; }
    public int item_id { get; set; }
    public int is_equipped { get; set; }
}