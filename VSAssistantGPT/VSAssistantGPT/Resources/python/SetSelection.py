# SetSelection.py

import json
from ToolBase import *

class SetSelection(ToolBase):
    name = "SetSelection"
    description = "Overwrites selected text in file. When making an edit make sure to call this function with entire selection text (use GetSelection tool), not just the text you want to modify."
    category = "Programming"
    arguments = [
            Argument("text", "string", "Text to replace current selection with."),
        ]

    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
