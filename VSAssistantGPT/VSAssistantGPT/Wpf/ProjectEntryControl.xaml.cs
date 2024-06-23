using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using cpGames.VSA.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;
using TabControl = System.Windows.Controls.TabControl;
using UserControl = System.Windows.Controls.UserControl;

namespace cpGames.VSA.Wpf
{
    public partial class ProjectEntryControl : UserControl
    {
        #region Properties
        public ProjectViewModel ViewModel
        {
            get => (DataContext as ProjectViewModel)!;
            set => DataContext = value;
        }
        #endregion

        #region Constructors
        public ProjectEntryControl()
        {
            ViewModel = ProjectUtils.ActiveProject;
            InitializeComponent();
            Loaded += OnLoaded;
        }
        #endregion

        #region Events
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // on FTE open Settings tab
            if (ViewModel.FTE)
            {
                TabControl.SelectedIndex = 3;
            }
        }
        #endregion

        #region Methods
        private async void TabSelected(object sender, SelectionChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(ViewModel.ApiKey))
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
                await ViewModel.LoadToolsetAsync();
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
            var assistantTemplateJson = JsonConvert.SerializeObject(ViewModel.NewAssistantTemplateViewModel.Model);
            var assistant = JsonConvert.DeserializeObject<AssistantModel>(assistantTemplateJson);
            ViewModel.AddAssistant(assistant!);
        }

        private void AddToolClicked(object sender, RoutedEventArgs e)
        {
            ViewModel.CreateToolAsync();
        }

        private async void LoadFilesClicked(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadFilesAsync();
        }

        private async void SyncFilesClicked(object sender, RoutedEventArgs e)
        {
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
            await ViewModel.DeleteSelectedFilesAsync();
        }

        private async void FilesExpanded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Files.Count == 0)
            {
                await ViewModel.LoadFilesAsync();
            }
        }

        private void SelectAllFilesChecked(object sender, RoutedEventArgs e)
        {
            foreach (var file in ViewModel.Files)
            {
                file.Selected = true;
            }
        }

        private void SelectAllFilesUnchecked(object sender, RoutedEventArgs e)
        {
            foreach (var file in ViewModel.Files)
            {
                file.Selected = false;
            }
        }

        private async void LoadVectorStoresClicked(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadVectorStoresAsync();
        }

        private async void VectorStoresExpanded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.VectorStores.Count == 0)
            {
                await ViewModel.LoadVectorStoresAsync();
            }
        }

        private async void AddVectorStoreClicked(object sender, RoutedEventArgs e)
        {
            await ViewModel.CreateVectorStoreAsync();
        }

        private void OpenToolsClicked(object sender, RoutedEventArgs e)
        {
            var toolsDir = Utils.GetOrCreateAppDir("Tools");
            Process.Start(toolsDir);
        }

        private void SaveProjectClicked(object sender, RoutedEventArgs e)
        {
            ViewModel.Save();
        }

        private void PythonSelectClicked(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Python DLL|*.dll",
                Title = "Select Python DLL"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                ViewModel.PythonDll = openFileDialog.FileName;
            }
        }

        private async void ReloadToolsClicked(object sender, RoutedEventArgs e)
        {
            await ViewModel.ReloadToolsetAsync();
        }
        #endregion

        #region Testing
        private void TestHasSelectionClick(object sender, RoutedEventArgs e)
        {
            var result = ToolAPI.HasSelection(new Dictionary<string, dynamic>());
            OutputWindowHelper.LogInfo("Testing", result.ToString());
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
            var result = ToolAPI.SetSelection(arguments);
            OutputWindowHelper.LogInfo("Testing", result.ToString());
        }

        private void TestGetActiveDocumentTextClick(object sender, RoutedEventArgs e)
        {
            var result = ToolAPI.GetActiveDocumentText(new Dictionary<string, dynamic>());
            OutputWindowHelper.LogInfo("Testing", result.ToString());
        }

        private void TestSetActiveDocumentTextClick(object sender, RoutedEventArgs e)
        {
            var arguments = new Dictionary<string, dynamic>
            {
                { "text", "This is a test file" }
            };
            var result = ToolAPI.SetActiveDocumentText(arguments);
            OutputWindowHelper.LogInfo("Testing", result.ToString());
        }

        private void TestGetActiveDocumentPathClick(object sender, RoutedEventArgs e)
        {
            var result = ToolAPI.GetActiveDocumentPath(new Dictionary<string, dynamic>());
            OutputWindowHelper.LogInfo("Testing", result.ToString());
        }

        private void TestGetDocumentTextClick(object sender, RoutedEventArgs e)
        {
            var arguments = new Dictionary<string, dynamic>
            {
                { "filename", "TestFolder\\TestFile.cs" }
            };
            var result = ToolAPI.GetDocumentText(arguments);
            OutputWindowHelper.LogInfo("Testing", result.ToString());
        }

        private void TestSetDocumentTextClick(object sender, RoutedEventArgs e)
        {
            var arguments = new Dictionary<string, dynamic>
            {
                { "filename", "TestFolder\\TestFile.cs" },
                { "text", "This is a modified test file" }
            };
            var result = ToolAPI.SetDocumentText(arguments);
            OutputWindowHelper.LogInfo("Testing", result.ToString());
        }

        private void TestOpenDocumentClick(object sender, RoutedEventArgs e)
        {
            var arguments = new Dictionary<string, dynamic>
            {
                { "filename", "TestFolder\\TestFile.cs" }
            };
            var result = ToolAPI.OpenDocument(arguments);
            OutputWindowHelper.LogInfo("Testing", result.ToString());
        }

        private void TestCloseDocumentClick(object sender, RoutedEventArgs e)
        {
            var arguments = new Dictionary<string, dynamic>
            {
                { "filename", "TestFolder\\TestFile.cs" }
            };
            var result = ToolAPI.CloseDocument(arguments);
            OutputWindowHelper.LogInfo("Testing", result.ToString());
        }

        private void TestCreateDocumentClick(object sender, RoutedEventArgs e)
        {
            var arguments = new Dictionary<string, dynamic>
            {
                { "filename", "TestFolder\\TestFile.cs" },
                { "text", "This is a test file" }
            };
            var result = ToolAPI.CreateDocument(arguments);
            OutputWindowHelper.LogInfo("Testing", result.ToString());
        }

        private void TestDeleteDocumentClick(object sender, RoutedEventArgs e)
        {
            var arguments = new Dictionary<string, dynamic>
            {
                { "filename", "TestFolder\\TestFile.cs" }
            };
            var result = ToolAPI.DeleteDocument(arguments);
            OutputWindowHelper.LogInfo("Testing", result.ToString());
        }

        private void TestHasDocumentClick(object sender, RoutedEventArgs e)
        {
            var arguments = new Dictionary<string, dynamic>
            {
                { "filename", "TestFolder\\TestFile.cs" }
            };
            var result = ToolAPI.HasDocument(arguments);
            OutputWindowHelper.LogInfo("Testing", result.ToString());
        }

        private void TestListDocumentsClick(object sender, RoutedEventArgs e)
        {
            var result = ToolAPI.ListDocuments(new Dictionary<string, dynamic>());
            OutputWindowHelper.LogInfo("Testing", result.ToString());
        }

        private void TestGetErrorsClick(object sender, RoutedEventArgs e)
        {
            var result = ToolAPI.GetErrors(new Dictionary<string, dynamic>());
            OutputWindowHelper.LogInfo("Testing", result.ToString());
        }

        private void TestGetProjectPathClick(object sender, RoutedEventArgs e)
        {
            var result = ToolAPI.GetProjectPath(new Dictionary<string, dynamic>());
            OutputWindowHelper.LogInfo("Testing", result.ToString());
        }
        #endregion
    }
}