# GetSelection.py

import json
from ToolBase import ToolBase

class GetSelection(ToolBase):
    name = "GetSelection"
    description = "Get selected text in active document."
    category = "Programming"

    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
