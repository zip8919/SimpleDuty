namespace SimpleDuty.Models;

/// <summary>
/// 值日生数据模型
/// </summary>
public class DutyStudent
{
    /// <summary>
    /// 编号
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 初始化一个新的值日生实例
    /// </summary>
    public DutyStudent() { }

    /// <summary>
    /// 初始化一个新的值日生实例
    /// </summary>
    /// <param name="number">编号</param>
    /// <param name="name">姓名</param>
    public DutyStudent(int number, string name)
    {
        Number = number;
        Name = name;
    }
}
