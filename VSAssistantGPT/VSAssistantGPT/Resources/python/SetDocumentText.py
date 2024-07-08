# SetDocumentText.py

import json
from ToolBase import *


class SetDocumentText(ToolBase):
    name = "SetDocumentText"
    description = "Set document text."
    category = "Programming"
    arguments = [
        Argument("document_path", "string", "Absolute path of the document."),
        Argument("document_text", "string", "Text to write to the document."),
    ]

    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
