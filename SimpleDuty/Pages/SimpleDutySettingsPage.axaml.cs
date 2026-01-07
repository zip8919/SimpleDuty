using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using SimpleDuty.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace SimpleDuty.Pages;

/// <summary>
/// SimpleDuty设置页面
/// </summary>
[SettingsPageInfo("SimpleDuty", "SimpleDuty")]
public partial class SimpleDutySettingsPage : SettingsPageBase
{
    /// <summary>
    /// 插件设置
    /// </summary>
    private readonly DutySettings _settings;
    
    /// <summary>
    /// 名单文本框
    /// </summary>
    private readonly TextBox _listTextBox;

    /// <summary>
    /// 构造函数
    /// </summary>
    public SimpleDutySettingsPage(DutySettings settings)
    {
        InitializeComponent();
        
        // 获取设置实例
        _settings = settings;
        
        // 设置数据上下文
        DataContext = this;
        
        // 获取名单文本框
        _listTextBox = this.FindControl<TextBox>("ListTextBox")!;
        
        // 初始化UI
        UpdateListTextBox();
        
        // 监听值日生名单变化
        _settings.DutyStudents.CollectionChanged += (_, _) => UpdateListTextBox();
        

    }

    /// <summary>
    /// 初始化组件UI
    /// </summary>
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    /// <summary>
    /// 更新名单文本框内容
    /// </summary>
    private void UpdateListTextBox()
    {
        var sb = new StringBuilder();
        foreach (var student in _settings.DutyStudents)
        {
            sb.AppendLine($"{student.Number} {student.Name}");
        }
        _listTextBox.Text = sb.ToString().Trim();
    }



    /// <summary>
    /// 自定义显示格式
    /// </summary>
    public string DisplayFormat
    {
        get => _settings.DisplayFormat;
        set => _settings.DisplayFormat = value;
    }

    /// <summary>
    /// 值日生名单
    /// </summary>
    public ObservableCollection<DutyStudent> DutyStudents => _settings.DutyStudents;

    /// <summary>
    /// 基准日期
    /// </summary>
    public DateTimeOffset BaseDate
    {
        get => _settings.BaseDate;
        set => _settings.BaseDate = value;
    }

    /// <summary>
    /// 当前值日偏移量
    /// </summary>
    public int CurrentOffset
    {
        get => _settings.CurrentOffset;
        set => _settings.CurrentOffset = value;
    }

    /// <summary>
    /// 设置参与轮换的星期几
    /// </summary>
    public void SetParticipatingDays(string days)
    {
        var dayList = new List<int>();
        var dayNames = new Dictionary<string, int> { { "周日", 0 }, { "周一", 1 }, { "周二", 2 }, { "周三", 3 }, { "周四", 4 }, { "周五", 5 }, { "周六", 6 } };
        
        foreach (var dayName in days.Split(new[] { ' ', ',', '，' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (dayNames.TryGetValue(dayName, out var day))
            {
                dayList.Add(day);
            }
        }
        
        _settings.ParticipatingDays = new ObservableCollection<int>(dayList.OrderBy(d => d));
    }

    /// <summary>
    /// 导入名单命令
    /// </summary>
    public ICommand ImportListCommand => new RelayCommand(ImportList);

    /// <summary>
    /// 保存名单命令
    /// </summary>
    public ICommand SaveListCommand => new RelayCommand(SaveList);

    /// <summary>
    /// 导入名单
    /// </summary>
    private async void ImportList()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel != null)
            {
                // 打开文件选择对话框
                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "选择名单文件",
                    AllowMultiple = false,
                    FileTypeFilter = new List<FilePickerFileType>
                    {
                        new FilePickerFileType("文本文件")
                        {
                            Patterns = new[] { "*.txt" },
                            MimeTypes = new[] { "text/plain" }
                        }
                    }
                });

