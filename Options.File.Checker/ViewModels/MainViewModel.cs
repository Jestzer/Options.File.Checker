﻿using System.Collections.ObjectModel;
namespace Options.File.Checker.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public static string PackageVersion => GetPackageVersion();

    public ObservableCollection<TreeViewItemModel> TreeViewItems { get; } = new ObservableCollection<TreeViewItemModel>();

    private static string GetPackageVersion()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var version = assembly?.GetName().Version?.ToString();
        return version ?? "Error getting version number.";
    }
}
