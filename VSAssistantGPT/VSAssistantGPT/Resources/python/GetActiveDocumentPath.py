# GetActiveDocumentPath.py

import json
from ToolBase import *


class GetActiveDocumentPath(ToolBase):
    name = "GetActiveDocumentPath"
    description = "Get absolute path of active document."
    category = "Programming"

    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
