import sys
import argparse
import pandas as pd

# Add the arguments of the script
parser = argparse.ArgumentParser()
parser.add_argument("-f", "--file_path", help="The absolute path to the Excel file")
parser.add_argument("-s", "--sheet_name", help="The name of the sheet from the Excel file", default="Sheet1")

# Get the arguments from the call
args = vars(parser.parse_args())

# Convert the Excel sheet to a JSON object
excel_data_df = pd.read_excel(args["file_path"], sheet_name=args["sheet_name"])
json = excel_data_df.to_json('./export.json')

# Print the JSON object
print(json)
