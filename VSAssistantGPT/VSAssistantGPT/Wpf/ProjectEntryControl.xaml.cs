using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using cpGames.VSA.ViewModel;
using Newtonsoft.Json.Linq;

namespace cpGames.VSA.Wpf
{
    public partial class ProjectEntryControl : UserControl
    {
        #region Properties
        public ProjectViewModel? ViewModel
        {
            get => DataContext as ProjectViewModel;
            set => DataContext = value;
        }
        #endregion

        #region Constructors
        public ProjectEntryControl()
        {
            InitializeComponent();
            ViewModel = ProjectUtils.ActiveProject;
        }
        #endregion

        #region Events
        private async void OnTabSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }
            // if tab 'Assistants' is selected
            if (e.Source is TabControl { SelectedIndex: 1 })
            {
                await ViewModel.LoadAssistantsAsync();
            }
            // if tab 'Tools' is selected
            else if (e.Source is TabControl { SelectedIndex: 2 })
            {
                ViewModel.LoadToolset();
            }
        }
        #endregion

        #region Methods
        private void AddTaskClicked(object sender, RoutedEventArgs e)
        {
            ViewModel?.AddTask(new TaskModel());
        }

        private void StartProjectClick(object sender, RoutedEventArgs e)
        {
            //Processor.GetInstance().ExecuteAsync().ConfigureAwait(false);
        }

        private void ReloadClicked(object sender, RoutedEventArgs e)
        {
            //Processor.GetInstance().ReloadProjectAsync().ConfigureAwait(false);
        }

        private async void TestCall(object sender, RoutedEventArgs e)
        {
            var toolCallJson = @"{
                'function': {
                    'name': 'create_task',
                    'arguments': {
                        'name': 'Sample Task',
                        'description': 'This is a sample task description.',
                        'assignee': 'Unassigned',
                        'order': 1,
                    }
                }
            }";

            var toolCall = JToken.Parse(toolCallJson);
            var result = await ToolAPI.HandleToolCallAsync(toolCall);
            Console.WriteLine($"Tool call response: {result}");
        }

        private void SelectAssistantClicked(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void AddAssistantClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }
            var assistant = new AssistantModel
            {
                name = "New Assistant",
                description = "Your helpful assistant",
                instructions = "Instructions for the assistant"
            };
            ViewModel.AddAssistant(assistant);
        }

        private void AddToolClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }
            var tool = new ToolModel
            {
                name = "NewTool",
                description = "Write tool description (used by GPT)",
                category = "New Category"
            };
            ViewModel.AddTool(tool, false);
        }

        private async void LoadFilesClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }
            await ViewModel.LoadFilesAsync();
        }

        private async void SyncFilesClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }
            if (ViewModel.Files.Count == 0)
            {
                await ViewModel.LoadFilesAsync();
            }
            while (ViewModel.Files.Count > 0)
            {
                var file = ViewModel.Files[0];
                await file.DeleteAsync();
            }
            var projectItems = DTEUtils.GetProjectItemsInActiveProject();
            foreach (var projectItem in projectItems)
            {
                await OutputWindowHelper.LogInfoAsync("Processor", $"Uploading {projectItem.FileNames[0]}");
                var fileModel = new FileModel
                {
                    name = projectItem.Name,
                    path = projectItem.FileNames[0]
                };
                ViewModel.AddFile(fileModel);
            }
            await ViewModel.UploadFilesAsync();
        }

        private async void DeleteFilesClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }
            await ViewModel.DeleteSelectedFilesAsync();
        }

        private async void FilesExpanded(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }
            if (ViewModel.Files.Count == 0)
            {
                await ViewModel.LoadFilesAsync();
            }
        }

        private void SelectAllFilesChecked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }
            foreach (var file in ViewModel.Files)
            {
                file.Selected = true;
            }
        }

        private void SelectAllFilesUnchecked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }
            foreach (var file in ViewModel.Files)
            {
                file.Selected = false;
            }
        }

        private void SelectAllVectorStoresChecked(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SelectAllVectorStoresUnchecked(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void LoadVectorStoresClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }
            await ViewModel.LoadVectorStoresAsync();
        }

        private async void VectorStoresExpanded(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }
            if (ViewModel.Files.Count == 0)
            {
                await ViewModel.LoadVectorStoresAsync();
            }
        }

        private void AddVectorStoreClicked(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void RefreshThreadClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null || string.IsNullOrEmpty(ViewModel.Thread.Id))
            {
                return;
            }
            await ViewModel.Thread.DeleteAsync();
            await ViewModel.Thread.CreateAsync();
        }
        #endregion

        #region Testing
        private void TestOpenFileClick(object sender, RoutedEventArgs e)
        {
            var arguments = new Dictionary<string, dynamic>
            {
                { "filename", "\\TestFolder\\TestFile.cs" }
            };
            ToolAPI.OpenFile(arguments);
        }

        private void TestCreateFileClick(object sender, RoutedEventArgs e)
        {
            var arguments = new Dictionary<string, dynamic>
            {
                { "filename", "\\TestFolder\\TestFile.cs" },
                { "text", "This is a test file" }
            };
            ToolAPI.CreateFile(arguments);
        }

        private void TestCreateFolderClick(object sender, RoutedEventArgs e)
        {
            DTEUtils.CreateFolder("\\TestFolder1");
        }

        private void TestHasFileClick(object sender, RoutedEventArgs e)
        {
            var arguments = new Dictionary<string, dynamic>
            {
                { "filename", "\\TestFolder\\TestFile.cs" }
            };
            var result = ToolAPI.HasFile(arguments);
            OutputWindowHelper.LogInfo("Testing", result.ToString());
        }

        private void TestDeleteFileClick(object sender, RoutedEventArgs e)
        {
            var arguments = new Dictionary<string, dynamic>
            {
                { "filename", "\\TestFolder\\TestFile.cs" }
            };
            ToolAPI.DeleteFile(arguments);
        }

        private void TestGetSelectionClick(object sender, RoutedEventArgs e)
        {
            var result = ToolAPI.GetSelection(new Dictionary<string, dynamic>());
            OutputWindowHelper.LogInfo("Testing", result.ToString());
        }

        private void TestSetSelectionClick(object sender, RoutedEventArgs e)
        {
            var arguments = new Dictionary<string, dynamic>
            {
                { "text", "some random text" }
            };
            ToolAPI.SetSelection(arguments);
        }

        private void TestGetFilesClick(object sender, RoutedEventArgs e)
        {
            var result = ToolAPI.GetFiles(new Dictionary<string, dynamic>());
            OutputWindowHelper.LogInfo("Testing", result.ToString());
        }


        private void TestGetFileTextClick(object sender, RoutedEventArgs e)
        {
            var arguments = new Dictionary<string, dynamic>
            {
                { "filename", "\\TestFolder\\TestFile.cs" }
            };
            var result = ToolAPI.GetFileText(arguments);
            OutputWindowHelper.LogInfo("Testing", result.ToString());
        }
        #endregion

    }
}