                if (files.Count > 0)
                {
                    var file = files[0];
                    using var stream = await file.OpenReadAsync();
                    using var reader = new StreamReader(stream);
                    var content = await reader.ReadToEndAsync();
                    
                    // 设置名单文本
                    _listTextBox.Text = content;
                    
                    // 解析并保存名单
                    ParseAndSaveList(content);
                }
            }
        }
    }

    /// <summary>
    /// 保存名单
    /// </summary>
    private void SaveList()
    {
        if (_listTextBox.Text != null)
        {
            ParseAndSaveList(_listTextBox.Text);
        }
    }

    /// <summary>
    /// 解析并保存名单
    /// </summary>
    /// <param name="content">名单文本内容</param>
    private void ParseAndSaveList(string content)
    {
        if (content == null)
        {
            return;
        }
        
        var students = new List<DutyStudent>();
        
        // 按行解析名单
        var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                if (int.TryParse(parts[0], out var number))
                {
                    var name = string.Join(" ", parts.Skip(1));
                    students.Add(new DutyStudent(number, name));
                }
            }
        }
        
        // 按编号排序
        students.Sort((a, b) => a.Number.CompareTo(b.Number));
        
        // 更新值日生名单
        _settings.DutyStudents.Clear();
        foreach (var student in students)
        {
            _settings.DutyStudents.Add(student);
        }
    }
    
    /// <summary>
    /// 设置为周一到周六参与轮换
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public void SetMonToSat(object sender, RoutedEventArgs e)
    {
        _settings.ParticipatingDays = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6 };
    }
    
    /// <summary>
    /// 设置为周一到周五参与轮换
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public void SetMonToFri(object sender, RoutedEventArgs e)
    {
        _settings.ParticipatingDays = new ObservableCollection<int> { 1, 2, 3, 4, 5 };
    }
    
    /// <summary>
    /// 设置为每天参与轮换
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public void SetEveryDay(object sender, RoutedEventArgs e)
    {
        _settings.ParticipatingDays = new ObservableCollection<int> { 0, 1, 2, 3, 4, 5, 6 };
    }
}    
    
/// <summary>    
/// 简单的命令实现    
/// </summary>    
/// <typeparam name="T">命令参数类型</typeparam>    
public class RelayCommand<T> : ICommand    
{    
    private readonly Action<T> _execute;    
    private readonly Func<T, bool>? _canExecute;    
    
    /// <summary>    
    /// 构造函数    
    /// </summary>    
    /// <param name="execute">执行的操作</param>    
    /// <param name="canExecute">是否可以执行</param>    
    public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)    
    {    
        _execute = execute;    
        _canExecute = canExecute;    
    }    
    
    /// <summary>    
    /// 执行命令    
    /// </summary>    
    /// <param name="parameter">命令参数</param>    
    public void Execute(object? parameter)    
    {    
        if (parameter is T t)    
        {    
            _execute(t);    
        }    
    }    
    
    /// <summary>    
    /// 是否可以执行命令    
    /// </summary>    
    /// <param name="parameter">命令参数</param>    
    /// <returns>是否可以执行</returns>    
    public bool CanExecute(object? parameter)    
    {    
        if (_canExecute == null)    
            return true;    
        
        if (parameter is T t)    
        {    
            return _canExecute(t);    
        }    
        
        return false;    
    }    
    
    /// <summary>    
    /// 当CanExecute状态变化时触发    
    /// </summary>    
    public event EventHandler? CanExecuteChanged;    
}    
    
/// <summary>    
/// 无参数的命令实现    
/// </summary>    
public class RelayCommand : ICommand    
{    
    private readonly Action _execute;    
    private readonly Func<bool>? _canExecute;    
    
    /// <summary>    
    /// 构造函数    
    /// </summary>    
    /// <param name="execute">执行的操作</param>    
    /// <param name="canExecute">是否可以执行</param>    
    public RelayCommand(Action execute, Func<bool>? canExecute = null)    
    {    
        _execute = execute;    
        _canExecute = canExecute;    
    }    
    
    /// <summary>    
    /// 执行命令    
    /// </summary>    
    /// <param name="parameter">命令参数</param>    
    public void Execute(object? parameter)    
    {    
        _execute();    
    }    
    
    /// <summary>    
    /// 是否可以执行命令    
    /// </summary>    
    /// <param name="parameter">命令参数</param>    
    /// <returns>是否可以执行</returns>    
    public bool CanExecute(object? parameter)    
    {    
        return _canExecute == null || _canExecute();    
    }    
    
    /// <summary>    
    /// 当CanExecute状态变化时触发    
    /// </summary>    
    public event EventHandler? CanExecuteChanged;    
}    
