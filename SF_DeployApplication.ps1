###################################################################################################
##  REF: https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-automate-powershell
###################################################################################################


# Connect to the Service Fabric cluster
Connect-ServiceFabricCluster localhost:19000

# Get all application types registered in the cluster
Get-ServiceFabricApplicationType

# Get all application instances created in the cluster
Get-ServiceFabricApplication

# Get all service instances for each application
Get-ServiceFabricApplication | Get-ServiceFabricService

# Remove an application instance
Remove-ServiceFabricApplication fabric:/Voting

# Unregister the application type
Unregister-ServiceFabricApplicationType VotingType 1.0.0

# Remove the application package
Remove-ServiceFabricApplicationPackage -ImageStoreConnectionString file:C:\SfDevCluster\Data\ImageStoreShare -ApplicationPackagePathInImageStore Voting