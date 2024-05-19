﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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

        public ObservableCollection<MessageViewModel> Messages { get; } = new();

        public Func<Task>? MessagePosted { get; set; }
        #endregion

        #region Constructors
        public ThreadViewModel(ThreadModel model) : base(model) { }
        #endregion

        #region Methods
        public async Task CreateAsync()
        {
            var request = new CreateThreadRequest();
            var response = await request.SendAsync();
            Id = response.id;
        }

        public async Task DeleteAsync()
        {
            if (string.IsNullOrEmpty(Id))
            {
                throw new Exception("Thread has not been created");
            }
            var request = new DeleteThreadRequest(Id);
            await request.SendAsync();
            Id = string.Empty;
            Messages.Clear();
            RemoveAction?.Invoke();
        }

        public async Task PostMessageAsync(string content)
        {
            if (string.IsNullOrEmpty(Id))
            {
                await CreateAsync();
            }
            var request = new PostMessageRequest(Id, content, new List<string>());
            await request.SendAsync();
            await UpdateMessagesAsync();
            await RunThread();
        }

        public async Task UpdateMessagesAsync()
        {
            if (string.IsNullOrEmpty(Id))
            {
                throw new Exception("Thread has not been created");
            }
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

        public async Task RunThread()
        {
            if (string.IsNullOrEmpty(Id))
            {
                throw new Exception("Thread has not been created");
            }
            if (Assistant == null)
            {
                throw new Exception("Assistant has not been set");
            }
            var run = new RunModel
            {
                assistantId = Assistant.Id,
                threadId = Id
            };
            Run = new RunViewModel(run);
            await Run.CreateAsync();
            await UpdateMessagesAsync();
        }
        #endregion
    }
}