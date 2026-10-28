"""Authenticated successor to the P1 web suite; runs only in a fresh isolated DB."""
import runpy
from pathlib import Path
runpy.run_path(str(Path(__file__).with_name('Test-Workflow.py')), run_name='__main__')
