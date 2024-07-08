# SetSelection.py

import json
from ToolBase import *

class SetSelection(ToolBase):
    name = "SetSelection"
    description = "Overwrite selected text in active document."
    category = "Programming"
    arguments = [
            Argument("selection_text", "string", "Text to replace current selection with."),
        ]

    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
