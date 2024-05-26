# CreateDocument.py

import json
from ToolBase import *

class CreateDocument(ToolBase):
    name = "CreateDocument"
    description = "Create a document with text."
    category = "Programming"
    arguments = [
            Argument("documentPath", "string", "Relative path of the document."),
            Argument("text", "string", "Text to write to the document."),
        ]
    
    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
