using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using cpGames.VSA.RestAPI;
using Newtonsoft.Json.Linq;

namespace cpGames.VSA.ViewModel
{
    public class ThreadViewModel : ViewModel<ThreadModel>
    {
        #region Fields
        private RunViewModel? _run;
        private AssistantViewModel? _assistant;
        private string _assistantName = "[None]";
        private Visibility _isThinking = Visibility.Collapsed;
        #endregion

        #region Properties
        public Action? RemoveAction { get; set; }
        public string Id
        {
            get => _model.id;
            set
            {
                if (_model.id != value)
                {
                    _model.id = value;
                    OnPropertyChanged();
                }
            }
        }

        public RunViewModel? Run
        {
            get => _run;
            set
            {
                if (_run != value)
                {
                    _run = value;
                    OnPropertyChanged();
                }
            }
        }

        public AssistantViewModel? Assistant
        {
            get => _assistant;
            set
            {
                if (_assistant != value)
                {
                    _assistant = value;
                    OnPropertyChanged();
                    AssistantName = _assistant?.Name ?? "[None]";
                }
            }
        }

        public string AssistantName
        {
            get => _assistantName;
            set
            {
                if (_assistantName != value)
                {
                    _assistantName = value;
                    OnPropertyChanged();
                }
            }
        }

        public Visibility IsThinking
        {
            get => _isThinking;
            set
            {
                if (_isThinking != value)
                {
                    _isThinking = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<MessageViewModel> Messages { get; } = new();

        public Func<Task>? MessagePosted { get; set; }
        #endregion

        #region Constructors
        public ThreadViewModel(ThreadModel model) : base(model) { }
        #endregion

        #region Methods
        public async Task CreateAsync()
        {
            if (!ProjectUtils.ActiveProject.ValidateSettings())
            {
                return;
            }
            try
            {
                var request = new CreateThreadRequest();
                var response = await request.SendAsync();
                Id = response.id;
            }
            catch (Exception e)
            {
                await OutputWindowHelper.LogErrorAsync(e);
                ProjectUtils.ActiveProject.Working = false;
            }
        }

        public async Task DeleteAsync()
        {
            if (string.IsNullOrEmpty(Id))
            {
                await OutputWindowHelper.LogErrorAsync("Thread has not been created.");
                return;
            }
            if (!ProjectUtils.ActiveProject.ValidateSettings())
            {
                return;
            }
            try
            {
                var request = new DeleteThreadRequest(Id);
                await request.SendAsync();
                Id = string.Empty;
                Messages.Clear();
            }
            catch (Exception e)
            {
                await OutputWindowHelper.LogErrorAsync(e);
                ProjectUtils.ActiveProject.Working = false;
                return;
            }
            RemoveAction?.Invoke();
        }

        public async Task PostMessageAsync(string content)
        {
            if (string.IsNullOrEmpty(Id))
            {
                await CreateAsync();
            }
            if (!ProjectUtils.ActiveProject.ValidateSettings())
            {
                return;
            }
            try
            {
                var request = new PostMessageRequest(Id, content, new List<string>());
                await request.SendAsync();
            }
            catch (Exception e)
            {
                await OutputWindowHelper.LogErrorAsync(e);
                ProjectUtils.ActiveProject.Working = false;
                return;
            }
            await UpdateMessagesAsync();
            await RunThreadAsync();
        }

        public async Task UpdateMessagesAsync()
        {
            if (string.IsNullOrEmpty(Id))
            {
                await OutputWindowHelper.LogErrorAsync("Thread has not been created.");
                return;
            }
            if (!ProjectUtils.ActiveProject.ValidateSettings())
            {
                return;
            }
            try
            {
                var request = new ListMessagesRequest(Id);
                var response = await request.SendAsync();
                Messages.Clear();
                foreach (var message in response.data)
                {
                    JArray content = message.content;
                    if (content.Count == 0)
                    {
                        continue;
                    }
                    var text = content[0]["text"]!["value"]!.ToString();
                    var messageModel = new MessageModel
                    {
                        id = message.id,
                        role = message.role,
                        text = text
                    };
                    Messages.Add(new MessageViewModel(messageModel));
                }
            }
            catch (Exception e)
            {
                await OutputWindowHelper.LogErrorAsync(e);
                ProjectUtils.ActiveProject.Working = false;
            }
        }

        public async Task RunThreadAsync()
        {
            if (string.IsNullOrEmpty(Id))
            {
                await OutputWindowHelper.LogErrorAsync("Thread has not been created.");
                return;
            }
            if (Assistant == null)
            {
                await OutputWindowHelper.LogErrorAsync("Assistant has not been set.");
                return;
            }
            if (!ProjectUtils.ActiveProject.ValidateSettings())
            {
                return;
            }
            var run = new RunModel();
            Run = new RunViewModel(run)
            {
                Assistant = Assistant,
                Thread = this
            };
            Run.RunStarted += () => IsThinking = Visibility.Visible;
            Run.RunEnded += () =>
            {
                IsThinking = Visibility.Collapsed;
                Run = null;
            };
            await Run.CreateAsync();
            await UpdateMessagesAsync();
        }
        #endregion
    }
}