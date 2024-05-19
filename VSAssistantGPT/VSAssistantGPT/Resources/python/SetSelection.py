# SetSelection.py

import json
from ToolBase import *

class SetSelection(ToolBase):
    name = "SetSelection"
    description = "Sets selected text in file."
    category = "Programming"
    arguments = [
            Argument("text", "string", "Text to replace current selection with."),
        ]

    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
