using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using cpGames.VSA.ViewModel;
using Newtonsoft.Json;
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

        #region Methods
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
                menuItem.Click += (s, a) => { ViewModel.SelectedAssistant = assistant.Name; };
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

        private void LocateClicked(object sender, RoutedEventArgs e)
        {
            if (!DTEUtils.HasActiveDocument())
            {
                return;
            }

            var documentPath = DTEUtils.GetActiveDocumentPath();
            SelectDocument(documentPath);
        }


        private void SearchFile_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchFile.Text))
            {
                return;
            }

            var files = ViewModel.GetAllFiles();
            var matchingFile = files.FirstOrDefault(file => file.Name.ToLower().Contains(SearchFile.Text.ToLower()));
            if (matchingFile != null)
            {
                SelectDocument(matchingFile.Path);
            }
        }

        private void SelectDocument(string documentPath)
        {
            var treePath = ViewModel.GetTreePath(documentPath);
            var itemNames = treePath.Split(Path.DirectorySeparatorChar);
            ItemsControl container = ResourceTreeView;
            TreeViewItem? treeViewItem = null;
            foreach (var itemName in itemNames)
            {
                for (var i = 0; i < container.Items.Count; i++)
                {
                    var item = (TreeViewItem)container.ItemContainerGenerator.ContainerFromIndex(i);
                    if ((container.Items[i] as FileViewModel)?.Name == itemName)
                    {
                        treeViewItem = item;
                        treeViewItem.IsExpanded = true;
                        treeViewItem.UpdateLayout();
                        container = treeViewItem;
                        break;
                    }
                }
            }

            if (treeViewItem != null)
            {
                treeViewItem.IsSelected = true;
                treeViewItem.BringIntoView();
            }
        }

        private async void ReloadResourcesClicked(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadSolutionAsync();
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
                { "selection_text", "some random text" }
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
                { "document_text", "This is a test file" }
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
            var projectPath = DTEUtils.GetActiveProjectPath();
            var documentPath = Path.Combine(projectPath, "TestFolder\\TestFile.cs");
            var arguments = new Dictionary<string, dynamic>
            {
                { "document_path", documentPath }
            };
            var result = ToolAPI.GetDocumentText(arguments);
            OutputWindowHelper.LogInfo("Testing", result.ToString());
        }

        private void TestSetDocumentTextClick(object sender, RoutedEventArgs e)
        {
            var projectPath = DTEUtils.GetActiveProjectPath();
            var documentPath = Path.Combine(projectPath, "TestFolder\\TestFile.cs");
            var arguments = new Dictionary<string, dynamic>
            {
                { "document_path", documentPath },
                { "document_text", "This is a modified test file" }
            };
            var result = ToolAPI.SetDocumentText(arguments);
            OutputWindowHelper.LogInfo("Testing", result.ToString());
        }

        private void TestOpenDocumentClick(object sender, RoutedEventArgs e)
        {
            var projectPath = DTEUtils.GetActiveProjectPath();
            var documentPath = Path.Combine(projectPath, "TestFolder\\TestFile.cs");
            var arguments = new Dictionary<string, dynamic>
            {
                { "document_path", documentPath }
            };
            var result = ToolAPI.OpenDocument(arguments);
            OutputWindowHelper.LogInfo("Testing", result.ToString());
        }

        private void TestCloseDocumentClick(object sender, RoutedEventArgs e)
        {
            var projectPath = DTEUtils.GetActiveProjectPath();
            var documentPath = Path.Combine(projectPath, "TestFolder\\TestFile.cs");
            var arguments = new Dictionary<string, dynamic>
            {
                { "document_path", documentPath }
            };
            var result = ToolAPI.CloseDocument(arguments);
            OutputWindowHelper.LogInfo("Testing", result.ToString());
        }

        private void TestCreateDocumentClick(object sender, RoutedEventArgs e)
        {
            var projectName = DTEUtils.GetActiveProjectName();
            var projectPath = DTEUtils.GetActiveProjectPath();
            var documentPath = Path.Combine(projectPath, "TestFolder\\TestFile.cs");
            var arguments = new Dictionary<string, dynamic>
            {
                { "project_name", projectName },
                { "document_path", documentPath },
                { "document_text", "This is a test file" }
            };
            var result = ToolAPI.CreateDocument(arguments);
            OutputWindowHelper.LogInfo("Testing", result.ToString());
        }

        private void TestDeleteDocumentClick(object sender, RoutedEventArgs e)
        {
            var projectPath = DTEUtils.GetActiveProjectPath();
            var documentPath = Path.Combine(projectPath, "TestFolder\\TestFile.cs");
            var arguments = new Dictionary<string, dynamic>
            {
                { "document_path", documentPath }
            };
            var result = ToolAPI.DeleteDocument(arguments);
            OutputWindowHelper.LogInfo("Testing", result.ToString());
        }

        private void TestHasDocumentClick(object sender, RoutedEventArgs e)
        {
            var projectPath = DTEUtils.GetActiveProjectPath();
            var documentPath = Path.Combine(projectPath, "TestFolder\\TestFile.cs");
            var arguments = new Dictionary<string, dynamic>
            {
                { "document_path", documentPath }
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
            var result = ToolAPI.GetActiveProjectPath(new Dictionary<string, dynamic>());
            OutputWindowHelper.LogInfo("Testing", result.ToString());
        }
        #endregion
    }
}