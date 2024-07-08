# GetDocumentText.py

import json
from ToolBase import *


class GetFileText(ToolBase):
    name = "GetDocumentText"
    description = "Get document text."
    category = "Programming"
    arguments = [
        Argument("document_path", "string", "Absolute path of the document.")
    ]

    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
