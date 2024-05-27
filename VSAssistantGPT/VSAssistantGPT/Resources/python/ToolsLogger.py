# ToolsLogger.py

import os
import logging

current_directory = os.path.dirname(os.path.abspath(__file__))
log_directory = os.path.join(current_directory, '..', 'Logs')
log_file = os.path.join(log_directory, 'tools.log')

if not os.path.exists(log_directory):
    os.makedirs(log_directory)
    
logging.basicConfig(
    filename=log_file,
    filemode='a',  # Append to the existing log, change to 'w' if you want to overwrite each time
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    datefmt='%Y-%m-%d %H:%M:%S',
    level=logging.DEBUG
)

logger = logging.getLogger('Tools')

def log_info(message):
    logger.info(message)
    
def log_debug(message):
    logger.debug(message)
    
def log_error(message):
    logger.error(message)
    
def clear_log():
    with open(log_file, 'w'):
        pass
