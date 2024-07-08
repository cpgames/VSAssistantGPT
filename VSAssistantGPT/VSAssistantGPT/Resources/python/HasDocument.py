# HasDocument.py

import json
from ToolBase import *

class HasFile(ToolBase):
    name = "HasDocument"
    description = "Check if a document exists."
    category = "Programming"
    arguments = [
        Argument("document_path", "string", "Absolute path of the document.")
        ]
    
    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
