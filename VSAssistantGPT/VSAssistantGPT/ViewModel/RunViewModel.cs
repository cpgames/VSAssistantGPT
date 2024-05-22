using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using cpGames.VSA.RestAPI;

namespace cpGames.VSA.ViewModel
{
    public class RunViewModel : ViewModel<RunModel>
    {
        private ThreadViewModel? _thread;
        private AssistantViewModel? _assistant;

        #region Properties
        public Action? RunStarted { get; set; }
        public Action? RunEnded { get; set; }
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

        public ThreadViewModel? Thread
        {
            get => _thread;
            set
            {
                if (_thread != value)
                {
                    _thread = value;
                    OnPropertyChanged();
                }
            }
        }

        public AssistantViewModel Assistant
        {
            get => _assistant!;
            set
            {
                if (_assistant != value)
                {
                    _assistant = value;
                    OnPropertyChanged();
                }
            }
        }

        public RunStatus Status
        {
            get => _model.status;
            set
            {
                if (_model.status != value)
                {
                    _model.status = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Ended => Status is RunStatus.completed or RunStatus.failed or RunStatus.cancelled or RunStatus.expired;
        #endregion

        #region Constructors
        public RunViewModel(RunModel model) : base(model) { }
        #endregion

        #region Methods
        public async Task CreateAsync()
        {
            if (Thread == null)
            {
                throw new Exception("Thread has not been set");
            }
            if (Assistant == null)
            {
                throw new Exception("Assistant has not been set");
            }
            RunStarted?.Invoke();
            var request = new CreateRunRequest(Thread.Model, Assistant.Model);
            var response = await request.SendAsync();
            Id = response.id;
            UpdateStatus(response.status.ToString());
            do
            {
                await Task.Delay(500);
                await GetAsync();
            } while (!Ended);
            RunEnded?.Invoke();
        }

        public async Task GetAsync()
        {
            if (Id == null)
            {
                throw new Exception("Run has not been created");
            }
            if (Thread == null)
            {
                throw new Exception("Thread has not been set");
            }
            var request = new GetRunRequest(Thread.Id, Id);
            var response = await request.SendAsync();
            UpdateStatus(response.status.ToString());
            if (Status == RunStatus.requires_action)
            {
                var required_action = response["required_action"];
                var submit_tool_outputs = required_action["submit_tool_outputs"];
                var toolCalls = submit_tool_outputs["tool_calls"];
                Dictionary<string, string> outputs = new();
                foreach (var toolCall in toolCalls)
                {
                    outputs[toolCall["id"]!.ToString()] = await ToolAPI.HandleToolCallAsync(toolCall);
                }
                await SubmitToolOutputsAsync(outputs);
            }
        }

        public async Task SubmitToolOutputsAsync(Dictionary<string, string> outputs)
        {
            if (Id == null)
            {
                throw new Exception("Run has not been created");
            }
            if (Thread == null)
            {
                throw new Exception("Thread has not been set");
            }
            var request = new SubmitToolOutputsRequest(Thread.Id, Id, outputs);
            var response = await request.SendAsync();
            UpdateStatus(response.status.ToString());
        }

        public void UpdateStatus(string status)
        {
            Status = (RunStatus)Enum.Parse(typeof(RunStatus), status);
        }
        #endregion
    }
}