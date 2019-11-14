import sys
import requests
import json
import tarfile

url = "https://yamato-api.cds.internal.unity3d.com/jobs"
srp_revision = ""
#unity_revision = sys.argv[2]
#api_key = sys.argv[3]
#branch_name = sys.argv[4]


package_path = 'External/PackageManager/Editor/'


with open('manifest.json') as f:
  manifest = json.load(f)

packages = manifest['packages']

core_package = packages['com.unity.render-pipelines.core']

version = core_package['minimumVersion']

package_name = 'com.unity.render-pipelines.core' + '-' + version + '.tgz'


package = ""
tar = tarfile.open(package_name, "r:*")
for filename in tar.getnames():
  try:
      package_tar = tar.extractfile('package/package.json')
      tar_data = package_tar.read()
      package = tar_data
  except:
      print('ERROR: Did not find %s in tar archive' % filename)


package_json = json.loads(package)
repository = package_json['repository']
srp_revision = repository['revision']
print(srp_revision)

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

#print '\n' + data + '\n'

key = 'ApiKey ' + api_key

response = requests.post(url, data=data, headers={'Authorization': key})

if(response.ok):
    print "ok"
else:
    response.raise_for_status()
    print "oh no"