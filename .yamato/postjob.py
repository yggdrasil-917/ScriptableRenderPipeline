import sys
import requests
import json
import tarfile
import subprocess

url = "https://yamato-api.cds.internal.unity3d.com/jobs"
srp_revision = ""

#hg log -r . --template "{node}"
unity_revision = ""

try:
    HG_REV = subprocess.check_output(['hg', 'id', '-i']).strip()
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

#do i want the minimum version here?
version = core_package['minimumVersion']

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

key = 'ApiKey ' + api_key

response = requests.post(url, data=data, headers={'Authorization': key})

if(response.ok):
    print("ok")
else:
    response.raise_for_status()
    print("oh no")