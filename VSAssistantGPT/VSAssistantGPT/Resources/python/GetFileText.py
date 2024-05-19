# GetFileText.py

import json
from ToolBase import *


class GetFileText(ToolBase):
    name = "GetFileText"
    description = "Gets all text in file."
    category = "Programming"
    arguments = [
        Argument("filename", "string", "Name of the file to get text from."),
    ]

    def call(self, function, tool_callback):
        return tool_callback(json.dumps(function)).Result
