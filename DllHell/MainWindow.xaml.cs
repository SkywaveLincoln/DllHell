using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using DllHell.Entities;

namespace DllHell
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string[] ValidExtensions = new string[] { ".DLL", ".EXE", ".OCX" };

        public MainWindow()
        {
            InitializeComponent();

            AverageType.Items.Add("By Count");
            AverageType.Items.Add("By Middle Version");
            AverageType.Items.Add("Highest Version");
            AverageType.SelectedIndex = 0;
        }

        private void BrowseForFolder_Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folder = new System.Windows.Forms.FolderBrowserDialog();
            if (!string.IsNullOrEmpty(FolderToScan_Text.Text))
                folder.SelectedPath = FolderToScan_Text.Text;

            var result = folder.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.Cancel)
                return;

            FolderToScan_Text.Text = folder.SelectedPath;

            ScanFolder(FolderToScan_Text.Text);
        }

        private void ScanFolder(string folderToScan)
        {
            var directory = new DirectoryInfo(folderToScan);
            if (!directory.Exists)
            {
                FolderToScan_Text.Foreground = Brushes.Crimson;
                return;
            }
            else { FolderToScan_Text.Foreground = Brushes.Black; }

            DllList_ListView.Items.Clear();

            var files = directory.GetFiles("*", SearchOption.AllDirectories).Where(
                f => ValidExtensions.Contains(f.Extension.ToUpper()));

            foreach (var file in files)
            {
                var lvi = DllList_ListView.Items.Add(new MyFile(file));
            }

            ColourVersions();
        }

        private void ColourVersions()
        {
            var myFiles = new List<MyFile>();
            foreach (var item in DllList_ListView.Items)
            {
                myFiles.Add((MyFile)item);
            }

            var distinctVersions = myFiles
                .Where(x => !x.Exclude)
                .Select(x => x.Version)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
            var versionsByCount = myFiles
                .Where(x => !x.Exclude)
                .Select(x => x.Version)
                .GroupBy(x => x)
                .ToDictionary(x => x.Key, x => x.ToList().Count);

            distinctVersions.ForEach(x => Console.WriteLine(x));
            versionsByCount.Keys.ToList().ForEach(x => Console.WriteLine("{0}: {1}", x, versionsByCount[x]));

            var middleVersion = distinctVersions
                .Skip((int)Math.Floor(distinctVersions.Count / 2.0)) // skip "lower" half (works on even and odd numbers)
                .FirstOrDefault();
            var popularVersion = versionsByCount
                .OrderByDescending(x => x.Key)
                .ThenByDescending(x => x.Value) // Gets the highest version with the highest count
                .FirstOrDefault();
            var highestVersion = distinctVersions
                .OrderByDescending(x => x)
                .FirstOrDefault();

            foreach (var file in myFiles)
            {
                if(file.Exclude)
                {
                    file.ErrorColor = Brushes.Silver;
                    continue;
                }
                switch (AverageType.SelectedItem)
                {
                    case "By Count":
                        file.ErrorColor = file.Version != popularVersion.Key ? Brushes.Crimson : DllList_ListView.Foreground;
                        break;
                    case "By Middle Version":
                        if (file.Version < middleVersion)
                            file.ErrorColor = Brushes.Crimson;
                        else if (file.Version > middleVersion)
                            file.ErrorColor = Brushes.ForestGreen;
                        else
                            file.ErrorColor = DllList_ListView.Foreground;
                        break;
                    case "Highest Version":
                        file.ErrorColor = file.Version != highestVersion ? Brushes.Crimson : DllList_ListView.Foreground;
                        break;
                    default:
                        break;
                }
            }

            for (int i = 0; i < DllList_ListView.Items.Count; i++)
            {
                var item = DllList_ListView.Items.GetItemAt(i);
                item = myFiles.First(x => x.Name == ((MyFile)item).Name);
            }
            DllList_ListView.Items.Refresh();
        }

        private void FolderToScan_Text_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
                ScanFolder(FolderToScan_Text.Text);
        }

        private void DllList_ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var shiftDown = (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift));

            if (DllList_ListView.SelectedItems.Count == 0)
                return;

            var selectedItem = VisualTreeHelper.HitTest(DllList_ListView, Mouse.GetPosition(DllList_ListView)).VisualHit;

            while (selectedItem != null && !(selectedItem is System.Windows.Controls.ListViewItem))
                selectedItem = VisualTreeHelper.GetParent(selectedItem);

            if (selectedItem != null)
            {
                var item = (MyFile)((ListViewItem)selectedItem).DataContext;
                item.Exclude = !item.Exclude;

                if(shiftDown)
                {
                    for (int i = 0; i < DllList_ListView.Items.Count; i++)
                    {
                        var loopItem = (MyFile)DllList_ListView.Items.GetItemAt(i);
                        if (loopItem.Version == item.Version)
                            loopItem.Exclude = item.Exclude;
                        var replaceItem = DllList_ListView.Items.GetItemAt(i);
                        replaceItem = loopItem;
                    }
                }
                else
                {
                    DllList_ListView.SelectedItem = item;
                }
                DllList_ListView.Items.Refresh();
            }

            ColourVersions();
        }

        private void AverageType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ColourVersions();
        }
    }
}

// https://stackoverflow.com/questions/27858274/getting-the-item-under-mouse-cursor-in-listview
