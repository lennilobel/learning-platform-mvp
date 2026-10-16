param location string
param functionAppName string
param functionPlanName string
param functionIdentityName string
param functionStorageAccountName string
param functionDeploymentContainerName string
param functionAppInsightsName string
param serviceBusNamespaceName string
param sqlServerFqdn string
param sqlDatabaseName string

// Azure built-in role definition IDs
var monitoringMetricsPublisherRoleDefinitionId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '3913510d-42f4-4e42-8a64-420c390055eb'
)

var storageBlobDataOwnerRoleDefinitionId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  'b7e6dc6d-f1e8-4753-8033-0f276bb0955b'
)

var storageBlobDataContributorRoleDefinitionId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
)

var serviceBusDataReceiverRoleDefinitionId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0'
)

// Reference the existing Function Application Insights component
resource functionAppInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: functionAppInsightsName
}

// Reference the Service Bus namespace
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2024-01-01' existing = {
  name: serviceBusNamespaceName
}

// Define the Function managed identity
resource functionIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: functionIdentityName
  location: location
}

// Define the Function storage account
resource functionStorageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: functionStorageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
  }
}

// Define the Blob service
resource functionBlobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: functionStorageAccount
  name: 'default'
}

// Define the Flex Consumption deployment package container
resource functionDeploymentContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: functionBlobService
  name: functionDeploymentContainerName
  properties: {
    publicAccess: 'None'
  }
}

// Define the Flex Consumption hosting plan
resource functionPlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: functionPlanName
  location: location
  kind: 'functionapp'
  sku: {
    name: 'FC1'
    tier: 'FlexConsumption'
  }
  properties: {
    reserved: true
  }
}

// Define the Learning Progress Function App
resource functionApp 'Microsoft.Web/sites@2024-04-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp,linux'
  tags: {
    'hidden-link: /app-insights-resource-id': functionAppInsights.id
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${functionIdentity.id}': {}
    }
  }
  properties: {
    serverFarmId: functionPlan.id
    httpsOnly: true
    publicNetworkAccess: 'Enabled'
    functionAppConfig: {
      deployment: {
        storage: {
          type: 'blobContainer'
          value: '${functionStorageAccount.properties.primaryEndpoints.blob}${functionDeploymentContainer.name}'
          authentication: {
            type: 'UserAssignedIdentity'
            userAssignedIdentityResourceId: functionIdentity.id
          }
        }
      }
      runtime: {
        name: 'dotnet-isolated'
        version: '10.0'
      }
      scaleAndConcurrency: {
        instanceMemoryMB: 2048
        maximumInstanceCount: 100
      }
      siteUpdateStrategy: {
        type: 'Recreate'
      }
    }
    siteConfig: {
      alwaysOn: false
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_AUTHENTICATION_STRING'
          value: 'ClientId=${functionIdentity.properties.clientId};Authorization=AAD'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: functionAppInsights.properties.ConnectionString
        }
        {
          name: 'AzureMonitor__ManagedIdentityClientId'
          value: functionIdentity.properties.clientId
        }
        {
          name: 'AzureWebJobsStorage__blobServiceUri'
          value: functionStorageAccount.properties.primaryEndpoints.blob
        }
        {
          name: 'AzureWebJobsStorage__clientId'
          value: functionIdentity.properties.clientId
        }
        {
          name: 'AzureWebJobsStorage__credential'
          value: 'managedidentity'
        }
        {
          name: 'AzureWebJobsStorage__queueServiceUri'
          value: functionStorageAccount.properties.primaryEndpoints.queue
        }
        {
          name: 'AzureWebJobsStorage__tableServiceUri'
          value: functionStorageAccount.properties.primaryEndpoints.table
        }
        {
          name: 'ConnectionStrings__LearningPlatform'
          value: 'Server=tcp:${sqlServerFqdn},1433;Initial Catalog=${sqlDatabaseName};Encrypt=True;TrustServerCertificate=False;Authentication=Active Directory Managed Identity;User ID=${functionIdentity.properties.clientId}'
        }
        {
          name: 'ServiceBusConnection__clientId'
          value: functionIdentity.properties.clientId
        }
        {
          name: 'ServiceBusConnection__credential'
          value: 'managedidentity'
        }
        {
          name: 'ServiceBusConnection__fullyQualifiedNamespace'
          value: '${serviceBusNamespaceName}.servicebus.windows.net'
        }
      ]
    }
  }
}

// Grant the Function identity permission to publish Application Insights metrics
resource functionMonitoringMetricsPublisherRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: '56719b58-927c-51f7-9709-a8ee98be9367'
  scope: functionAppInsights
  properties: {
    principalId: functionIdentity.properties.principalId
    roleDefinitionId: monitoringMetricsPublisherRoleDefinitionId
    principalType: 'ServicePrincipal'
  }
}

// Grant the Function identity access to Function storage
resource functionStorageBlobDataOwnerRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: '77c9bfcf-2c95-53f7-a288-26b07e7b8b98'
  scope: functionStorageAccount
  properties: {
    principalId: functionIdentity.properties.principalId
    roleDefinitionId: storageBlobDataOwnerRoleDefinitionId
    principalType: 'ServicePrincipal'
  }
}

// Grant the Function identity access to the deployment package container
resource functionDeploymentContainerRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: 'ddcaa4ae-9790-5132-8dc8-bda8f796491e'
  scope: functionDeploymentContainer
  properties: {
    principalId: functionIdentity.properties.principalId
    roleDefinitionId: storageBlobDataContributorRoleDefinitionId
    principalType: 'ServicePrincipal'
  }
}

// Grant the Function identity permission to receive Service Bus messages
resource functionServiceBusReceiverRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: 'e9d049ab-3dfe-439f-bcf8-a6cd62812e67'
  scope: serviceBusNamespace
  properties: {
    principalId: functionIdentity.properties.principalId
    roleDefinitionId: serviceBusDataReceiverRoleDefinitionId
    principalType: 'ServicePrincipal'
  }
}

// Return values needed by the deployment
output functionAppId string = functionApp.id
output functionIdentityClientId string = functionIdentity.properties.clientId
output functionIdentityPrincipalId string = functionIdentity.properties.principalId
