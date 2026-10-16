param location string
param apiIdentityName string
param outboxIdentityName string
param progressIdentityName string
param githubActionsIdentityName string
param aksOidcIssuer string
param aksName string
param serviceBusNamespaceName string
param serviceBusTopicName string
param kubernetesNamespace string
param apiServiceAccountName string
param outboxServiceAccountName string
param progressServiceAccountName string
param githubOidcIssuer string
param githubOidcSubject string

// Azure built-in role definition IDs
var serviceBusDataSenderRoleDefinitionId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39'
)

var serviceBusDataReceiverRoleDefinitionId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0'
)

var aksClusterUserRoleDefinitionId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '4abbcc35-e782-43d8-92c5-2d3f1bd2253f'
)

// Reference resources used as RBAC scopes
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2024-01-01' existing = {
  name: serviceBusNamespaceName
}

resource serviceBusTopic 'Microsoft.ServiceBus/namespaces/topics@2024-01-01' existing = {
  parent: serviceBusNamespace
  name: serviceBusTopicName
}

resource aks 'Microsoft.ContainerService/managedClusters@2026-05-01' existing = {
  name: aksName
}

// Define the managed identity used by the API workload
resource apiIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: apiIdentityName
  location: location
}

// Define the managed identity used by the Outbox workload
resource outboxIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: outboxIdentityName
  location: location
}

// Define the managed identity used by the Learning Progress workload
resource progressIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: progressIdentityName
  location: location
}

// Define the managed identity used by GitHub Actions
resource githubActionsIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: githubActionsIdentityName
  location: location
}

// Define the federated identity credential for the API workload
resource apiFederatedCredential 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2023-01-31' = {
  parent: apiIdentity
  name: 'fic-learningplatform-api'
  properties: {
    issuer: aksOidcIssuer
    subject: 'system:serviceaccount:${kubernetesNamespace}:${apiServiceAccountName}'
    audiences: [
      'api://AzureADTokenExchange'
    ]
  }
}

// Define the federated identity credential for the Outbox workload
resource outboxFederatedCredential 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2023-01-31' = {
  parent: outboxIdentity
  name: 'fic-learningplatform-outbox'
  properties: {
    issuer: aksOidcIssuer
    subject: 'system:serviceaccount:${kubernetesNamespace}:${outboxServiceAccountName}'
    audiences: [
      'api://AzureADTokenExchange'
    ]
  }
}

// Define the federated identity credential for the Learning Progress workload
resource progressFederatedCredential 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2023-01-31' = {
  parent: progressIdentity
  name: 'fic-learningplatform-progress'
  properties: {
    issuer: aksOidcIssuer
    subject: 'system:serviceaccount:${kubernetesNamespace}:${progressServiceAccountName}'
    audiences: [
      'api://AzureADTokenExchange'
    ]
  }
}

// Define the federated identity credential used by GitHub Actions
resource githubActionsFederatedCredential 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2023-01-31' = {
  parent: githubActionsIdentity
  name: 'fic-github-main'
  properties: {
    issuer: githubOidcIssuer
    subject: githubOidcSubject
    audiences: [
      'api://AzureADTokenExchange'
    ]
  }
}

// Grant the Outbox workload permission to send messages to the Service Bus topic
resource outboxServiceBusSenderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: 'fb4c8826-1a0b-4f7c-911f-f1a472b48e75'
  scope: serviceBusTopic
  properties: {
    principalId: outboxIdentity.properties.principalId
    roleDefinitionId: serviceBusDataSenderRoleDefinitionId
    principalType: 'ServicePrincipal'
  }
}

// Grant the Learning Progress workload permission to receive messages from the Service Bus topic
resource progressServiceBusReceiverRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: '38717f05-9ae7-4012-b7c1-8dc91944cea0'
  scope: serviceBusTopic
  properties: {
    principalId: progressIdentity.properties.principalId
    roleDefinitionId: serviceBusDataReceiverRoleDefinitionId
    principalType: 'ServicePrincipal'
  }
}

// Grant GitHub Actions permission to obtain AKS cluster credentials
resource githubActionsAksUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: 'ecbee97f-8511-491f-970f-7132a90d2178'
  scope: aks
  properties: {
    principalId: githubActionsIdentity.properties.principalId
    roleDefinitionId: aksClusterUserRoleDefinitionId
    principalType: 'ServicePrincipal'
  }
}

// Return values needed by other modules
output apiClientId string = apiIdentity.properties.clientId
output apiPrincipalId string = apiIdentity.properties.principalId
output outboxClientId string = outboxIdentity.properties.clientId
output outboxPrincipalId string = outboxIdentity.properties.principalId
output progressClientId string = progressIdentity.properties.clientId
output progressPrincipalId string = progressIdentity.properties.principalId
output githubActionsClientId string = githubActionsIdentity.properties.clientId
output githubActionsPrincipalId string = githubActionsIdentity.properties.principalId
