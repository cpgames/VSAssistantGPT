# ListFiles.py

import json
from ToolBase import ToolBase


class ListFiles(ToolBase):
    name = "ListFiles"
    description = "Lists all file relative paths in the project."
    category = "Programming"

    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
