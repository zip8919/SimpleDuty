using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using SimpleDuty.Models;
using System;

namespace SimpleDuty.Components;

/// <summary>
/// 值日生组件
/// </summary>
[ComponentInfo("6F9619FF-8B86-D011-B42D-00CF4FC964FF", "值日生", "显示当前值日生信息，支持自动轮换")]
public partial class DutyStudentComponent : ComponentBase
{
    private readonly DutySettings _settings;
    private Timer? _updateTimer;
    private TextBlock? _textBlock;

    /// <summary>
    /// 初始化组件
    /// </summary>
    public DutyStudentComponent(DutySettings settings)
    {
        InitializeComponent();
        
        // 获取设置实例
        _settings = settings;
        
        // 查找TextBlock控件
        _textBlock = this.Find<TextBlock>("DutyTextBlock");
        
        // 初始化显示
        UpdateDutyStudent();
        
        // 监听设置变化
        _settings.PropertyChanged += (_, _) => UpdateDutyStudent();
        
        // 设置定时器，每分钟检查一次日期变化
        _updateTimer = new Timer(_ =>
        {
            Dispatcher.UIThread.Post(UpdateDutyStudent);
        }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        
        // 订阅Unloaded事件，释放资源
        Unloaded += (_, _) => _updateTimer?.Dispose();
    }

    /// <summary>
    /// 初始化组件UI
    /// </summary>
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// 更新值日生信息
    /// </summary>
    private void UpdateDutyStudent()
    {
        if (_textBlock == null)
        {
            return;
        }
        
        if (_settings.DutyStudents.Count == 0)
        {
            _textBlock.Text = "未设置值日生";
            return;
        }

        // 计算当前应该显示的值日生
        var currentStudent = CalculateCurrentStudent();

        // 格式化显示文本
        string formatted = _settings.DisplayFormat
            .Replace("{n}", currentStudent.Number.ToString())
            .Replace("{s}", currentStudent.Name);
            
        _textBlock.Text = formatted;
    }

    /// <summary>
    /// 计算当前应该显示的值日生
    /// </summary>
    /// <returns>当前值日生</returns>
    private DutyStudent CalculateCurrentStudent()
    {
        if (_settings.DutyStudents.Count == 0)
        {
            throw new InvalidOperationException("值日生名单为空");
        }

        // 计算从基准日期到当前日期的天数差
        int daysDiff = (int)(DateTime.Now.Date - _settings.BaseDate.Date).TotalDays;
        if (daysDiff < 0)
        {
            daysDiff = 0;
        }

        // 计算实际轮换次数
        int rotationCount = 0;
        for (int i = 0; i < daysDiff; i++)
        {
            DateTime date = _settings.BaseDate.Date.AddDays(i);
            // 检查该日期是否参与轮换（0表示周日，1表示周一，以此类推）
            if (_settings.ParticipatingDays.Contains((int)date.DayOfWeek))
            {
                rotationCount++;
            }
        }

        // 计算当前值日生索引，加上偏移量
        int currentIndex = (rotationCount + _settings.CurrentOffset) % _settings.DutyStudents.Count;
        if (currentIndex < 0)
        {
            currentIndex += _settings.DutyStudents.Count;
        }

        return _settings.DutyStudents[currentIndex];
    }
}    
