# CreateReadme.py

import json
import os
from ToolBase import *

class CreateReadme(ToolBase):
    name = "CreateReadme"
    description = "Creates readme.md for the project"
    category = "Custom"
    arguments = [
        Argument("project_description", "string", "Description of the project.")
    ]

    def call(self, function, tool_callback):
        get_project_path_callback_json = '{"name":"GetProjectPath","arguments":[]}'
        result_json = tool_callback(get_project_path_callback_json).Result
        result = json.loads(result_json)
        project_path = result.get("project_path")
        # create readme.md in project path
        readme_path = os.path.join(project_path, "readme.md")
        with open(readme_path, "w") as readme_file:
            arguments_json = function.get("arguments")
            arguments = json.loads(arguments_json)
            project_description = arguments.get("project_description")
            readme_file.write(project_description)
        return {"result": "success"}
