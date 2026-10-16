param location string
param logAnalyticsWorkspaceName string
param containerAppsEnvironmentName string
param apiContainerAppName string
param outboxContainerAppName string
param progressContainerAppName string
param acrLoginServer string
param serviceBusNamespaceName string
param serviceBusTopicName string
param serviceBusSubscriptionName string
param entraClientId string
@secure()
param learningPlatformConnectionString string

// Azure built-in role definition IDs
var serviceBusDataSenderRoleDefinitionId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39'
)

var serviceBusDataReceiverRoleDefinitionId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0'
)

// Reference Service Bus resources used as RBAC scopes
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2024-01-01' existing = {
  name: serviceBusNamespaceName
}

resource serviceBusTopic 'Microsoft.ServiceBus/namespaces/topics@2024-01-01' existing = {
  parent: serviceBusNamespace
  name: serviceBusTopicName
}

resource serviceBusSubscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2024-01-01' existing = {
  parent: serviceBusTopic
  name: serviceBusSubscriptionName
}

// Define the Log Analytics workspace
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// Define the Container Apps environment
resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2025-01-01' = {
  name: containerAppsEnvironmentName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspace.properties.customerId
        sharedKey: logAnalyticsWorkspace.listKeys().primarySharedKey
      }
    }
  }
}

// Define the API Container App
resource apiContainerApp 'Microsoft.App/containerApps@2025-01-01' = {
  name: apiContainerAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    environmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      maxInactiveRevisions: 100
      registries: [
        {
          server: acrLoginServer
          identity: 'system-environment'
        }
      ]
      ingress: {
        external: true
        targetPort: 8080
        transport: 'Auto'
        allowInsecure: false
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
        stickySessions: {
          affinity: 'none'
        }
      }
    }
    template: {
      containers: [
        {
          name: apiContainerAppName
          image: '${acrLoginServer}/learning-platform-api-only:phase13'
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Development'
            }
            {
              name: 'DOTNET_ENVIRONMENT'
              value: 'Development'
            }
            {
              name: 'ServiceBus__FullyQualifiedNamespace'
              value: '${serviceBusNamespaceName}.servicebus.windows.net'
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
              name: 'Entra__ClientId'
              value: entraClientId
            }
            {
              name: 'ConnectionStrings__LearningPlatform'
              value: learningPlatformConnectionString
            }
          ]
          probes: [
            {
              type: 'Liveness'
              tcpSocket: {
                port: 8080
              }
              failureThreshold: 3
              initialDelaySeconds: 0
              periodSeconds: 10
              successThreshold: 1
              timeoutSeconds: 5
            }
            {
              type: 'Readiness'
              tcpSocket: {
                port: 8080
              }
              failureThreshold: 48
              initialDelaySeconds: 0
              periodSeconds: 5
              successThreshold: 1
              timeoutSeconds: 5
            }
            {
              type: 'Startup'
              tcpSocket: {
                port: 8080
              }
              failureThreshold: 240
              initialDelaySeconds: 1
              periodSeconds: 1
              successThreshold: 1
              timeoutSeconds: 3
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 10
      }
    }
  }
}

// Define the Outbox Worker Container App
resource outboxContainerApp 'Microsoft.App/containerApps@2025-01-01' = {
  name: outboxContainerAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    environmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      maxInactiveRevisions: 100
      registries: [
        {
          server: acrLoginServer
          identity: 'system-environment'
        }
      ]
    }
    template: {
      containers: [
        {
          name: outboxContainerAppName
          image: '${acrLoginServer}/learning-platform-outbox-worker:phase13'
          env: [
            {
              name: 'ServiceBus__FullyQualifiedNamespace'
              value: '${serviceBusNamespaceName}.servicebus.windows.net'
            }
            {
              name: 'ServiceBus__EnrollmentCreatedTopic'
              value: serviceBusTopicName
            }
            {
              name: 'DOTNET_ENVIRONMENT'
              value: 'Development'
            }
            {
              name: 'ConnectionStrings__LearningPlatform'
              value: learningPlatformConnectionString
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}

// Define the Learning Progress Worker Container App
resource progressContainerApp 'Microsoft.App/containerApps@2025-01-01' = {
  name: progressContainerAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    environmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      maxInactiveRevisions: 100
      registries: [
        {
          server: acrLoginServer
          identity: 'system-environment'
        }
      ]
    }
    template: {
      containers: [
        {
          name: progressContainerAppName
          image: '${acrLoginServer}/learning-platform-learning-progress-worker:phase13'
          env: [
            {
              name: 'DOTNET_ENVIRONMENT'
              value: 'Development'
            }
            {
              name: 'ConnectionStrings__LearningPlatform'
              value: learningPlatformConnectionString
            }
            {
              name: 'ServiceBus__FullyQualifiedNamespace'
              value: '${serviceBusNamespaceName}.servicebus.windows.net'
            }
            {
              name: 'ServiceBus__EnrollmentCreatedTopic'
              value: serviceBusTopicName
            }
            {
              name: 'ServiceBus__LearningProgressSubscription'
              value: serviceBusSubscriptionName
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}

// Grant the Outbox worker permission to publish Service Bus messages
resource outboxServiceBusSenderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: '88688b58-b3b8-433a-806f-4fb33f0328f2'
  scope: serviceBusTopic
  properties: {
    principalId: outboxContainerApp.identity.principalId
    roleDefinitionId: serviceBusDataSenderRoleDefinitionId
    principalType: 'ServicePrincipal'
  }
}

// Grant the Learning Progress worker permission to receive Service Bus messages
resource progressServiceBusReceiverRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: '5a7ed2b7-8976-4e2c-9051-5ba68b7ced5b'
  scope: serviceBusSubscription
  properties: {
    principalId: progressContainerApp.identity.principalId
    roleDefinitionId: serviceBusDataReceiverRoleDefinitionId
    principalType: 'ServicePrincipal'
  }
}

// Return values needed by the deployment
output environmentId string = containerAppsEnvironment.id
output apiContainerAppId string = apiContainerApp.id
output outboxContainerAppId string = outboxContainerApp.id
output progressContainerAppId string = progressContainerApp.id
