# DeleteDocument.py

import json
from ToolBase import *

class DeleteDocument(ToolBase):
    name = "DeleteDocument"
    description = "Delete a document."
    category = "Programming"
    arguments = [
            Argument("relative_path", "string", "Relative path of the document."),
        ]
    
    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
