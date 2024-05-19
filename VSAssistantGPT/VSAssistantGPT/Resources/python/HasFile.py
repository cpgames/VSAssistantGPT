# HasFile.py

import json
from ToolBase import *

class HasFile(ToolBase):
    name = "HasFile"
    description = "Checks if a file exists."
    category = "Programming"
    arguments = [
            Argument("filename", "string", "Name of the file to check."),
        ]
    
    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
