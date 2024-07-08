# DeleteDocument.py

import json
from ToolBase import *

class DeleteDocument(ToolBase):
    name = "DeleteDocument"
    description = "Delete a document."
    category = "Programming"
    arguments = [
            Argument("project_name", "string", "Name of the project to delete document from."),
            Argument("document_path", "string", "Absolute path of the document.")
        ]
    
    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
