import sys
import requests
url = "https://yamato-api.cds.internal.unity3d.com/jobs"
srp_revision = sys.argv[1]
unity_revision = sys.argv[2]
job_definition = sys.argv[3]
api_key = sys.argv[4]
branch_name = sys.argv[5]
data = '''{
  "source": {
    "branchname": "''' + branch_name + '''",
    "revision": "''' + srp_revision + '''"
  },
  "links": {
    "project": "/projects/78",
    "jobDefinition": "/projects/78/revisions/''' + srp_revision + '''/job-definitions/''' + job_definition + '''"
  },
  "environmentVariables": [
    { "key": "CUSTOM_REVISION", "value": "''' + unity_revision + '''" }
]
}'''

print '\n' + data + '\n'

key = 'ApiKey ' + api_key

response = requests.post(url, data=data, headers={'Authorization': key})

if(response.ok):
    print "ok"
else:
    response.raise_for_status()
    print "oh no"