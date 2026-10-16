param location string
param acrName string
param githubActionsPrincipalId string
param aksKubeletPrincipalId string

// Azure built-in role definition IDs
var acrPushRoleDefinitionId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '8311e382-0749-4cb8-b61a-304f252e45ec'
)

var acrConfigurationReaderRoleDefinitionId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '69b07be0-09bf-439a-b9a6-e73de851bd59'
)

var acrPullRoleDefinitionId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '7f951dda-4ed3-4680-a7ca-43fe172d538d'
)

// Define the Azure Container Registry
resource acr 'Microsoft.ContainerRegistry/registries@2025-04-01' = {
  name: acrName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    dataEndpointEnabled: false
    encryption: {
      status: 'disabled'
    }
  }
}

// Grant GitHub Actions permission to push container images to ACR
resource githubActionsAcrPushRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: 'b8daecd6-828f-4d8c-bf15-b127e2e6692c'
  scope: acr
  properties: {
    principalId: githubActionsPrincipalId
    roleDefinitionId: acrPushRoleDefinitionId
    principalType: 'ServicePrincipal'
  }
}

// Grant GitHub Actions permission to read ACR configuration and data-access configuration
resource githubActionsAcrReaderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: '2e50a2f9-f875-402d-a9b8-f32ee1603ad4'
  scope: acr
  properties: {
    principalId: githubActionsPrincipalId
    roleDefinitionId: acrConfigurationReaderRoleDefinitionId
    principalType: 'ServicePrincipal'
  }
}

// Grant the AKS kubelet identity permission to pull container images from ACR
resource aksAcrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: 'b8a368ba-ae07-4ed7-8ab2-0750fa4a7e8d'
  scope: acr
  properties: {
    principalId: aksKubeletPrincipalId
    roleDefinitionId: acrPullRoleDefinitionId
    principalType: 'ServicePrincipal'
  }
}

// Return values needed by other modules
output acrId string = acr.id
output registryName string = acr.name
output acrLoginServer string = acr.properties.loginServer
