This utility first tries to delete the "temp" folder that is in the same directory.
Then, a git clone is performed, and the specified branch (via microServiceDefs.json) is checked out.
Following that, the appropriate appsettings file is modified with any config overrides, and
then the project is started.

Example microServiceDefs.json.

{
  "microServices": [
    {
      "gitRepo": "https://github.com/Example/example.git",
      "gitRepoBranch": "dev",
      "projectPath": "MySolutionFolder/MyProject.csproj",
      "aspNetCoreEnvironment": "local", 
      "configOverrides": [
      ]
    }
  ],
  "globalConfigOverrides": [
    {
      "propertySelector": "Connection.CatalogSuffix",
      "propertyValue": "",
      "operation": "modify"
    }
    //,{
    //  "propertySelector": "Connection.CatalogSuffix",
    //  "operation": "delete"
    //}
    //,{
    //  "propertySelector": "Connection",
    //  "propertyName": "CatalogSuffix",
    //  "propertyValue": 1,
    //  "operation": "add"
    //}
  ]
}