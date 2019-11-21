import sys
import requests
import json
import subprocess

# Delete this file

url = "https://yamato-api.cds.internal.unity3d.com/jobs/769241"

key = 'ApiKey ' + key

#response = requests.post(url, data=data, headers={'Authorization': key})

test = requests.get(url, headers={'Authorization': key})
print(test)

#job = json.loads(test)
job = test.json()
jobdef = job['jobDefinitionName']
print(jobdef)