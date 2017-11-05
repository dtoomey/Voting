Import-Module "$ENV:ProgramFiles\Microsoft SDKs\Service Fabric\Tools\PSModule\ServiceFabricSDK\ServiceFabricSDK.psm1"

$path = 'C:\Users\Dan\Documents\Visual Studio 2015\Projects\Voting\Voting\pkg\Debug'
$appName = 'Voting'
$appTypeName = 'VotingType'
$appVersion = '1.0.0'
$clusterAddress = 'localhost:19000'


# Connect to the Service Fabric cluster
Connect-ServiceFabricCluster $clusterAddress

# Get all application types registered in the cluster
Get-ServiceFabricApplicationType

# Get all application instances created in the cluster
Get-ServiceFabricApplication

# Get all service instances for each application
Get-ServiceFabricApplication | Get-ServiceFabricService

# Remove an application instance
Remove-ServiceFabricApplication "fabric:/$appName"

# Unregister the application type
Unregister-ServiceFabricApplicationType $appTypeName $appVersion

# Remove the application package
Remove-ServiceFabricApplicationPackage -ImageStoreConnectionString file:C:\SfDevCluster\Data\ImageStoreShare -ApplicationPackagePathInImageStore $appName