using System.Collections.ObjectModel;

namespace Options.File.Checker
{
    public class TreeViewItemModel
    {
       public string? Title { get; set; }
        public ObservableCollection<TreeViewItemModel> Children { get; set; } = new ObservableCollection<TreeViewItemModel>();
    }
}