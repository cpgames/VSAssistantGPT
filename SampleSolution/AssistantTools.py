# AssistantTools.py

import json


def test_call_func(func_name):
    return "Function called: " + func_name


def register_tool_callback(callback):
    global tool_callback
    tool_callback = callback


def create_task(function):
    return tool_callback(json.dumps(function))


def get_project_description(function):
    return tool_callback(json.dumps(function))


def call_tool_function(tool_call_json):
    tool_call = json.loads(tool_call_json)
    function = tool_call.get("function", {})
    name = function.get("name", "")
    arguments = function.get("arguments", {})
    if name == "create_task":
        return create_task(function)
    if name == "get_project_description":
        return get_project_description(function)
    return "No callback registered"
