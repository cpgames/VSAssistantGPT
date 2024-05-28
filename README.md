# VSAssistantGPT
Visual Studio extension utilizing OpenAI GPT assistants

## Setup
1. Open Visual Studio
2. Tools->Show VS Assistant
3. In VS Assistant, go to `Settings` tab.
4. Set your OpenAI `API key`.
5. Set `Python Dll` path. E.g. C:\Users\{username}\miniconda3\python39.dll
6. Save settings ![image](https://github.com/cpgames/VSAssistantGPT/assets/49317353/8b37325c-c5c3-4c07-b2f4-4addb70453e8).

## Uploading Project Resources (optional)
To better understand the project, VS assistant can use project's source files. They need to be uploaded manually.
1. In VS Assistant, go to `Resources` tab.
2. Expand the `Files` section. This shows all files uploaded for the project.
3. Click `sync` ![image](https://github.com/cpgames/VSAssistantGPT/assets/49317353/3c52cf71-4a87-483a-aeea-15985fb212bd) button.
4. Wait for file upload to finish.
5. Expand `File Stores` section. This shows individual file sets used by assistant.
6. Create new `File Store` by clicking the plus icon ![image](https://github.com/cpgames/VSAssistantGPT/assets/49317353/4009d44d-9c8b-4e8b-83ca-53db14c7aa77).
7. From `Files` section, select the files to add to `File Store`.
8. Select files to add to the store (top checkbox selects all), and click `sync` ![image](https://github.com/cpgames/VSAssistantGPT/assets/49317353/3c52cf71-4a87-483a-aeea-15985fb212bd) button in `File Store` section.

Note: when making large changes to the project, it may make sense to reupload updated files.

## Creating Assistant
You can create as many assistants as you need to acompish different kinds of tasks.
1. In VS Assistant, go to `Assistants` tab.
2. Create new assistant by clicking the plus icon ![image](https://github.com/cpgames/VSAssistantGPT/assets/49317353/4009d44d-9c8b-4e8b-83ca-53db14c7aa77).
3. Configure assistant as needed. For best results, I recommend `gpt-4o` model. It is more expensive than `gpt-3.5-turbo-0125`, but produces much better results. `gpt-3.5-turbo-0125` is also known for struggling with `file_search` tool.
4. Assign a set of required tools to the assistant. E.g. if it needs to be able to read current text selection, add `GetSelection` tool, or if it needs to create new files, add `CreateDocument` tool, etc.
5. You can see all tool information in [here](https://github.com/cpgames/VSAssistantGPT/tree/main/VSAssistantGPT/VSAssistantGPT/Resources/python).
6. Assign `File Store` the assistant will use for file_search.
7. Save assistant ![image](https://github.com/cpgames/VSAssistantGPT/assets/49317353/6cd0f905-1602-46e3-bffc-3d3cc60e3f86).

Note: Adding more tools will increase assistant's capabilities, but may also confuse it when selecting the right tool for the job. Feel free to experiment.

## Interacting with Assistant
1. In VS Assistant, go to `Chat` tab.
2. Select the assistant to interact with.
3. Type a prompt and press Enter, or click `Submit` button ![image](https://github.com/cpgames/VSAssistantGPT/assets/49317353/c253abe6-3eb3-4655-869c-400a695154e8).
4. To add multiple lines to your message, press Shift+Enter.
5. Wait for assistant to finish responding (`thinking...` message will disappear).
6. To start new conversation and erase current one, click on `Refrersh`![image](https://github.com/cpgames/VSAssistantGPT/assets/49317353/3bafb799-581d-47a0-b25c-c06f8d35c80b) button.

Note: Prompt engineering is important to achieve better results. You can specify specific tools to call, or give hints as to the direction the assistant should take.

## Interacting with Assistant in Editor Window
You can bring up a minimal chat window directly from the text editor, instead of using full VS Assistant window.
1. Make sure assistant has at least `GetSelection` tool.
2. Select text in editor.
3. Click on robot glyph on the left.
![image](https://github.com/cpgames/VSAssistantGPT/assets/49317353/01582fa7-eb1c-418f-8004-2839b22dee88)
4. You can move the window by dragging its title, or close it by clicking outside.

## Creating Custom Tools
1. In VS Assistant, go to `Tools` tab.
2. Create new tool by clicking the plus icon ![image](https://github.com/cpgames/VSAssistantGPT/assets/49317353/4009d44d-9c8b-4e8b-83ca-53db14c7aa77).
3. Set tool name.
4. Save tool ![image](https://github.com/cpgames/VSAssistantGPT/assets/49317353/6cd0f905-1602-46e3-bffc-3d3cc60e3f86).
5. You should see tool automatically open in your text editor with template code.
6. Implement the `call` function, and add required `arguments` (if tool does not require arguments, you can safely delete this field).
7. Use `ToolsLogger` if you need to debug the code, log is located in `C:\Users\{username}\AppData\Local\cpGames\VSA\Logs`
8. Save python file.
9. In the `Tools` tab, click `Refresh` ![image](https://github.com/cpgames/VSAssistantGPT/assets/49317353/3bafb799-581d-47a0-b25c-c06f8d35c80b) button to reload the toolset. This reloads all tool modules in case you've made changes to them without requiring to restart Visual Studio.

Note: If you would like to call any of built-in VSIX functions, refer to [ToolAPI class](https://github.com/cpgames/VSAssistantGPT/blob/main/VSAssistantGPT/VSAssistantGPT/VSAPI/ToolAPI.cs), specifically functions of return type and parameters like these `public static JObject HasSelection(Dictionary<string, dynamic> arguments)` (they are at the bottom of the file).

## Debugging Assistant Errors
All output is printed to Output window (`View->Output->AssistantGPT`).
Additionally python tools output is logged in  `C:\Users\{username}\AppData\Local\cpGames\VSA\Logs`.
