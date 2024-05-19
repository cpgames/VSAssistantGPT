# HasSelection.py

import json
from ToolBase import ToolBase

class HasSelection(ToolBase):
    name = "HasSelection"
    description = "Returns true if there is selected text in file."
    category = "Programming"

    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
