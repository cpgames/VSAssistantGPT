# GetActiveDocumentText.py

import json
from ToolBase import *


class GetActiveDocumentText(ToolBase):
    name = "GetActiveDocumentText"
    description = "Get text of active document."
    category = "Programming"

    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
