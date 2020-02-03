# ITLab-Salary
Service for storing salary

Status | master | develop
---|---|---
build | [![Build Status](https://dev.azure.com/rtuitlab/RTU%20IT%20Lab/_apis/build/status/ITLab-Salary?branchName=master)](https://dev.azure.com/rtuitlab/RTU%20IT%20Lab/_build/latest?definitionId=88&branchName=master) | [![Build Status](https://dev.azure.com/rtuitlab/RTU%20IT%20Lab/_apis/build/status/ITLab-Salary?branchName=develop)](https://dev.azure.com/rtuitlab/RTU%20IT%20Lab/_build/latest?definitionId=88&branchName=develop)
test | [![master tests](https://img.shields.io/azure-devops/tests/RTUITLab/RTU%20IT%20Lab/88/master?label=%20&style=plastic)](https://dev.azure.com/rtuitlab/RTU%20IT%20Lab/_build/latest?definitionId=88&branchName=master) | [![develop tests](https://img.shields.io/azure-devops/tests/RTUITLab/RTU%20IT%20Lab/88/develop?label=%20&style=plastic)](https://dev.azure.com/rtuitlab/RTU%20IT%20Lab/_build/latest?definitionId=88&branchName=develop)

## Requirements

* Net Core 3.1
* Mongo DB

## Configuration

Example configuration file

```jsonc
{
  "ConnectionStrings": {
    "MongoDb": "mongodb://localhost:27017/itlabsalarydevelop2" // connection string to mongodb
  },
  "JwtOptions": { // Options to configure JWT
    "Authority": "https://somesite.net", // Authority to use when making OpenIdConnect calls.
    "Audience": "itlab.salary", // Single valid audience value for any received OpenIdConnect token
    "DebugKey": "abcdefg123456gfedcba654321", // Key for signing debug tokens
    "DebugAdminUserId": "E38C35C2-18DA-4156-9FEA-B8549C736AB5" // Tests admin user id (GUID)

  },
  "TESTS": true|false, // If TESTS mode enabled
}
```

## Tests mode

In **TESTS** mode app generates test bearer token for requests and writes it to logs in category _ITLab.Salary.Backend.Services.Configure.ShowTestAdminTokenWork_

That mode used in e2e tests and can be helpful to develop.

## End-to-End Testing

### 1. With UI

1. Make sure that option _TESTS_ is set to _true_

2. Run application

```bash
cd src/Backend
dotnet run
```

3. Run [TestMace](https://testmace.com/) and open project _tests/e2e/TestMace/Project_

4. Select environment _localEnv_

5. Run root node _Project_

### 2. With CLI in docker

1. Publish project to special folder

```bash
dotnet publish -c Release -o .\tests\e2e\salary-api\itlab-salary-build\ .\src\Backend\Backend.csproj
```

2. Run tests using docker-compose

```bash
cd tests/e2e
docker-compose up testmace
```

3. Stop and remove started containers

```bash
docker-compose rm -fs
```

4. JUnit results will be available in _tests/e2e/out_
