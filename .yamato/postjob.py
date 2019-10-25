import requests
url = "https://yamato-api.cds.internal.unity3d.com/jobs"
data = '''{
  "source": {
    "branchname": "ci/custom-unity-revision",
    "revision": "f1151e7173e2edfbefb32bbfb753b2a252cd8d26"
  },
  "links": {
    "project": "/projects/78",
    "jobDefinition": "/projects/78/revisions/f1151e7173e2edfbefb32bbfb753b2a252cd8d26/job-definitions/.yamato%2fupm-ci-universal.yml#Universal_Windows64_DX11_editmode_CUSTOM%20REVISION"
  },
  "environmentVariables": [
    { "key": "CUSTOM_REVISION", "value": "ccf51fff0e6a13a6152b2e18f7bb3366046dcaed" }
]
}'''

response = requests.post(url, data=data, headers={'Authorization': 'ApiKey <apikey>'})

if(response.ok):
    print "ok"
else:
    response.raise_for_status()
    print "oh no"