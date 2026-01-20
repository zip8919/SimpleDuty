using System.Collections.ObjectModel;
using ClassIsland.Shared;

namespace SimpleDuty.Models;

/// <summary>
/// 插件设置模型
/// </summary>
public class DutySettings : AttachableSettingsObject
{
    private ObservableCollection<int> _participatingDays = [];
    private ObservableCollection<DutyStudent> _dutyStudents = [];
    private string _displayFormat = "值日：{n}{s}";
    private DateTimeOffset _baseDate = DateTimeOffset.Now.Date;
    private int _currentOffset = 0;

    /// <summary>
    /// 参与轮换的星期几列表，0表示周日，1表示周一，以此类推
    /// </summary>
    public ObservableCollection<int> ParticipatingDays
    {
        get => _participatingDays;
        set => SetProperty(ref _participatingDays, value);
    }

    /// <summary>
    /// 值日生名单
    /// </summary>
    public ObservableCollection<DutyStudent> DutyStudents
    {
        get => _dutyStudents;
        set => SetProperty(ref _dutyStudents, value);
    }

    /// <summary>
    /// 自定义显示格式，{n}表示编号，{s}表示姓名
    /// </summary>
    public string DisplayFormat
    {
        get => _displayFormat;
        set => SetProperty(ref _displayFormat, value);
    }

    /// <summary>
    /// 基准日期，用于计算轮换次数
    /// </summary>
    public DateTimeOffset BaseDate
    {
        get => _baseDate;
        set => SetProperty(ref _baseDate, value);
    }

    /// <summary>
    /// 当前值日偏移量
    /// </summary>
    public int CurrentOffset
    {
        get => _currentOffset;
        set => SetProperty(ref _currentOffset, value);
    }

    /// <summary>
    /// 初始化一个新的插件设置实例
    /// </summary>
    public DutySettings()
    {
        // 默认周一到周六参与轮换（1-6）
        ParticipatingDays = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6 };
        
        // 默认值日生名单
        DutyStudents = new ObservableCollection<DutyStudent>
        {
            new DutyStudent(1, "张三"),
            new DutyStudent(2, "李四"),
            new DutyStudent(3, "王五")
        };
    }
}
