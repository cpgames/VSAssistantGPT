# GetFiles.py

import json
from ToolBase import ToolBase


class GetFiles(ToolBase):
    name = "GetFiles"
    description = "Gets all files in the project."
    category = "Programming"

    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
