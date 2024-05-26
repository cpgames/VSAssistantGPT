# SetDocumentText.py

import json
from ToolBase import *


class SetDocumentText(ToolBase):
    name = "SetDocumentText"
    description = "Set document text."
    category = "Programming"
    arguments = [
        Argument("documentPath", "string", "Relative path of the document."),
        Argument("text", "string", "Text to replace contents of the document with."),
    ]

    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
