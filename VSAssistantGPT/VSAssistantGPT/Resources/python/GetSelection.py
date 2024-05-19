# GetSelection.py

import json
from ToolBase import ToolBase

class GetSelection(ToolBase):
    name = "GetSelection"
    description = "Gets selected text in file."
    category = "Programming"

    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
