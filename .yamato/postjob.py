import sys
import requests
import json
import subprocess
import time

url = "https://yamato-api.cds.internal.unity3d.com/jobs"
srp_revision = ""

unity_revision = ""

try:
    HG_REV = subprocess.check_output(['hg', 'log', '-r', '.', '--template', '{node}']).strip()
except OSError:
    HG_REV = "? (Couldn't find hg)"
except subprocess.CalledProcessError as e:
    HG_REV = "? (Error {})".format(e.returncode)
except Exception:
    # should never have to deal with a hangup 
    HG_REV = "???"

unity_revision = HG_REV

package_path = 'External/PackageManager/Editor/'


with open('manifest.json') as f:
  manifest = json.load(f)

packages = manifest['packages']

core_package = packages['com.unity.render-pipelines.core']

version = core_package['version']

package_url = 'https://artifactory.prd.cds.internal.unity3d.com/artifactory/api/npm/upm-candidates-master/com.unity.render-pipelines.core/' + version

package = requests.get(package_url)

package = package.text

package_json = json.loads(package)
repository = package_json['repository']
srp_revision = repository['revision']

data = '''{
  "source": {
    "branchname": "master",
    "revision": "''' + srp_revision + '''"
  },
  "links": {
    "project": "/projects/78",
    "jobDefinition": "/projects/78/revisions/''' + srp_revision + '''/job-definitions/.yamato#upm-ci-abv.yml#trunk_verification"
  },
  "environmentVariables": [
    { "key": "CUSTOM_REVISION", "value": "''' + unity_revision + '''" }
]
}'''

# please do not use this api key without checking in #devs-yamato first!
key = 'ApiKey '

response = requests.post(url, data=data, headers={'Authorization': key})

# get id of the job
# wait for it to finish

response_json = response.json()

job_id = response_json[id]
print('Yamato job id: ' + job_id)

status = ''

results = ['success', 'failed', 'cancelled', 'done']

while results not in status:
  time.sleep(20)
  get_job = requests.get(url + '/' + job_id, headers={'Authorization': key})
  job_json = get_job.json()
  status = job_json['status']
  print('current job status: ' + status)

#maybe this part needs to log the yamato job link so the person checking the log can find where to look?
if status == 'success':
  sys.exit(0)
else:
  sys.exit(1)