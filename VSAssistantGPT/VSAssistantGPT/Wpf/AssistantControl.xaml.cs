using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using cpGames.VSA.RestAPI;
using cpGames.VSA.ViewModel;

namespace cpGames.VSA.Wpf
{
    /// <summary>
    /// Interaction logic for AssistantControl.xaml
    /// </summary>
    public partial class AssistantControl : UserControl
    {
        #region Properties
        public AssistantViewModel? ViewModel
        {
            get => DataContext as AssistantViewModel;
            set => DataContext = value;
        }
        #endregion

        #region Constructors
        public AssistantControl()
        {
            InitializeComponent();
        }
        #endregion

        #region Methods
        private async void AddToolClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
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

            if (ProjectUtils.ActiveProject.Toolset.Count == 0)
            {
                await ProjectUtils.ActiveProject.LoadToolsetAsync();
            }

            var toolset = ProjectUtils.ActiveProject.Toolset;
            var contextMenu = new ContextMenu
            {
                Background = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                Template = menuTemplate
            };

            var toolCategories = toolset.Select(x => x.Category).Distinct();
            foreach (var category in toolCategories)
            {
                var background = ToolUtils.GetColor(category, true);
                var categoryMenuItem = new MenuItem
                {
                    Header = category,
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    Background = background,
                    Template = itemTemplate
                };
                contextMenu.Items.Add(categoryMenuItem);
                var categoryTools = toolset.Where(x => x.Category == category);
                var addAllMenuItem = new MenuItem
                {
                    Header = "** Add All **",
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    Background = background,
                    Template = itemTemplate
                };
                addAllMenuItem.Click += (o, args) =>
                {
                    foreach (var tool in categoryTools)
                    {
                        if (ViewModel.Toolset.Any(x => x.Name == tool.Name))
                        {
                            continue;
                        }
                        ViewModel.AddTool(tool.Model);
                    }
                };
                categoryMenuItem.Items.Add(addAllMenuItem);
                foreach (var tool in categoryTools)
                {
                    if (ViewModel.Toolset.Any(x => x.Name == tool.Name))
                    {
                        continue;
                    }
                    var menuItem = new MenuItem
                    {
                        Header = tool.Name,
                        Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                        Background = background,
                        Template = itemTemplate
                    };
                    menuItem.Click += (o, args) =>
                    {
                        ViewModel.AddTool(tool.Model);
                    };
                    categoryMenuItem.Items.Add(menuItem);
                }
            }
            contextMenu.IsOpen = true;
        }

        private async void RemoveAssistantClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }
            await ViewModel.DeleteAsync();
        }

        private async void SaveAssistantClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }
            await ViewModel.SaveAsync();
        }

        private async void SelectVectorStoreClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
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
            if (ProjectUtils.ActiveProject.VectorStores.Count == 0)
            {
                await ProjectUtils.ActiveProject.LoadVectorStoresAsync();
            }
            var contextMenu = new ContextMenu
            {
                Background = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                Template = menuTemplate
            };
            foreach (var vectorStore in ProjectUtils.ActiveProject.VectorStores)
            {
                var menuItem = new MenuItem
                {
                    Header = vectorStore.Id,
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    Background = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    Template = itemTemplate
                };
                menuItem.Click += (s, a) =>
                {
                    ViewModel.VectorStoreId = vectorStore.Id;
                };
                contextMenu.Items.Add(menuItem);
            }
            contextMenu.IsOpen = true;
        }
        #endregion

        private async void SelectModelClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }
            if (!ProjectUtils.ActiveProject.ValidateSettings())
            {
                return;
            }
            var listModelsRequest = new ListModelsRequest();
            var listModelsResponse = await listModelsRequest.SendAsync();
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
            var contextMenu = new ContextMenu
            {
                Background = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                Template = menuTemplate
            };
            foreach (var model in listModelsResponse.data)
            {
                var id = model.id.ToString();
                if (!id.Contains("gpt"))
                {
                    continue;
                }
                var menuItem = new MenuItem
                {
                    Header = id,
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    Background = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    Template = itemTemplate
                };
                menuItem.Click += (s, a) =>
                {
                    ViewModel.GPTModel = id;
                };
                contextMenu.Items.Add(menuItem);
            }
            contextMenu.IsOpen = true;
        }
    }
}