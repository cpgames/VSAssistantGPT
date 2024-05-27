# HasSelection.py

import json
from ToolBase import ToolBase

class HasSelection(ToolBase):
    name = "HasSelection"
    description = "Check if there is a selection in active document."
    category = "Programming"

    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
