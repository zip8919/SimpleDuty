using System.IO;
using ClassIsland.Core;
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Extensions.Registry;
using ClassIsland.Shared.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleDuty.Components;
using SimpleDuty.Models;
using SimpleDuty.Pages;

namespace SimpleDuty;

[PluginEntrance]
public class Plugin : PluginBase
{
    /// <summary>
    /// 插件设置
    /// </summary>
    public DutySettings Settings { get; set; } = null!;

    public override void Initialize(HostBuilderContext context, IServiceCollection services)
    {
        // 加载配置文件
        var configPath = Path.Combine(PluginConfigFolder, "Settings.json");
        Settings = ConfigureFileHelper.LoadConfig<DutySettings>(configPath);

        // 订阅配置变化事件，自动保存配置
        Settings.PropertyChanged += (sender, args) =>
        {
            ConfigureFileHelper.SaveConfig(configPath, Settings);
        };

        // 注册服务
        services.AddSingleton(Settings);
        
        // 注册组件
        services.AddComponent<DutyStudentComponent>();
        
        // 注册设置页面
        services.AddSettingsPage<SimpleDutySettingsPage>();
    }
}