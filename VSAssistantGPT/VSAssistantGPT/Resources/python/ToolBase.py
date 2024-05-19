# ToolBase.py

class Argument:
    def __init__(self, name, type_, description):
        self.name = name
        self.type = type_
        self.description = description
        
    def to_dict(self):
        return {
            "name": self.name,
            "type": self.type,
            "description": self.description
        }
        
class ToolBase:
    name = ""
    description = ""
    category = ""
    arguments = []

    def get_info(self):
        return {
            "name": self.name,
            "description": self.description,
            "category": self.category,
            "arguments": [arg.to_dict() for arg in self.arguments]
        }

    def call(self, function, tool_callback):
        raise NotImplementedError("Each derived tool must implement the `call` method.")
    
