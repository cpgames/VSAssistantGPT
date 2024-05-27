# AssistantTools.py

import importlib
import inspect
import json
import os
import sys
import subprocess
from ToolBase import ToolBase
from ToolsLogger import log_info, clear_log

tool_callback = None
tools = {}

def register_tool_callback(callback):
    global tool_callback
    tool_callback = callback

def load_tools():
    log_info("loading tools...")
    global tools
    tools = {}
    current_directory = os.path.dirname(os.path.abspath(__file__))
    for filename in os.listdir(current_directory):
        if (
                filename.endswith(".py") and 
                filename[0].isupper() and 
                filename != "ToolBase.py" and 
                filename != "ToolsLogger.py" and 
                filename != "AssistantTools.py"
            ):
            module_name = filename[:-3]
            # Check if module is already loaded
            if module_name in sys.modules:
                # Reload the module if it's already loaded
                log_info(f"Reloading module: {module_name}")
                module = importlib.reload(sys.modules[module_name])
            else:
                # Import the module if it's not loaded
                log_info(f"Importing module: {module_name}")
                module = importlib.import_module(module_name)
            for name, obj in inspect.getmembers(module):
                if inspect.isclass(obj) and issubclass(obj, ToolBase) and obj is not ToolBase:
                    tools[obj.name] = obj()
                    log_info(f"Tool loaded: {obj.name}")

def call_tool_function(tool_call_json):
    tool_call = json.loads(tool_call_json)
    function = tool_call.get("function", {})
    name = function.get("name", "")
    for tool in tools.values():
        if hasattr(tool, 'name') and tool.name == name:
            return tool.call(function, tool_callback)
    return "No callback registered"

def get_tools_info():
    tools_info = [tool.get_info() for tool in tools.values()]
    tools_info_json = json.dumps(tools_info, indent=4)
    return tools_info_json

def create_tool(tool_json):
    tool_info = json.loads(tool_json)
    name = tool_info.get("name")
    file_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), f"{name}.py")
    if os.path.exists(file_path):
        return f"Tool '{name}' already exists"
    
    with open(file_path, "w") as file:
        file.write(f"# {name}.py\n\n")
        file.write("import json\n")
        file.write("from ToolsLogger import log_info\n")
        file.write("from ToolBase import *\n\n")
        file.write(f"class {name}(ToolBase):\n")
        file.write(f"    name = '{name}'\n")
        file.write(f"    description = '{tool_info.get('description', '')}'\n")
        file.write(f"    category = '{tool_info.get('category', '')}'\n")
        file.write(f"    arguments = [\n")
        file.write(f"        Argument('myArgument', 'string', 'Description of the argument, used by GPT.')\n")
        file.write(f"    ]\n\n")
        file.write("    def call(self, function, tool_callback):\n")
        file.write(f"        log_info('Calling {name}')\n")
        file.write("        return {'result': 'error', 'message': 'Not implemented'}\n")
    
    os.startfile(file_path)
    
    load_tools()
    return ""

def open_tool(name):
    file_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), f"{name}.py")
    if not os.path.exists(file_path):
        return f"Tool '{name}' not found"
    
    os.startfile(file_path)
    return ""

def save_tool(original_name, tool_json):
    tool_info = json.loads(tool_json)
    new_name = tool_info.get("name")
    original_file_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), f"{original_name}.py")
    new_file_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), f"{new_name}.py")
    if not os.path.exists(original_file_path):
        return create_tool(tool_json)

    with open(original_file_path, "r") as file:
        lines = file.readlines()

    with open(new_file_path, "w") as file:
        for line in lines:
            if f"{original_name}" in line:
                file.write(line.replace(original_name, new_name))
            elif "name =" in line:
                file.write(f"    name = '{new_name}'\n")
            elif "description =" in line:
                file.write(f"    description = '{tool_info.get('description', '')}'\n")
            elif "category =" in line:
                file.write(f"    category = '{tool_info.get('category', '')}'\n")
            elif f"class {original_name}(ToolBase):" in line:
                file.write(f"class {new_name}(ToolBase):\n")
            else:
                file.write(line)

    if original_name != new_name:
        os.remove(original_file_path)
    
    load_tools()
    return ""

def open_tool(name):
    file_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), f"{name}.py")
    if not os.path.exists(file_path):
        return f"Tool '{name}' not found"
    
    os.startfile(file_path)
    return ""

def remove_tool(name):
    file_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), f"{name}.py")
    if not os.path.exists(file_path):
        return f"Tool '{name}' not found"
    
    os.remove(file_path)
    load_tools()
    return ""

def reload_tools():
    load_tools()
    return ""

clear_log()

# Load all tools at startup
load_tools()
