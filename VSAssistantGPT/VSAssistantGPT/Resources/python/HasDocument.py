# HasDocument.py

import json
from ToolBase import *

class HasFile(ToolBase):
    name = "HasDocument"
    description = "Check if a document exists."
    category = "Programming"
    arguments = [
        Argument("documentPath", "string", "Relative path of the document."),
        ]
    
    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
