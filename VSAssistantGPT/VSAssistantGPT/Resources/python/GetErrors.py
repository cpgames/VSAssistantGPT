# GetErrors.py

import json
from ToolBase import ToolBase

class GetErrors(ToolBase):
    name = "GetErrors"
    description = "Get compilation errors."
    category = "Programming"

    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
