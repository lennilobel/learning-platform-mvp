param location string
param appServicePlanName string
param webAppName string
param appInsightsName string
param entraClientId string
@secure()
param learningPlatformConnectionString string
param appInsightsWorkspaceResourceGroupName string
param appInsightsWorkspaceName string
param serviceBusNamespaceName string
param serviceBusTopicName string
param serviceBusSubscriptionName string

// Azure built-in role definition IDs
var serviceBusDataSenderRoleDefinitionId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39'
)

var serviceBusDataReceiverRoleDefinitionId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0'
)

// Reference the existing Log Analytics workspace used by Application Insights
resource appInsightsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' existing = {
  scope: resourceGroup(subscription().subscriptionId, appInsightsWorkspaceResourceGroupName)
  name: appInsightsWorkspaceName
}

// Reference the Service Bus namespace used as the RBAC scope
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2024-01-01' existing = {
  name: serviceBusNamespaceName
}

// Define Application Insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: appInsightsWorkspace.id
  }
}

// Define the Linux App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: appServicePlanName
  location: location
  kind: 'linux'
  sku: {
    name: 'B1'
    tier: 'Basic'
    size: 'B1'
    capacity: 1
  }
  properties: {
    reserved: true
  }
}

// Define the Learning Platform Web App
resource webApp 'Microsoft.Web/sites@2024-04-01' = {
  name: webAppName
  location: location
  kind: 'app,linux'
  tags: {
    'hidden-link: /app-insights-resource-id': appInsights.id
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    publicNetworkAccess: 'Enabled'
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: true
      ftpsState: 'FtpsOnly'
      http20Enabled: false
      minTlsVersion: '1.2'
      webSocketsEnabled: false
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'ConnectionStrings__LearningPlatform'
          value: learningPlatformConnectionString
        }
        {
          name: 'Entra__ClientId'
          value: entraClientId
        }
        {
          name: 'Entra__Instance'
          value: environment().authentication.loginEndpoint
        }
        {
          name: 'Entra__TenantId'
          value: tenant().tenantId
        }
        {
          name: 'ServiceBus__EnrollmentCreatedTopic'
          value: serviceBusTopicName
        }
        {
          name: 'ServiceBus__FullyQualifiedNamespace'
          value: '${serviceBusNamespaceName}.servicebus.windows.net'
        }
        {
          name: 'ServiceBus__LearningProgressSubscription'
          value: serviceBusSubscriptionName
        }
        {
          name: 'WEBSITE_HTTPLOGGING_RETENTION_DAYS'
          value: '1'
        }
        {
          name: 'XDT_MicrosoftApplicationInsights_Mode'
          value: 'Recommended'
        }
      ]
    }
  }
}

// Grant the App Service permission to send messages through Service Bus
resource webAppServiceBusSenderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: '9e712f95-ce9d-48ac-842f-4322b060dd23'
  scope: serviceBusNamespace
  properties: {
    principalId: webApp.identity.principalId
    roleDefinitionId: serviceBusDataSenderRoleDefinitionId
    principalType: 'ServicePrincipal'
  }
}

// Grant the App Service permission to receive messages through Service Bus
resource webAppServiceBusReceiverRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: '4d887bbf-083f-4568-bad6-2c563d4bcfa2'
  scope: serviceBusNamespace
  properties: {
    principalId: webApp.identity.principalId
    roleDefinitionId: serviceBusDataReceiverRoleDefinitionId
    principalType: 'ServicePrincipal'
  }
}

// Return values needed by other modules
output webAppId string = webApp.id
output siteName string = webApp.name
output webAppPrincipalId string = webApp.identity.principalId
output appInsightsId string = appInsights.id
output appInsightsConnectionString string = appInsights.properties.ConnectionString
