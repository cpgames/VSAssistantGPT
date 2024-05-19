# DeleteFile.py

import json
from ToolBase import *

class DeleteFile(ToolBase):
    name = "DeleteFile"
    description = "Deletes a file."
    category = "Programming"
    arguments = [
            Argument("filename", "string", "Name of the file to delete."),
        ]
    
    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
