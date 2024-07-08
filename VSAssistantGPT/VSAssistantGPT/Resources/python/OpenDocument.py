# OpenDocument.py

import json
from ToolBase import *


class OpenDocument(ToolBase):
    name = "OpenDocument"
    description = "Open a document."
    category = "Programming"
    arguments = [
        Argument("document_path", "string", "Absolute path of the document.")
    ]

    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
