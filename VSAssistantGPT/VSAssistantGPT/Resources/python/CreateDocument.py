# CreateDocument.py

import json
from ToolBase import *

class CreateDocument(ToolBase):
    name = "CreateDocument"
    description = "Create a document with text."
    category = "Programming"
    arguments = [
            Argument("project_name", "string", "Name of the project to add document to."),
            Argument("document_path", "string", "Absolute path of the document."),
            Argument("document_text", "string", "Text to write to the document."),
        ]
    
    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
