# CreateFile.py

import json
from ToolBase import *

class CreateFile(ToolBase):
    name = "CreateFile"
    description = "Creates a file with text."
    category = "Programming"
    arguments = [
            Argument("filename", "string", "Name of the file to create."),
            Argument("text", "string", "Text to write to the file."),
        ]
    
    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
