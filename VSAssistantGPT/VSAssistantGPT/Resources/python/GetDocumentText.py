# GetDocumentText.py

import json
from ToolBase import *


class GetFileText(ToolBase):
    name = "GetDocumentText"
    description = "Get all text in a document."
    category = "Programming"
    arguments = [
        Argument("documentPath", "string", "Relative path of the document."),
    ]

    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
