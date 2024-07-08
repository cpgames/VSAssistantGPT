# ListDocuments.py

import json
from ToolBase import ToolBase


class ListDocuments(ToolBase):
    name = "ListDocuments"
    description = "List all documents in solution."
    category = "Programming"

    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
