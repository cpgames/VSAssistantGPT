# SetFileText.py

import json
from ToolBase import *

class SetFileText(ToolBase):
    name = "SetFileText"
    description = "Sets text to file."
    category = "Programming"
    arguments = [
            Argument("filename", "string", "Name of the file to set text to."),
            Argument("text", "string", "Text to replace contents of the file with."),
        ]
    
    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
