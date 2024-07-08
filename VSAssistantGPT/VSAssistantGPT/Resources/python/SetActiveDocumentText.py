# SetActiveDocumentText.py

import json
from ToolBase import *


class SetActiveDocumentText(ToolBase):
    name = "SetActiveDocumentText"
    description = "Set text in active document."
    category = "Programming"
    arguments = [
        Argument("document_text", "string", "Text to replace contents of the document with."),
    ]

    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
