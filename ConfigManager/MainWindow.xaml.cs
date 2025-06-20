﻿using ConfigManagerLib;
using ConfigManagerLib.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ConfigManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string ConfigFileName = "config.json";
        private IProfileStoreManager _profileStoreManager;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                _profileStoreManager = new ProfileStoreManager(ConfigFileName, "ProfilesStore");
                LoadProfilesFromConfig();
            }
            catch (Exception ex)
            {
                WriteOutput($"Failed to load profiles: {ex}");
            }
        }

        private void tbProfileFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(ProfilesListView.ItemsSource).Refresh();
        }

        private bool ProfileFilter(object item)
        {
            if (String.IsNullOrEmpty(tbProfileFilter.Text))
                return true;

            return ((item as ProfileInfo).Name.IndexOf(tbProfileFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void LoadProfilesFromConfig()
        {
            ProfilesListView.ItemsSource = _profileStoreManager.Configuration.Profiles;
            var view = (CollectionView)CollectionViewSource.GetDefaultView(ProfilesListView.ItemsSource);
            view.Filter = ProfileFilter;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _profileStoreManager.Save();
            }
            catch (Exception ex)
            {
                WriteOutput($"Failed to save config: {ex}");
            }
        }

        private void WriteOutput(string msg)
        {
            OutputTextBox.Dispatcher.Invoke(new Action(() => { OutputTextBox.Text = msg + Environment.NewLine + OutputTextBox.Text; }));
        }

        private void AddProfileButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileNameInputDialog dialog = new ProfileNameInputDialog();
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (dialog.ShowDialog() == true)
            {
                var profileName = dialog.ProfileName;
                if (_profileStoreManager.Configuration.Profiles.FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.CurrentCultureIgnoreCase)) != null)
                {
                    MessageBox.Show($"Profile name: {profileName} is already defined, please choose a different name");
                    return;
                }

                var profile = _profileStoreManager.AddProfile(profileName);
                _profileStoreManager.Save();
                LoadProfilesFromConfig();
            }
        }

        private void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedProfile == null)
                return;

            var profile = _profileStoreManager.GetProfile(SelectedProfile.Name);
            if (profile == null)
                return;

            ProfileNameInputDialog dialog = new ProfileNameInputDialog() { ProfileName = profile.Name };
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (dialog.ShowDialog() == true)
            {
                var profileName = dialog.ProfileName;
                if (_profileStoreManager.Configuration.Profiles.FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.CurrentCultureIgnoreCase)) != null)
                {
                    MessageBox.Show($"Profile name: {profileName} is already defined, please choose a different name");
                    return;
                }

                _profileStoreManager.RenameProfile(profile.Name, profileName);

                LoadProfilesFromConfig();
            }
        }

        private void CloneProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedProfile == null)
                return;

            var profile = _profileStoreManager.GetProfile(SelectedProfile.Name);
            if (profile == null)
                return;

            ProfileNameInputDialog dialog = new ProfileNameInputDialog() { ProfileName = profile.Name };
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (dialog.ShowDialog() == true)
            {
                var newProfileName = dialog.ProfileName;
                if (_profileStoreManager.Configuration.Profiles.FirstOrDefault(p => p.Name.Equals(newProfileName, StringComparison.CurrentCultureIgnoreCase)) != null)
                {
                    MessageBox.Show($"Profile name: {newProfileName} is already defined, please choose a different name");
                    return;
                }
                _profileStoreManager.CloneProfile(profile.Name, newProfileName);

                LoadProfilesFromConfig();
            }
        }

        private ProfileInfo? SelectedProfile { get; set; }
        private void ProfilesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 1)
            {
                SelectedProfile = null;
                return;
            }

            var profile = e.AddedItems[0] as ProfileInfo;
            SelectedProfile = profile;

            if (SelectedProfile == null)
            {
                return;
            }

            SetFilesFromProfile(SelectedProfile.Name);
        }

        private void SetFilesFromProfile(string profileName)
        {
            FilesListView.Items.Clear();

            var profile = _profileStoreManager.GetProfile(profileName);
            if (profile == null)
            {
                return;
            }

            foreach (var file in profile.Files)
            {
                FilesListView.Items.Add(file);
            }
        }

        private ConfigFileInfo? SelectedFile { get; set; }
        private void FilesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count < 1)
            {
                SelectedFile = null;
                return;
            }
            SelectedFile = e.AddedItems[0] as ConfigFileInfo;
        }

        private void UseProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedProfile == null) 
            {
                return;
            }

            try
            {
                _profileStoreManager.UseProfile(SelectedProfile.Name);
                WriteOutput($"Successfully applied profile: {SelectedProfile.Name} {DateTime.Now.ToString("f")}");
            }
            catch (Exception ex)
            {
                WriteOutput($"Failed to use profile: {SelectedProfile.Name}, {ex}");
            }
        }

        private void AddFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedProfile == null)
                return;

            var profile = _profileStoreManager.GetProfile(SelectedProfile.Name);
            if (profile == null)
                return;

            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == false)
            {
                return;
            }

            var file = ofd.FileName;
            if (profile.Files.FirstOrDefault(x => x.OriginalFileName == file) != null)
            {
                MessageBox.Show($"File already added to profile: {file}");
                return;
            }

            try
            {
                var fileInfo = _profileStoreManager.AddFileToProfile(profile.Name, file);

                FilesListView.Items.Add(fileInfo);
            }
            catch (Exception ex)
            {
                WriteOutput($"Failed to add file: {file}, {ex}");
            }
        }

        private void EditFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFile == null)
                return;

            try
            {
                var txt = File.ReadAllText(SelectedFile.FileName);
                var dialog = new TextFileEditDialog { Title = SelectedFile.Description };
                dialog.Owner = this;
                dialog.FileContent = txt;
                if (dialog.ShowDialog() == false)
                    return;

                File.WriteAllText(SelectedFile.FileName, dialog.FileContent);
            }
            catch (Exception ex)
            {
                WriteOutput($"Failed to edit file: {ex}");
            }
        }

        

        private void DeleteProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedProfile == null)
            {
                return;
            }

            if (MessageBox.Show($"You are deleting profile: {SelectedProfile.Name}, are you sure?", "Delete", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                var profileName = SelectedProfile.Name;

                _profileStoreManager.DeleteProfile(SelectedProfile.Name);
                SelectedProfile = null;

                LoadProfilesFromConfig();

                WriteOutput($"Profile: {profileName} deleted successfully");
            }
            catch (Exception ex)
            {
                WriteOutput($"Failed to delete profile: {SelectedProfile.Name}, {ex}");
            }
        }

        private void DeleteFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedProfile == null || SelectedFile == null) 
                return;
            
            var profile = _profileStoreManager.GetProfile(SelectedProfile.Name);
            if (profile == null)
                return;

            try
            {
                File.Delete(SelectedFile.FileName);
                var file = profile.Files.FirstOrDefault(x => x.OriginalFileName == SelectedFile.OriginalFileName);
                if (file != null)
                {
                    profile.Files.Remove(file);
                    _profileStoreManager.Save();
                }
                FilesListView.Items.Remove(SelectedFile);
            }
            catch (Exception ex )
            {
                WriteOutput($"Failed to delete file: {SelectedFile.OriginalFileName}, {ex}");
            }
        }

        private void CompareButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFile == null) 
                return;

            var orgText = File.ReadAllText(SelectedFile.OriginalFileName);
            var profileTxt = File.ReadAllText(SelectedFile.FileName);

            var diff = Difference(orgText, profileTxt);
            if (string.IsNullOrEmpty(diff))
            {
                MessageBox.Show("Files are identical");
            }
            else
            {
                MessageBox.Show(diff);
            }
        }

        public string Difference(string str1, string str2)
        {
            if (str1 == null)
            {
                return str2;
            }
            if (str2 == null)
            {
                return str1;
            }

            List<string> set1 = str1.Split(' ').Distinct().ToList();
            List<string> set2 = str2.Split(' ').Distinct().ToList();

            var diff = set2.Count() > set1.Count() ? set2.Except(set1).ToList() : set1.Except(set2).ToList();

            return string.Join("", diff);
        }

        private void DiffButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedProfile == null)
            {
                MessageBox.Show("Please select a profile to compare.");
                return;
            }

            var differencesResult = "";
            foreach (var file in SelectedProfile.Files)
            {
                try
                {
                    var originalContent = File.ReadAllText(file.OriginalFileName);
                    var profileContent = File.ReadAllText(file.FileName);
                    var diff = Difference(originalContent, profileContent);
                    if (!string.IsNullOrEmpty(diff))
                    {
                        differencesResult += $"Differences found in file: {file.Description}\n{diff}";
                    }
                }
                catch (Exception ex)
                {
                    WriteOutput($"Failed to compare file: {file.Description}, {ex}");
                }
            }

            if (differencesResult == "")
            {
                MessageBox.Show("No differences found in the selected profile files.");
            }
            else
            {
                MessageBox.Show(differencesResult, "Differences Found", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
