using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

        #region Methods
        private async void TabSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null || string.IsNullOrEmpty(ViewModel.ApiKey))
            {
                return;
            }
            // if tab 'Chat' is selected
            if (e.Source is TabControl { SelectedIndex: 0 })
            {
                if (ViewModel.Assistants.Count == 0)
                {
                    await ViewModel.LoadAssistantsAsync();
                }
                var selectedAssistant = ViewModel.Assistants
                    .FirstOrDefault(x => x.Name == ViewModel.SelectedAssistant);
                if (selectedAssistant == null)
                {
                    selectedAssistant = ViewModel.Assistants.FirstOrDefault();
                    ViewModel.SelectedAssistant = selectedAssistant?.Name ?? "";
                }
                ViewModel.Thread.Assistant = selectedAssistant;
            }
            // if tab 'Assistants' is selected
            if (e.Source is TabControl { SelectedIndex: 1 })
            {
                if (ViewModel.Assistants.Count == 0)
                {
                    await ViewModel.LoadAssistantsAsync();
                }
            }
            // if tab 'Tools' is selected
            else if (e.Source is TabControl { SelectedIndex: 2 })
            {
                ViewModel.LoadToolset();
            }
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

        private async void SelectAssistantClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null || ProjectUtils.ActiveProject == null)
            {
                return;
            }
            var resourceDictionary = new ResourceDictionary
            {
                Source = new Uri("/VSA;component/generic.xaml", UriKind.RelativeOrAbsolute)
            };
            var menuTemplate = resourceDictionary["SimpleMenuTemplate"] as ControlTemplate;
            if (menuTemplate == null)
            {
                return;
            }
            var itemTemplate = resourceDictionary["SimpleMenuItemTemplate"] as ControlTemplate;
            if (itemTemplate == null)
            {
                return;
            }
            if (ProjectUtils.ActiveProject.Assistants.Count == 0)
            {
                await ProjectUtils.ActiveProject.LoadAssistantsAsync();
            }
            var contextMenu = new ContextMenu
            {
                Background = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                Template = menuTemplate
            };
            foreach (var assistant in ProjectUtils.ActiveProject.Assistants)
            {
                var menuItem = new MenuItem
                {
                    Header = assistant.Name,
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    Background = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    Template = itemTemplate
                };
                menuItem.Click += (s, a) =>
                {
                    ViewModel.SelectedAssistant = assistant.Name;
                };
                contextMenu.Items.Add(menuItem);
            }
            contextMenu.IsOpen = true;
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
                var fileViewModel = ViewModel.AddFile(fileModel);
                if (SelectAllCheckbox.IsChecked == true)
                {
                    fileViewModel.Selected = true;
                }
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
            if (ViewModel.VectorStores.Count == 0)
            {
                await ViewModel.LoadVectorStoresAsync();
            }
        }

        private async void AddVectorStoreClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }
            var vectorStore = new VectorStoreModel();
            var vectorStoreViewModel = ViewModel.AddVectorStore(vectorStore);
            await vectorStoreViewModel.CreateAsync();
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
            var result = ToolAPI.ListFiles(new Dictionary<string, dynamic>());
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

        private void TestSetFileTextClick(object sender, RoutedEventArgs e)
        {
            var arguments = new Dictionary<string, dynamic>
            {
                { "filename", "\\TestFolder\\TestFile.cs" },
                { "text", "This is a modified test file" }
            };
            ToolAPI.SetFileText(arguments);
        }

        private void TestGetErrorsClick(object sender, RoutedEventArgs e)
        {
            var result = ToolAPI.GetErrors(new Dictionary<string, dynamic>());
            OutputWindowHelper.LogInfo("Testing", result.ToString());
        }
        #endregion
    }
}