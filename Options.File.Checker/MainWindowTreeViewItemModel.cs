using System.Collections.ObjectModel;

namespace Options.File.Checker
{
    // For the TreeView used on the MainWindow, displaying products' options lines that subtract from their seat count.
    public class MainWindowTreeViewItemModel
    {
       public string? Title { get; set; }
        public ObservableCollection<MainWindowTreeViewItemModel> Children { get; set; } = [];
    }
}