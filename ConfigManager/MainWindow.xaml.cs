﻿using ConfigManagerLib;
using ConfigManagerLib.Model;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
                WriteOutput($"Failed to load prfiles: {ex}");
            }
        }

        private void LoadProfilesFromConfig()
        {
            foreach (var profile in _profileStoreManager.Configuration.Profiles)
            {
                ProfilesListView.Items.Add(profile);
            }
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
            OutputTextBox.Dispatcher.Invoke(new Action(() => { OutputTextBox.Text += msg; }));
        }

        private void AddProfileButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileNameInputDialog dialog = new ProfileNameInputDialog();
            if (dialog.ShowDialog() == true)
            {
                var profileName = dialog.ProfileName;
                if (_profileStoreManager.Configuration.Profiles.FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.CurrentCultureIgnoreCase)) != null)
                {
                    MessageBox.Show($"Profile name: {profileName} is already defined, please choose a different name");
                    return;
                }

                var profile = _profileStoreManager.AddProfile(profileName);
                ProfilesListView.Items.Add(profile);

                _profileStoreManager.Save();
            }
        }

        private ProfileInfo? SelectedProfile { get; set; }
        private void ProfilesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
                WriteOutput($"Successfully applied profile: {SelectedProfile}");
            }
            catch (Exception ex)
            {
                WriteOutput($"Failed to use profile: {SelectedProfile}, {ex}");
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
                var fi = new FileInfo(file);
                var newFileName = System.IO.Path.Combine(profile.ProfileDirectory, fi.Name);
                var fileInfo = new ConfigFileInfo
                {
                    Description = fi.Name,
                    OriginalFileName = file,
                    FileName = newFileName,
                    Created = DateTime.Now,
                    Updated = DateTime.Now
                };

                File.Copy(file, newFileName, true);
                FilesListView.Items.Add(fileInfo);
                profile.Files.Add(fileInfo);
                _profileStoreManager.Save();
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

            if (MessageBox.Show($"You are deleting profile: {SelectedProfile}, are you sure?", "Delete", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                _profileStoreManager.DeleteProfile(SelectedProfile.Name);
                ProfilesListView.Items.Remove(SelectedProfile);
                SelectedProfile = null;
                WriteOutput($"Profile: {SelectedProfile} deleted successfully");
            }
            catch (Exception ex)
            {
                WriteOutput($"Failed to delete profile: {SelectedProfile}, {ex}");
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
    }
}
