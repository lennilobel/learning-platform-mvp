param location string
param serviceBusNamespaceName string
param serviceBusTopicName string
param serviceBusSubscriptionName string

// Define the Azure Service Bus namespace
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2024-01-01' = {
  name: serviceBusNamespaceName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
}

// Define the Service Bus topic within the namespace
resource serviceBusTopic 'Microsoft.ServiceBus/namespaces/topics@2024-01-01' = {
  parent: serviceBusNamespace
  name: serviceBusTopicName
  properties: {
    autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
    defaultMessageTimeToLive: 'P14D'
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    enableBatchedOperations: true
    enableExpress: false
    enablePartitioning: false
    maxMessageSizeInKilobytes: 256
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: true
    supportOrdering: false
  }
}

// Define the Service Bus subscription within the topic
resource serviceBusSubscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2024-01-01' = {
  parent: serviceBusTopic
  name: serviceBusSubscriptionName
  properties: {
    autoDeleteOnIdle: 'P10675198DT2H48M5.477S'
    deadLetteringOnFilterEvaluationExceptions: false
    deadLetteringOnMessageExpiration: true
    defaultMessageTimeToLive: 'P14D'
    enableBatchedOperations: true
    isClientAffine: false
    lockDuration: 'PT1M'
  }
}

// Return values needed by other modules
output namespaceId string = serviceBusNamespace.id
output namespaceName string = serviceBusNamespace.name
output fullyQualifiedNamespace string = '${serviceBusNamespace.name}.servicebus.windows.net'
output topicId string = serviceBusTopic.id
output topicName string = serviceBusTopic.name
output subscriptionName string = serviceBusSubscription.name
